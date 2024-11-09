using System;
using Godot;

public partial class CharacterBody3d : CharacterBody3D
{
	//Debug
	MeshInstance3D TestMesh;

	//General
	CollisionShape3D Body;
	CapsuleShape3D BodyCol;
	Marker3D HeadCenterMarker;
	Marker3D LookAtMarker;
	public const float CrouchHeight = 1.0f;
	public float StandHeight;
	public float CurStandHeight;
	public const float ChargeStandDifHeight = 0.5f;
	public const float HeightStep = 0.1f;
	public bool Crouch = false;
	float velYbypass = 0.0f;

	//Walking
	public const float Friction = 17.0f;
	public const float MinSpeed = 400.0f;
	public float MaxSpeed = 1000.0f;
	public const float RegularMaxSpeed = 1000.0f;
	public const float ChargeMaxSpeed = 600.0f;
	public const float HighSpeed = 2000.0f;
	public const float HighSpeedFriction = 4.0f;
	public float Accel = 7.0f;
	public const float RegularAccel = 7.0f;
	public const float ChargeAccel = 5.0f;
	public bool walk = false;

	//Jumping base
	public const float AirMaxSpeed = 1500.0f;
	public const float AirControl = 60.0f;
	public const float JumpForce = 15.0f;
	public const float LeapForce = 5.0f;
	public const float Weight = 2.0f;
	public const float LowJumpDegree = 15.0f;
	public const float BasicJumpLeapMod = 1.2f;
	public const float BasicJumpJumpMod = 1f;
	public Timer PressSpaceTimer;
	public Timer JumpCoolDown;
	public Timer CoyoteeTimer;
	public bool coyoteeOnFloorCheck = true;

	//KingJump
	public float KingJumpRatio = 0f;
	public float KingJumpRatioGrowthSpeed = 1.1f;
	public float MaxKingJumpRatio = 0.9f;
	public float MinKingJumpRatio = 0.2f;
	public const float KingJumpLeapMod = 2.8f;
	public const float KingJumpJumpMod = 0.75f;

	//Ledge Grapple
	RayCast3D RayCastHighGrapleSpaceCheck;
	LedgeDetector LedgeDetector;
	public const float HighGrappleLeapMod = 1.0f;
	public const float HighGrappleJumpMod = 0.75f;
	public const float LowGrappleLeapMod = 1.0f;
	public const float LowGrappleJumpMod = 0.5f;
	Marker3D HighGrappleJumpMarker;
	Marker3D LowGrappleJumpMarker;
	Timer GrappleSecondJumpTimer;
	Timer LedgeGrappleCoolDown;
	public const float GrappleSecondJumpLeapMod = 2.5f;
	public const float GrappleSecondJumpJumpMod = 0.5f;

	// Slopes
	RayCast3D RayCastSlopeDetector;
	RayCast3D RayCastSlopeDetector1;
	RayCast3D RayCastSlopeDetector2;
	RayCast3D RayCastSlopeDetector3;
	RayCast3D RayCastSlopeDetector4;
	public const float SlopeSlideMagnetism = 3.0f;
	public const float SlopeAddAccel = 10.0f;
	public const float SlopeMargin = 5.0f;
	public const float SlideFriction = 10f;
	public bool IsSliding = false;
	public const float SlopeJumpLookToNormalRatio = 1;
	public const float SlopeJumpLeapMod = 1f;
	public const float SlopeJumpJumpMod = 1f;

	//Wall Jump
	bool WallSlide = false;
	const float WallWeight = 0.5f;

	// TO DO:

	// Wall sliding & jumping (maybe no)

	// you can grapple on accident while holding space for king jump

	// when sliding off slope there needs to be coyotee
	// Sliding accumulates speed when jumping

	// Check for wall walking
	// bug with high slope (maybe check velY.Slide)
	// bug with stoping before slopes after high slopes

	// Shift release do timer

	public override void _Ready()
	{
		//Debug
		TestMesh = GetNode<MeshInstance3D>("TestMesh");

		//General
		BodyCol = (CapsuleShape3D)GetNode<CollisionShape3D>("CollisionShape3D").Shape;
		StandHeight = BodyCol.Height;
		LookAtMarker = GetNode<Marker3D>("Camera3D/LookAtMarker");

		//Jumping QoL
		PressSpaceTimer = GetNode<Timer>("PressSpaceTimer");
		CoyoteeTimer = GetNode<Timer>("CoyoteeTimer");
		JumpCoolDown = GetNode<Timer>("JumpCoolDown");
		HeadCenterMarker = GetNode<Marker3D>("HeadCenterMarker");

		//GroundJump
		//Marker3D GroundJumpBaseMarker = GetNode<Marker3D>("HeadCenterMarker/GroundJumpBaseMarker");
		//GroundJumpBaseMarker.RotationDegrees = new Vector3(LowJumpDegree, 0, 0);

		//Ledge Grapple
		RayCastHighGrapleSpaceCheck = GetNode<RayCast3D>("HeadCenterMarker/RayCastHighGrapleSpaceCheck");
		HighGrappleJumpMarker = GetNode<Marker3D>("HeadCenterMarker/HighGrappleJumpMarker");
		LowGrappleJumpMarker = GetNode<Marker3D>("HeadCenterMarker/LowGrappleJumpMarker");
		GrappleSecondJumpTimer = GetNode<Timer>("GrappleSecondJumpTimer");
		LedgeGrappleCoolDown = GetNode<Timer>("LedgeGrappleCoolDown");
		LedgeDetector = GetNode<LedgeDetector>("HeadCenterMarker/LedgeDetector");

		//Slopes
		RayCastSlopeDetector = GetNode<RayCast3D>("HeadCenterMarker/RayCastSlopeDetector");
		RayCastSlopeDetector1 = GetNode<RayCast3D>("HeadCenterMarker/RayCastSlopeDetector1");
		RayCastSlopeDetector2 = GetNode<RayCast3D>("HeadCenterMarker/RayCastSlopeDetector2");
		RayCastSlopeDetector3 = GetNode<RayCast3D>("HeadCenterMarker/RayCastSlopeDetector3");
		RayCastSlopeDetector4 = GetNode<RayCast3D>("HeadCenterMarker/RayCastSlopeDetector4");
	}

	public override void _PhysicsProcess(double delta)
	{
		HeadCenterMarker.RotationDegrees = new Vector3(0, GetNode<Camera3D>("Camera3D").RotationDegrees.Y, 0);

		Vector3 velocity = Velocity;
		if (velocity.Y != 0.0f)
			velYbypass = velocity.Y; //set to 0 after using

		if (Input.IsActionJustPressed("Alt"))
			walk = !walk;
		if (Input.IsActionPressed("Shift"))
		{
			MaxSpeed = ChargeMaxSpeed - 500 * KingJumpRatio;
			Accel = ChargeAccel;
		}
		else
		{
			MaxSpeed = RegularMaxSpeed;
			Accel = RegularAccel;
		}

		Vector2 inputDir = Input.GetVector("A", "D", "W", "S");
		inputDir = inputDir.Rotated(-GetNode<Camera3D>("Camera3D").Rotation.Y).Normalized();
		Vector2 velocity2 = new Vector2(velocity.X, velocity.Z);
		if (!Crouch)
		{
			if (IsOnFloor())//&& !IsSliding)
			{
				if (inputDir != Vector2.Zero)
				{
					if (!walk && Input.IsActionPressed("W"))
					{
						Sprint(ref velocity2, inputDir, (float)delta);
					}
					else
					{
						Walk(ref velocity2, inputDir, (float)delta);
					}
				}
				else
				{
					Slowdown(ref velocity2, (float)delta);
				}
			}
			else
			{
				Fly(ref velocity2, inputDir, (float)delta);
			}
		}

		if(Crouch && !IsOnFloor() && !IsSliding)
			Fly(ref velocity2, inputDir, (float)delta);
		
		velocity.X = velocity2.X;
		velocity.Z = velocity2.Y;

		// Add the gravity.
		if (!IsOnFloor() && !IsSliding)
			velocity += Weight * GetGravity() * (float)delta;

		// Handle Jump.
		if (!IsOnFloor() && coyoteeOnFloorCheck)
		{
			coyoteeOnFloorCheck = false;
			CoyoteeTimer.Start();
		}
		if (IsOnFloor())
			coyoteeOnFloorCheck = true;

		CurStandHeight = StandHeight - (ChargeStandDifHeight * KingJumpRatio);
		if (Input.IsActionPressed("Shift"))
		{
			//KingJump
			if (Input.IsActionJustReleased("Space") && !JumpCoolDown.IsStopped())
				PressSpaceTimer.Start();
			if ((Input.IsActionJustReleased("Space") || !PressSpaceTimer.IsStopped())
			&& (IsOnFloor() || !CoyoteeTimer.IsStopped()) && !IsSliding)
			{
				PressSpaceTimer.Stop();
				JumpCoolDown.Start();
				coyoteeOnFloorCheck = false;
				KingJump(ref velocity, KingJumpRatio, KingJumpLeapMod, KingJumpJumpMod + KingJumpRatio/2f);
			}
			if (Input.IsActionPressed("Space"))
				KingJumpRatio += KingJumpRatioGrowthSpeed * (float)delta;
			else
				KingJumpRatio = 0f;
			KingJumpRatio = Mathf.Clamp(KingJumpRatio, MinKingJumpRatio, MaxKingJumpRatio);
		}
		else
		{
			//BasicJump
			if (Input.IsActionJustPressed("Space") && !JumpCoolDown.IsStopped())
				PressSpaceTimer.Start();
			if ((Input.IsActionJustPressed("Space") || !PressSpaceTimer.IsStopped())
			&& (IsOnFloor() || !CoyoteeTimer.IsStopped()) && !IsSliding)
			{
				PressSpaceTimer.Stop();
				JumpCoolDown.Start();
				coyoteeOnFloorCheck = false;
				var dir = GetNode<Marker3D>("HeadCenterMarker/BasicJumpBase/BasicJump").GlobalPosition - HeadCenterMarker.GlobalPosition;
				StraightJump(ref velocity, dir, BasicJumpLeapMod, BasicJumpJumpMod);
			}
		}

		// if ((Input.IsActionJustPressed("Space") || !PressSpaceTimer.IsStopped())
		// && (IsOnFloor() || !CoyoteeTimer.IsStopped()) && !IsSliding)
		// {
		// 	PressSpaceTimer.Stop();
		// 	JumpCoolDown.Start();
		// 	coyoteeOnFloorCheck = false;
		// 	Jump(ref velocity);
		// }

		// //Wall jump
		// if(IsOnWall() && !IsOnFloor()
		// && GrappleSecondJumpTimer.IsStopped())
		// {
		// 	WallSlide = true;
		// 	velocity += WallWeight * GetGravity() * (float)delta;
		// }
		// else
		// {
		// 	WallSlide = false;
		// }

		// if(WallSlide && (Input.IsActionJustPressed("Space") || !PressSpaceTimer.IsStopped()))
		// {
		// 	var dir = GetNode<Marker3D>("Camera3D/LookAtMarker").GlobalPosition - GlobalPosition;
		// 	var TestMesh = GetNode<MeshInstance3D>("TestMesh");
		// 	TestMesh.LookAt(TestMesh.GlobalPosition + dir);
		// 	StraightJump(ref velocity, dir, 1.0f, 1.0f);
		// }

		//Grapple second Jump
		if (Input.IsActionJustPressed("Space") && !GrappleSecondJumpTimer.IsStopped() && !IsOnFloor())
		{
			//velocity /= 10;
			velocity.Y = 0;
			Jump(ref velocity, GrappleSecondJumpLeapMod, GrappleSecondJumpJumpMod);
			GrappleSecondJumpTimer.Stop();
			LedgeGrappleCoolDown.Start();
		}

		//Grapple
		if ((Input.IsActionJustPressed("Space") || !PressSpaceTimer.IsStopped())
		&& GrappleSecondJumpTimer.IsStopped() && !IsOnFloor() && !IsSliding
		&& LedgeGrappleCoolDown.IsStopped()
		&& !RayCastHighGrapleSpaceCheck.IsColliding()
		&& LedgeDetector.FindLedge() != Vector3.Zero)
		{
			if (!LedgeDetector.Lower)
				HighGrapple(ref velocity);
			else
				LowGrapple(ref velocity);
		}

		//Slope
		if ((IsOnFloor() || IsOnWall() || IsSliding) && Crouch &&
		(RayCastSlopeDetector.IsColliding() ||
		RayCastSlopeDetector1.IsColliding() || RayCastSlopeDetector2.IsColliding() ||
		RayCastSlopeDetector3.IsColliding() || RayCastSlopeDetector4.IsColliding()))
		{
			IsSliding = true;

			//normal & slope angle
			Vector3 normal = Vector3.Zero;
			if (RayCastSlopeDetector.IsColliding())
				normal += RayCastSlopeDetector.GetCollisionNormal();
			if (RayCastSlopeDetector1.IsColliding())
				normal += RayCastSlopeDetector1.GetCollisionNormal();
			if (RayCastSlopeDetector2.IsColliding())
				normal += RayCastSlopeDetector2.GetCollisionNormal();
			if (RayCastSlopeDetector3.IsColliding())
				normal += RayCastSlopeDetector3.GetCollisionNormal();
			if (RayCastSlopeDetector4.IsColliding())
				normal += RayCastSlopeDetector4.GetCollisionNormal();

			normal = normal.Normalized();
			Slide(ref velocity, normal, inputDir, (float)delta);
		}
		else
		{
			IsSliding = false;
			MotionMode = MotionModeEnum.Grounded;
		}

		//Ctrl
		if (Input.IsActionPressed("Ctrl"))
		{
			Crouch = true;
			BodyCol.Height = Mathf.MoveToward(CrouchHeight, CurStandHeight, HeightStep * (float)delta);
			FloorMaxAngle = (90.0f - SlopeMargin) * (Mathf.Pi / 180.0f);
		}
		else
		{
			Crouch = false;
			BodyCol.Height = Mathf.MoveToward(CurStandHeight, CrouchHeight, HeightStep * (float)delta);
			FloorMaxAngle = Mathf.Pi / 4;
		}
		BodyCol.Height = Mathf.Clamp(BodyCol.Height, CrouchHeight, StandHeight);

		//GD.Print(velocity.Length());
		GD.Print(velocity2.Length());
		Velocity = velocity;
		MoveAndSlide();
	}

	public void Slide(ref Vector3 velocity, Vector3 normal, Vector2 inputDir, float delta)
	{
		float angle = normal.AngleTo(Vector3.Up);

		//NormalX & NormalY
		Vector3 normalX;
		if (normal == Vector3.Up)
			normalX = normal.Cross(Vector3.Up + Vector3.Back);
		else
			normalX = normal.Cross(Vector3.Up);
		var normalY = normal.Cross(normalX);

		var InputDir = new Vector3(inputDir.X, 0, inputDir.Y);

		//if not floor (walls are acounted for in crouch (do they?))
		if (angle > SlopeMargin * (Mathf.Pi / 180))
		{
			//when we land on slope
			if (MotionMode == MotionModeEnum.Grounded)
			{
				var velY = new Vector3(0, velYbypass, 0);
				velYbypass = 0;
				velocity.Y = 0;
				velocity = velocity.Slide(normal) + velY.Slide(normal);
			}

			//apply to velocity
			MotionMode = MotionModeEnum.Floating;
			//if (velocity.Length() > SlopeSlideMagnetism * delta)
			//	velocity -= normal * SlopeSlideMagnetism * delta;
			velocity += normalY * SlopeAddAccel * Weight * delta;
			velocity += InputDir.Rotated(normalX.Normalized(), -angle) * Accel * delta;

			//Jump
			if ((Input.IsActionJustPressed("Space") || !PressSpaceTimer.IsStopped()) && JumpCoolDown.IsStopped())
			{
				PressSpaceTimer.Stop();
				JumpCoolDown.Start();
				coyoteeOnFloorCheck = false;
				velocity.Y = 0;
				IsSliding = false;
				MotionMode = MotionModeEnum.Grounded;
				StraightJump(ref velocity,
					(LookAtMarker.GlobalPosition - GlobalPosition).Normalized() * SlopeJumpLookToNormalRatio + normal,
					SlopeJumpLeapMod, SlopeJumpJumpMod);
			}
		}
		else //if floor
		{
			//slowdown
			if (velocity.Length() < SlideFriction * delta)
			{
				velocity = Vector3.Zero;
			}
			else
			{
				velocity = velocity.MoveToward(Vector3.Zero, SlideFriction * delta);
			}
		}

		var velocity22 = new Vector2(velocity.X, velocity.Z);
		if (velocity22.Length() > 30.0f)
		{
			velocity22 = velocity22.Normalized() * 30;
			velocity = new Vector3(velocity22.X, velocity.Y, velocity22.Y);
		}

		//debug
		//TestMesh.LookAt(TestMesh.GlobalPosition + velY.Slide(normal));
		//GD.Print(velocity.Length());
		//GD.Print(angle);
	}

	//Low and High are the same but keep functions for if you wanna change it
	public void HighGrapple(ref Vector3 velocity)
	{
		//GD.Print("High");
		PressSpaceTimer.Stop();
		velocity /= 5;
		velocity.Y = 0;
		StraightJump(ref velocity, HighGrappleJumpMarker.GlobalPosition - GlobalPosition, HighGrappleLeapMod, HighGrappleJumpMod);
		GrappleSecondJumpTimer.Start();
	}
	public void LowGrapple(ref Vector3 velocity)
	{
		//GD.Print("Low");
		PressSpaceTimer.Stop();
		velocity /= 5;
		velocity.Y = 0;
		StraightJump(ref velocity, LowGrappleJumpMarker.GlobalPosition - GlobalPosition, LowGrappleLeapMod, LowGrappleJumpMod);
		GrappleSecondJumpTimer.Start();
	}


	public void StraightJump(ref Vector3 velocity, Vector3 dir, float LeapForceMod, float JumpForceMod)
	{
		dir = dir.Normalized();

		velocity.X += LeapForce * LeapForceMod * dir.X;
		velocity.Z += LeapForce * LeapForceMod * dir.Z;
		velocity.Y += JumpForce * JumpForceMod * dir.Y;
	}

	public void Jump(ref Vector3 velocity, float LeapForceMod = 1, float JumpForceMod = 1)
	{
		//decide where to Jump
		Vector3 dir;
		if (GetNode<Marker3D>("Camera3D/LookAtMarker").GlobalRotationDegrees.X < LowJumpDegree && IsOnFloor())
		{
			dir = GetNode<Marker3D>("HeadCenterMarker/GroundJumpBaseMarker/GroundJumpMarker").GlobalPosition - GlobalPosition;
		}
		else
		{
			dir = GetNode<Marker3D>("Camera3D/LookAtMarker").GlobalPosition - GlobalPosition;
		}
		StraightJump(ref velocity, dir, LeapForceMod, JumpForceMod);
	}

	public void KingJump(ref Vector3 velocity, float KingJumpRatio, float LeapForce, float JumpForce)
	{
		GD.Print(KingJumpRatio);
		Vector3 dir;
		var highPos = Vector3.Up;
		var lowPos = GetNode<Marker3D>("HeadCenterMarker/GroundJumpBaseMarker/GroundJumpMarker").GlobalPosition - HeadCenterMarker.GlobalPosition;
		//lowPos = lowPos.Slide(highPos);
		dir = lowPos * (1.0f - KingJumpRatio) + highPos * KingJumpRatio;
		StraightJump(ref velocity, dir, LeapForce, JumpForce);
	}

	public void Fly(ref Vector2 velocity, Vector2 inputDir, float delta)
	{
		//GD.Print("Fly");

		//Min speed
		Vector2 accel = inputDir * AirControl * delta;
		if (velocity.Length() <= MinSpeed * delta)
		{
			velocity += accel;
			return;	
		}

		//accel components
		Vector2 parallelComponent = velocity.Normalized() * (velocity.Dot(accel) / velocity.Length());
		Vector2 perpendicularComponent = accel - parallelComponent;
		
		//apply accel only if it not contributes to speed
		if ((velocity + accel).Length() > velocity.Length())
		{
			velocity += perpendicularComponent;
		}
		else
		{
			velocity += accel;
		}
	}

	public void Sprint(ref Vector2 velocity, Vector2 inputDir, float delta)
	{
		//minspeed
		if (velocity.Length() < MinSpeed * delta)
			velocity = inputDir * MinSpeed * delta;
		//accelerate
		velocity += inputDir * Accel * delta;
		velocity = inputDir * velocity.Length();
		//maxspeed
		if (velocity.Length() > MaxSpeed * delta)
			velocity = inputDir * MaxSpeed * delta;
			//velocity = velocity.MoveToward(inputDir * MaxSpeed * delta, Accel * (float)delta + 10 * (float)delta);
	}

	public void Walk(ref Vector2 velocity, Vector2 inputDir, float delta)
	{
		if (velocity.Length() > MinSpeed * delta)
		{
			velocity = velocity.Lerp(inputDir.Normalized() * MinSpeed * delta, HighSpeedFriction * 3 * delta);
		}
		else
		{
			velocity = inputDir.Normalized() * MinSpeed * delta;
		}
	}

	public void Slowdown(ref Vector2 velocity, float delta)
	{
		//??? wierd but its soft

		//case when speed so low its almost zero
		if (velocity.Length() < MinSpeed / 5000 * delta)
		{
			velocity = new Vector2(0, 0);
			return;
		}

		//high velocity => less friction
		if (velocity.Length() > HighSpeed * delta)
		{
			velocity = velocity.Lerp(new Vector2(0, 0), HighSpeedFriction * delta);
		}
		else
		{
			velocity = velocity.Lerp(new Vector2(0, 0), Friction * delta);
		}
	}
}
