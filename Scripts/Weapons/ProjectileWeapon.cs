using Godot;
using System;


public partial class ProjectileWeapon : Weapon 
{
	private PackedScene _projectileScene = (PackedScene)GD.Load("res://Scenes/Weapons/Projectile.tscn");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void Shoot()
	{
		// Spawn a projectile
		var projectile = (Projectile)_projectileScene.Instantiate();
		GetTree().Root.AddChild(projectile);
		projectile.GlobalTransform = GlobalTransform;
	}
}
