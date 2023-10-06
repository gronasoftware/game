using Godot;
using System;
using System.Runtime.CompilerServices;

public abstract partial class Weapon : Node3D
{
    protected PackedScene _projectileScene;
    protected float _shotTimer = 0f;
    protected float _shootInterval = 0.2f;

    // Called when the node enters the scene tree for the first time.
    public override void _Process(double delta)
    {
        _shotTimer += (float)delta;
    }

    public virtual void Shoot()
    {
        if (_shotTimer < _shootInterval)
            return;
        _shotTimer = 0f;
        // Spawn a projectile
        var projectile = (Node3D)_projectileScene.Instantiate();
        projectile.GlobalTransform = GlobalTransform;
        GetTree().Root.AddChild(projectile);
        // Spawn projectile slightly in front of the player
        projectile.GlobalPosition += -GlobalTransform.Basis.Z * .5f;
    }
}
