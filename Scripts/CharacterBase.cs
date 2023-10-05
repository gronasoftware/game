﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract partial class CharacterBase : CharacterBody3D, ICombatant
{

    [Signal]
    public delegate void ShotFiredEventHandler(Weapon weapon);

    [Export]
    public int health = 100;

    public override void _Ready()
    {
    }

    public override void _ExitTree()
    {
    }

    public void ReduceHealth(int amount)
    {
        health -= amount;

        GD.Print("Health: " + health);
        if (health <= 0)
        {
            health = 0;
            NoMoreHealth();
        }
    }

    public void Hit(IProjectile projectile)
    {
        ReduceHealth(projectile.damage);
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
    }

    public abstract void NoMoreHealth();
}
