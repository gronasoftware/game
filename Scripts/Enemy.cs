using Godot;
using System;

public partial class Enemy : CharacterBase
{

	private int shotTimer = 120;

	public override void _PhysicsProcess(double delta)
	{
		shotTimer--;
		if (shotTimer <= 0)
		{
			shotTimer = 120;
			EmitSignal(SignalName.ShotFired, Transform.Origin, -Transform.Basis.Z);
		}
		MoveAndSlide();
	}

  public override void NoMoreHealth()
	{
		// Signal a death event
		QueueFree();
	}


}
