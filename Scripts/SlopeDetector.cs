using System.IO;
using Godot;

public partial class SlopeDetector : Node3D
{
	[Export] public float SlopeAngleMargin = 5;
	RayCast3D RayCastNorth;
	RayCast3D RayCastSouth;
	RayCast3D RayCastWest;
	RayCast3D RayCastEast;
	[Export] public float Lenght = 1;
	[Export] public float Width = 0.06f;
	
	public override void _Ready()
	{
		RayCastNorth = GetNode<RayCast3D>("RayCastN");
		RayCastNorth.TargetPosition = new Vector3(0, Lenght, 0);
		RayCastNorth.Position = new Vector3(0, 0, -Width / 2);

		RayCastSouth = GetNode<RayCast3D>("RayCastS");
		RayCastSouth.TargetPosition = new Vector3(0, Lenght, 0);
		RayCastSouth.Position = new Vector3(0, 0, Width / 2);
		
		RayCastWest = GetNode<RayCast3D>("RayCastW");
		RayCastWest.TargetPosition = new Vector3(0, Lenght, 0);
		RayCastWest.Position = new Vector3(-Width / 2, 0, 0);
		
		RayCastEast = GetNode<RayCast3D>("RayCastE");
		RayCastEast.TargetPosition = new Vector3(0, Lenght, 0);
		RayCastEast.Position = new Vector3(Width / 2, 0, 0);
	}

	public bool IsAllColliding()
	{
		if (!RayCastNorth.IsColliding() || !RayCastSouth.IsColliding()
		|| !RayCastWest.IsColliding() || !RayCastEast.IsColliding())
			return false;
		return true;
	}

	public Vector3 GetNormalVec()
	{
		if (!IsAllColliding())
			return Vector3.Zero;

		var vec = new Vector3(0,1,0);
		vec = vec.Rotated(new Vector3(1,0,0), -GetAngleNorth());
		vec = vec.Rotated(new Vector3(0,0,1), -GetAngleEast());
		vec = vec.Rotated(new Vector3(0,1,0), GlobalRotation.Y);
		return vec.Normalized();
	}

	public Vector3 GetSlope()
	{
		if (!IsAllColliding())
			return Vector3.Zero;

		var vec = GetNormalVec();
		if(Mathf.Abs(vec.Z) > Mathf.Abs(vec.X))
		{
			vec = vec.Rotated(new Vector3(1,0,0), Mathf.Sign(vec.Z) * Mathf.Pi/2);
		}
		else
		{
			vec = vec.Rotated(new Vector3(0,0,1), Mathf.Sign(vec.X) * Mathf.Pi/2);
		}

		return vec.Normalized();
	}

	public float GetAngleNorth()
	{
		if (!IsAllColliding())
			return 0;
		float DistanceN = (RayCastNorth.GetCollisionPoint() - RayCastNorth.GlobalPosition).Length();
		float DistanceS = (RayCastSouth.GetCollisionPoint() - RayCastSouth.GlobalPosition).Length();
		float DistanceY = DistanceN - DistanceS;
		float angleY = Mathf.Atan(DistanceY / Width) + (Rotation.X - Mathf.Sign(Rotation.X) * Mathf.Pi);
		return angleY;
	}

	public float GetAngleEast()
	{
		if(!IsAllColliding())
			return 0;
		float DistanceW = (RayCastWest.GetCollisionPoint() - RayCastWest.GlobalPosition).Length();
		float DistanceE = (RayCastEast.GetCollisionPoint() - RayCastEast.GlobalPosition).Length();
		float DistanceX = DistanceW - DistanceE;
		float angleX = Mathf.Atan(DistanceX / Width) + (Rotation.Y - Mathf.Sign(Rotation.Y) * Mathf.Pi);
		return angleX;
	}

	public float GetAngle()
	{
		var angleX = GetAngleNorth();
		var angleY = GetAngleEast();
		return Mathf.Sqrt(angleX * angleX + angleY * angleY);
	}

	public bool IsSlope()
	{
		if(!IsAllColliding())
			return false;

		float angle = GetAngle();
		if (angle < SlopeAngleMargin * (Mathf.Pi / 180) 
		|| angle > (90 - SlopeAngleMargin) * (Mathf.Pi / 180))
			return false;
		return true;
	}

	public bool CastRayIsAllColliding(Vector3 N, Vector3 S)
	{
		return !(N == Vector3.Zero || S == Vector3.Zero);
	}

	public bool CastRayIsAllColliding(Vector3 N, Vector3 S, Vector3 W, Vector3 E)
	{
		return !(N == Vector3.Zero || S == Vector3.Zero || W == Vector3.Zero || E == Vector3.Zero);
	}

	//Dir will be normalized and multiplied br Lenght
	//Keep in mind its global
	public float CastRayGetAngle(Vector3 Pos, Vector3 Dir)
	{
		Dir = Dir.Normalized();
		
		Vector3 DirN;
		if (Dir.Cross(Vector3.Right).Length() < 0.0001)  
			DirN = Dir.Cross(Vector3.Forward).Normalized();
		else
			DirN = Dir.Cross(Vector3.Right).Normalized();
		var DirE = Dir.Cross(DirN).Normalized(); 

		var N = CastRay(Pos + DirN * (Width/2), Dir * Lenght);
		var S = CastRay(Pos - DirN * (Width/2), Dir * Lenght);
		var E = CastRay(Pos + DirE * (Width/2), Dir * Lenght);
		var W = CastRay(Pos - DirE * (Width/2), Dir * Lenght);
		if (!CastRayIsAllColliding(N,S,E,W))
			return 0;

		float DistanceN = (N - Pos).Length();
		float DistanceS = (S - Pos).Length();
		float DistanceY = DistanceN - DistanceS;
		float angleY = Mathf.Atan(DistanceY / Width);

		float DistanceE = (E - Pos).Length();
		float DistanceW = (W - Pos).Length();
		float DistanceX = DistanceW - DistanceE;
		float angleX = Mathf.Atan(DistanceX / Width);

		return Mathf.Sqrt(angleX * angleX + angleY * angleY);
	}

	//Assuming we are flat (Dir.Y == 0)
	public float CastRayGetAngleNorth(Vector3 Pos, Vector3 Dir)
	{
		Dir = Dir.Normalized();

		var N = CastRay(Pos + Vector3.Up * (Width/2), Dir * Lenght);
		var S = CastRay(Pos - Vector3.Up * (Width/2), Dir * Lenght);
		if (!CastRayIsAllColliding(N,S))
			return 0;

		float DistanceN = (N - Pos).Length();
		float DistanceS = (S - Pos).Length();
		float DistanceY = DistanceN - DistanceS;
		return Mathf.Atan(DistanceY / Width);
	}

	public bool CastRayIsSlopeNorth(Vector3 Pos, Vector3 Dir)
	{
		var angle = CastRayGetAngleNorth(Pos, Dir); 
		if(angle > SlopeAngleMargin* (Mathf.Pi / 180) && angle < (90 - SlopeAngleMargin) * (Mathf.Pi / 180))
		{
			return true;
		}
		return false;
	}

	public Vector3 CastRay(Vector3 rayStart, Vector3 rayDirection)
	{
        Vector3 rayEnd = rayStart + rayDirection;

        var spaceState = GetWorld3D().DirectSpaceState;
		var parameters = new PhysicsRayQueryParameters3D();
        parameters.From = rayStart;
        parameters.To = rayEnd;
        parameters.CollisionMask = 2;
        var result = spaceState.IntersectRay(parameters);
		if (result.Count > 0)
        {
            Vector3 collisionPoint = (Vector3)result["position"];

            //var collider = result["collider"];
            //GD.Print(collider);
            return collisionPoint;
            //GD.Print("Ray hit point: ", collisionPoint);
            //GD.Print("Collider: ", collider);
        }
        else
        {
            return Vector3.Zero;
            //GD.Print("No hit detected");
        }
	}

	public void Print()
	{
		GD.Print(Lenght);
		//GD.Print(GetAngle() * 180 / Mathf.Pi);
		//GD.Print(GetRelativeSlope());
		//GD.Print(GetNormalVec());
		// GD.Print(GetAngleNorth() * 180 / Mathf.Pi);
		// GD.Print(GetAngleEast() * 180 / Mathf.Pi);
		
		//GD.Print(GetSlope());
		
		// if (IsAllColliding())
		// 	GD.Print(AngleEast() * 180 / Mathf.Pi);

		//float DistanceN = RayCastNorth.GetCollisionPoint().Y - RayCastNorth.GlobalPosition.Y;
		//GD.Print(DistanceN);
		//GD.Print(RayCastNorth.GetCollider());
		//GD.Print(RayCastSouth.GetCollider());
	}

	// public override void _Process(double delta)
	// {
	// }
}
