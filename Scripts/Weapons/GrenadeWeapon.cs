using Godot;
using System;

public partial class GrenadeWeapon : Weapon
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _projectileScene = (PackedScene)GD.Load("res://Scenes/Weapons/GrenadeProjectile.tscn");
    }
}
