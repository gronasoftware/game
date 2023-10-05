using Godot;
using System;

public partial class MovingProjectile : AnimatableBody3D, IProjectile
{
    private bool _hasHit = false;
    public int damage { get; set; } = 30;
    public float speed = 7f;

    private float _despawnTimer = 0;
    private float _despawnTime = 2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (_hasHit)
        {
            _despawnTimer += (float)delta;
            if (_despawnTimer >= _despawnTime)
            {
                QueueFree();
            }
            return;
        }

        // Move the projectile forward
        var sp = !_hasHit ? speed : 0f;
        // Check for collisions
        var collision = MoveAndCollide(-Transform.Basis.Z * sp * (float)delta);

        if (collision != null && !_hasHit)
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
