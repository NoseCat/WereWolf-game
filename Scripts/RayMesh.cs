using System;
using Godot;

public partial class RayMesh : MeshInstance3D
{
	private SurfaceTool surfaceTool;
	private Mesh rayMesh;

	private Material rayMaterial;

	public override void _Ready()
	{
		TopLevel = true;
	}

	public void DrawRay(Vector3 from, Vector3 to, Color color)
	{
		// Calculate direction and length
        Vector3 direction = to - from;
        float length = direction.Length();

        CylinderMesh cylinder = new CylinderMesh();
        cylinder.TopRadius = 0.02f; 
        cylinder.BottomRadius = 0.02f;
        cylinder.Height = length; 
        cylinder.RadialSegments = 6;

        Mesh = cylinder;
        Vector3 midPoint = (from + to) * 0.5f;
		GlobalPosition = midPoint;

		StandardMaterial3D material = new StandardMaterial3D();
    	material.AlbedoColor = color;  // Set the color
    	Mesh.SurfaceSetMaterial( 0, material); 

		LookAt(GlobalPosition + direction);
		var right = direction.Cross(Vector3.Up);
		Rotate( right.Normalized(), Mathf.Pi / 2);
	}
}
