using Godot;
using System;

public partial class Character : CharacterBody3D
{
	public int health = 100;

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}

	public void TakeDamage(int damage)
	{
		health -= damage;
		GD.Print("Enemy Health: " + health);
		if (health <= 0)
		{
			health = 0;
			Die();
		}
	}

	public void Die()
	{
		// Signal a death event
		QueueFree();
	}


}
