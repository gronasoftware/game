using Godot;
using System;

public partial class Enemy : CharacterBase
{

	public override void _PhysicsProcess(double delta)
	{
		var healthbar = (ProgressBar)GetNode("SubViewport/HealthBar");
		healthbar.Value = health;
		MoveAndSlide();
	}

    public override void NoMoreHealth()
	{
		// Signal a death event
		QueueFree();
	}


}
