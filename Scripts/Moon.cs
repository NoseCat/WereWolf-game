using Godot;

public partial class Moon : Node3D
{
	[Export] float Size = 1f;

	CharacterBody3D Player;
	Sprite3D moon;
	
	public override void _Ready()
	{
		moon = GetNode<Sprite3D>("MoonSprite");
		float scale = moon.Position.Y * Size;
		moon.Scale = new Vector3(scale,scale,scale);
	}

	public override void _Process(double delta)
	{
		Player = GetTree().Root.GetNode<CharacterBody3D>("Level/Player/CharacterBody3D");
		GlobalPosition = Player.GlobalPosition;
		// LookAt(GlobalPosition + Vector3.Down);
		//Rotation = Rotation + new Vector3((float)delta,0,0);
	}
}
