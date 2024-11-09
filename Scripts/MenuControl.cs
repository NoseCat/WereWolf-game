using Godot;

public partial class MenuControl : Control
{
	bool Paused = false;
	Node Level;
	Camera3d Cam;
	public override void _Ready()
	{
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Level = GetParent<Node>();
		//SensitivityChanged((float)GetNode<HSlider>("Sensitivity").Value);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause"))
		{
			Switch();
		}

		if (Paused)
		{
			DoPause();
		}
		else
		{
			DoUnPause();
		}
	}

	public void Switch()
	{
		Visible = !Visible;
		Paused = !Paused;
		GetTree().Paused = !GetTree().Paused;
	}

	public void DoPause()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
 
	public void DoUnPause()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public float MaxSensitivity = 0.5f;
	public float MinSensitivity = 0f;
	public void SensitivityChanged(float val)
	{
		Cam = GetTree().Root.GetNode<Camera3d>("Level/Player/CharacterBody3D/Camera3D");
		Cam.MouseSensetivity = MinSensitivity + val/100 * (MaxSensitivity - MinSensitivity);
	}

	public void ResumePressed()
	{
		Switch();
	}

	public void ExitPressed()
	{
		//Save??
		GetTree().Quit();
	}
}
