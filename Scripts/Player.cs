using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 10f;

	public override void _PhysicsProcess(double delta)
	{
		// https://ask.godotengine.org/25922/how-to-get-3d-position-of-the-mouse-cursor

		var camera = GetViewport().GetCamera3D();
		var mousePosition2D = GetViewport().GetMousePosition();
		var dropPlane = new Plane(Vector3.Up, Transform.Origin.Y);

		var _mousePosition3D = dropPlane.IntersectsRay(
			camera.ProjectRayOrigin(mousePosition2D),
			camera.ProjectRayNormal(mousePosition2D)
		);

		Vector3 mousePosition3D = _mousePosition3D ?? Vector3.Zero;

		// Rotate to face the mouse.
		Vector3 lookAt = mousePosition3D - Transform.Origin;
		lookAt.Y = Transform.Origin.Y;
		Transform = Transform.LookingAt(lookAt, Vector3.Up);

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
		Position += direction * (float)delta * Speed;
	}
}
