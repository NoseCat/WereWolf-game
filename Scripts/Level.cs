using Godot;

public partial class Level : Node3D
{
	Camera3d Cam;
	public override void _Ready()
	{
		var MenuScene = GD.Load<PackedScene>("res://Objects/MenuControl.tscn");
		var Menu = MenuScene.Instantiate();
		AddChild(Menu);

		var PlayerScene = GD.Load<PackedScene>("res://Objects/player.tscn");
		Node3D Player = (Node3D)PlayerScene.Instantiate();
		AddChild(Player);
		Player.GlobalPosition = GetNode<Marker3D>("PlayerSpawner").GlobalPosition;
		
		Cam = Player.GetNode<Camera3d>("CharacterBody3D/Camera3D");
		Cam.Control = false;
		Cam.LookAt(GetNode<Marker3D>("PlayerSpawner/Look").GlobalPosition + GetNode<Marker3D>("PlayerSpawner/Look/DisplaceMarker").Position);
		
		var PlayerMesh = GetNode<MeshInstance3D>("PlayerSpawner/MeshInstance3D");
		PlayerMesh.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	bool camon = false;
	public override void _Process(double delta)
	{
		if(GetNode<Timer>("PlayerSpawner/Timer").IsStopped() && !camon)
		{
			camon = true;
			Cam.MouseRotVec = Cam.GlobalRotationDegrees;
			Cam.Control = true;
		}
		
	}
}
