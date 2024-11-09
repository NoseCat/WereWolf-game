using Godot;
using System;

public partial class Camera3d : Camera3D
{
	public float MouseSensetivity = 0.3f;

	public Vector3 MouseRotVec = new Vector3(0,0,0);

	public bool Control = true;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			//#### we swap X and Y because schizo math stuff in godot
			MouseRotVec += -MouseSensetivity * new Vector3(eventMouseMotion.Relative.Y, eventMouseMotion.Relative.X, 0);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print(MouseRotVec);
		if(MouseRotVec.Y > 360.0f)
			MouseRotVec.Y -= 360.0f;
		if (MouseRotVec.Y < -360.0f)
			MouseRotVec.Y += 360.0f;
		MouseRotVec.X = Math.Clamp(MouseRotVec.X, -90.0f, 90.0f); //X and Y swaped
		if(Control)
			RotationDegrees = MouseRotVec;
	}
}
