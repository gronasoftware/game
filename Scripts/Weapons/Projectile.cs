using Godot;
using System;

public partial class Projectile : AnimatableBody3D
{
    private bool _hasHit = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Created projectile");
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
            if (!_hasHit) {
                GD.Print("Hit something");
								//CombatManager combatManager = (CombatManager)GetTree().Root.FindChild("CombatManager");
								//EmitSignal(SignalName.ProjectileHit, collision);
								if (collision.GetCollider() is CharacterBase character) {
									character.ReduceHealth(10);
								}
						}
            _hasHit = true;
            // Destroy the projectile
            //QueueFree();
        }
    }
}
