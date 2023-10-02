using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class CharacterBase : CharacterBody3D
{
    [Signal]
    public delegate void ShotFiredEventHandler(Vector3 origin, Vector3 direction);

    [Export]
    public int health = 100;

    private Node root;

    public override void _Ready()
    {
        // Register with the TestEnvironment
        root = GetTree().Root.GetChild(0);
        if (root is ICombatScene combatScene)
        {
            combatScene.RegisterCharacter(this);
        }
    }

    public override void _ExitTree()
    {
        // Unregister with the TestEnvironment
        if (root is ICombatScene combatScene)
        {
            combatScene.UnregisterCharacter(this);
        }
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

    public void IncreaseHealth(int amount)
    {
        health += amount;
    }

    public abstract void NoMoreHealth();
}
