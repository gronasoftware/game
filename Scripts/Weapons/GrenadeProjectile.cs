using Godot;
using System;

public partial class GrenadeProjectile : CharacterBody3D, IProjectile
{
    public int damage { get; set; } = 5;
    private float _explosionTimer = 2f;
    private float _despawnTimer = 0.5f;
    private bool _hasExploded = false;
    public float explosionRadius = 5f;
    private Area3D _explosionArea;
    private CsgSphere3D _explosionSphere;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _explosionArea = GetNode<Area3D>("ExplosionArea");
        _explosionArea.Scale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
        _explosionSphere = GetNode<CsgSphere3D>("ExplosionSphere");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _explosionTimer -= (float)delta;
        if (!_hasExploded)
        {
            MoveAndCollide(-Transform.Basis.Z * 2 * (float)delta);
        }

        if (_hasExploded)
        {
            _despawnTimer -= (float)delta;
            if (_despawnTimer <= 0)
            {
                QueueFree();
            }
        }

        if (_explosionTimer <= 0 && !_hasExploded)
        {
            _hasExploded = true;
            Explode();
        }
    }

    public void Explode()
    {
        _explosionSphere.Visible = true;
        _explosionSphere.Scale = new Vector3(explosionRadius, explosionRadius, explosionRadius);
        var bodies = _explosionArea.GetOverlappingBodies();
        for (int i = 0; i < bodies.Count; i++)
        {
            if (bodies[i] is ICombatant combatant)
            {
                combatant.Hit(this);
            }
        }
    }
}
