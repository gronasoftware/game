using Godot;
using System;

public partial class MovingProjectile : AnimatableBody3D, IProjectile
{
    private bool _hasHit = false;
    public int damage { get; set; } = 30;

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
            if (!_hasHit)
            {
                //CombatManager combatManager = (CombatManager)GetTree().Root.FindChild("CombatManager");
                //EmitSignal(SignalName.ProjectileHit, collision);
                if (collision.GetCollider() is ICombatant combatant)
                {
                    combatant.Hit(this);
                }
            }
            _hasHit = true;
            // Destroy the projectile
            //QueueFree();
        }
    }
}
