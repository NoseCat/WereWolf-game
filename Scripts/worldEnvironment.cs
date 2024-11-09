using Godot;

public partial class worldEnvironment : WorldEnvironment
{
	[Export] float SkyXRotSpeed = 0.5f;
	[Export] float SkyYRotSpeed = 0.5f;
	[Export] float SkyZRotSpeed = 0.5f;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		Environment.SkyRotation += new Vector3(SkyXRotSpeed, SkyYRotSpeed, SkyZRotSpeed) * (float)delta;
	}
}
