using Godot;
using System;

public partial class Projectile : AnimatableBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		// Move the projectile forward
		var speed = 3f;
		// Check for collisions
		var collision = MoveAndCollide(-Transform.Basis.Z * speed * (float)delta);
		if (collision != null)
		{
			// Emit a signal
			//EmitSignal(nameof(HitSomething), collision.Collider);
			GD.Print("Hit something");
			// Destroy the projectile
			//QueueFree();
		}
	}
}
