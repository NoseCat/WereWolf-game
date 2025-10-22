using Godot;

public partial class LedgeDetector : Node3D
{
    Marker3D Start;
    Marker3D StartLower;
    Marker3D End;
    MeshInstance3D TestMesh;
    MeshInstance3D TestMesh2;
    SlopeDetector SlopeDetector;

    [Export] float DownStep = 0.1f;
    [Export] float Length = 2.5f;
    [Export] float MinLedgeHeight = 0.1f;
    [Export] float MinLedgeLenght = 0.1f;
    [Export] float MinLedgeWidth = 0.1f;
    [Export] float AngleRange = 60.0f;
    [Export] float AngleStep = 5.0f;
    //SideSLope angle = atan(MinLedgeLenght/MinLedgeWidth) or smth similar

    // [Export] float HandWidth = 0.05f;
    // [Export] float HandHeight = 0.01f;
    // [Export] float HandLenght = 0.1f;

    public bool Lower = false;

	public override void _Ready()
	{
        Start = GetNode<Marker3D>("Start");
        StartLower = GetNode<Marker3D>("StartLower");
        End = GetNode<Marker3D>("End");

        SlopeDetector = GetNode<SlopeDetector>("SlopeDetector");
    }

    //returns Global position
    public Vector3 FindLedge()
    {
        var Dir = Vector3.Forward;
        Dir = Dir.Rotated(Vector3.Up, GlobalRotation.Y).Normalized();
        Dir *= Length;

        Vector3 vecMinLedgeHeight = Vector3.Up * MinLedgeHeight;
        bool FlipFlop = false;

        for(float i = Start.Position.Y; i > End.Position.Y; i -= DownStep)
        {
            //include height
            var relVec = new Vector3(Start.Position.X + Mathf.Abs(End.Position.X - Start.Position.X)/2, i, Start.Position.Z);
            for(float angle = 0; angle < AngleRange; angle += AngleStep * (FlipFlop ? 1 : 0))
            {
                //include angle
                Vector3 RotatedDir = Dir.Rotated(Vector3.Up, angle * (Mathf.Pi/180) * (FlipFlop ? -1 : 1));
                FlipFlop = !FlipFlop;
                
                //final relVec Adjustments
                relVec = relVec.Rotated(Vector3.Up, GlobalRotation.Y);

                //get info
                var ColInf = CastRayGetResult(GlobalPosition + relVec, RotatedDir);
                if(ColInf.Count <= 0)
                    continue; 
                var ColVec = (Vector3)ColInf["position"];
                if(ColVec == Vector3.Zero)
                    continue;
                var ColNormal = (Vector3)ColInf["normal"];
                float ColAngle = ColNormal.AngleTo(Vector3.Up);
                float SlopeDecrease = 0;
                if(!(ColAngle < 1.0f * (Mathf.Pi/180.0f) || ColAngle > 89.0f * (Mathf.Pi/180.0f)))
                    SlopeDecrease = MinLedgeHeight / Mathf.Tan(ColAngle);
                float NegColLength = (ColVec - (GlobalPosition + relVec)).Length() + MinLedgeLenght + SlopeDecrease;

                Vector3 NegCol = CastRay(GlobalPosition + relVec + vecMinLedgeHeight, setLenght(RotatedDir, NegColLength));
                if(NegCol != Vector3.Zero)
                    continue;
                Vector3 NegColRLOffset = (ColVec - (GlobalPosition + relVec)).Cross(Vector3.Up);
                NegColRLOffset = NegColRLOffset.Normalized() * (MinLedgeWidth/2.0f);
                Vector3 RNegCol = CastRay(GlobalPosition + relVec + vecMinLedgeHeight + NegColRLOffset, setLenght(RotatedDir, NegColLength));
                Vector3 LNegCol = CastRay(GlobalPosition + relVec + vecMinLedgeHeight - NegColRLOffset, setLenght(RotatedDir, NegColLength));
                Vector3 SlopeNegCol = CastRay(ColVec + vecMinLedgeHeight, -new Vector3(ColNormal.X, 0, ColNormal.Z).Normalized() * (SlopeDecrease + MinLedgeHeight));
                
                //determine ledge                
                if(NegCol == Vector3.Zero && RNegCol == Vector3.Zero && LNegCol == Vector3.Zero && SlopeNegCol == Vector3.Zero)
                {
                    //debug
                    //GD.Print(ColAngle * (180/Mathf.Pi));
                    //GD.Print(SlopeDecrease);
                    //TestMesh.GlobalPosition = ColVec;
                    // rayMesh.DrawRay(GlobalPosition + relVec, ColVec, new Color(0, 0, 1));
                    // rayMesh2.DrawRay(GlobalPosition + relVec + vecMinLedgeHeight, GlobalPosition + relVec + vecMinLedgeHeight + setLenght(RotatedDir, NegColLength), new Color(1, 0, 0));
                    // var rayMesh3 = GetNode<RayMesh>("RayMesh3");
                    // rayMesh3.DrawRay(GlobalPosition + relVec + vecMinLedgeHeight + NegColRLOffset, GlobalPosition + relVec + vecMinLedgeHeight + NegColRLOffset + setLenght(RotatedDir, NegColLength), new Color(1, 0, 0));
                    // var rayMesh4 = GetNode<RayMesh>("RayMesh4");
                    // rayMesh4.DrawRay(ColVec + vecMinLedgeHeight, ColVec + vecMinLedgeHeight - new Vector3(ColNormal.X, 0, ColNormal.Z).Normalized() * (SlopeDecrease + MinLedgeHeight), new Color(0, 1, 0));
                   
                    //done 
                    Lower = i < StartLower.Position.Y;
                    return ColVec;
                }
            }
        }
        return Vector3.Zero;
    }

    public Vector3 addLenght(Vector3 vec, float x)
    {
        return vec.Normalized() * (vec.Length() + x);
    }    
    public Vector3 setLenght(Vector3 vec, float x)
    {
        return vec.Normalized() * x;
    }
    
    public bool CastShapeIsCollide(Vector3 pos, float width, float height, float lenght)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        BoxShape3D boxShape = new BoxShape3D();
        boxShape.Size = new Vector3(width, height, lenght);

        PhysicsShapeQueryParameters3D shapeQuery = new PhysicsShapeQueryParameters3D();
        shapeQuery.SetShape(boxShape);
        shapeQuery.Transform = new Transform3D(Basis.Identity, pos);
        shapeQuery.CollisionMask = 2; 

        var results = spaceState.IntersectShape(shapeQuery, 32); 

        if (results.Count > 0)
        {
           // GD.Print("Collision detected with world geometry!");
            return true;
        }
        else
        {
           // GD.Print("No collision detected.");
            return false;
        }
    }

    //Keep in mind its global
	public Vector3 CastRay(Vector3 rayStart, Vector3 rayDirection)
	{
        Vector3 rayEnd = rayStart + rayDirection;

        var spaceState = GetWorld3D().DirectSpaceState;
		var parameters = new PhysicsRayQueryParameters3D();
        parameters.From = rayStart;
        parameters.To = rayEnd;
        parameters.CollisionMask = 1;
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

    public Godot.Collections.Dictionary CastRayGetResult(Vector3 rayStart, Vector3 rayDirection)
	{
        Vector3 rayEnd = rayStart + rayDirection;

        var spaceState = GetWorld3D().DirectSpaceState;
		var parameters = new PhysicsRayQueryParameters3D();
        parameters.From = rayStart;
        parameters.To = rayEnd;
        parameters.CollisionMask = 1;
        return spaceState.IntersectRay(parameters);
	}

    public Vector3 CastRayGetNormal(Vector3 rayStart, Vector3 rayDirection)
	{
        Vector3 rayEnd = rayStart + rayDirection;

        var spaceState = GetWorld3D().DirectSpaceState;
		var parameters = new PhysicsRayQueryParameters3D();
        parameters.From = rayStart;
        parameters.To = rayEnd;
        parameters.CollisionMask = 1;
        var result = spaceState.IntersectRay(parameters);
		if (result.Count > 0)
        {
            Vector3 collisionNormal = (Vector3)result["normal"];
            return collisionNormal;
        }
        else
            return Vector3.Zero;
	}
}
