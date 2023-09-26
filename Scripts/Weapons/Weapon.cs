using Godot;
using System;

public abstract partial class Weapon : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public abstract void Shoot();
}
