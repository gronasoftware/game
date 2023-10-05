using Godot;
using System;

public partial class ProjectileWeapon : Weapon
{
    private PackedScene _projectileScene = (PackedScene)
        GD.Load("res://Scenes/Weapons/Projectile.tscn");

    private float _shotTimer = 0f;
    private float _shootInterval = 0.2f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _shotTimer += (float)delta;
    }

    public override void Shoot()
    {
        if (_shotTimer < _shootInterval)
            return;
        _shotTimer = 0f;
        // Spawn a projectile
        var projectile = (MovingProjectile)_projectileScene.Instantiate();
        projectile.GlobalTransform = GlobalTransform;
        GetTree().Root.AddChild(projectile);
        // Spawn projectile slightly in front of the player
        projectile.GlobalPosition += -GlobalTransform.Basis.Z * .5f;
    }
}
