using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Player : CharacterBase
{
    [Export]
    public const float Speed = 3.5f;
    private MeshInstance3D mousePositionSphere = null;

    [Export]
    private AnimationPlayer animPlayer;

    private Weapon currentWeapon;
    private int weaponI = 0;
    private List<Weapon> weapons = new();

    public override void _Ready()
    {
        currentWeapon = GetNode<Weapon>("InstantWeapon");
        weapons.Add(GetNode<Weapon>("InstantWeapon"));
        weapons.Add(GetNode<Weapon>("ProjectileWeapon"));
        weapons.Add(GetNode<Weapon>("GrenadeWeapon"));
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("change_weapon"))
        {
            weaponI++;
            if (weaponI >= weapons.Count)
                weaponI = 0;
            currentWeapon = weapons[weaponI];
        }
        // https://ask.godotengine.org/25922/how-to-get-3d-position-of-the-mouse-cursor

        var camera = GetViewport().GetCamera3D();
        var mousePosition2D = GetViewport().GetMousePosition();
        // After we turn on gravity, we should update this to use the plane that the player is standing on (instead of the Y=0 plane)
        var dropPlane = new Plane(Vector3.Up, 0);
        //var dropPlane = new Plane(Vector3.Up, Transform.Origin.Y);

        Vector3 mousePosition3D =
            dropPlane.IntersectsRay(
                camera.ProjectRayOrigin(mousePosition2D),
                camera.ProjectRayNormal(mousePosition2D)
            ) ?? Vector3.Forward;

        // DEBUG STUFF:::
        // Draw a sphere at mousePosition3D
        var sphere = new SphereMesh();
        sphere.Radius = 0.1f;
        sphere.Height = 0.1f;
        var sphereInstance = new MeshInstance3D();
        sphereInstance.Mesh = sphere;
        // Again, this should be updated after we turn on gravity
        sphereInstance.Position = new Vector3(mousePosition3D.X, 0, mousePosition3D.Z);
        mousePositionSphere?.QueueFree();
        mousePositionSphere = sphereInstance;
        GetParent().AddChild(sphereInstance);

        Transform = Transform.LookingAt(
            new Vector3(mousePosition3D.X, Transform.Origin.Y, mousePosition3D.Z),
            Vector3.Up
        );
        //currentWeapon.GlobalTransform = Transform;
        //currentWeapon.LookAt(-Transform.Basis.Z);

        Vector2 inputDir = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );
        Vector3 direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

        Velocity = direction * Speed;

        if (Input.IsActionPressed("shoot"))
        {
            //EmitSignal(SignalName.ShotFired, currentWeapon);
            currentWeapon.Shoot();
        }

        if (animPlayer != null)
        {
            if (Velocity.Length() != 0 && animPlayer.CurrentAnimation != "run")
                animPlayer.Play("run");
            if (Velocity.Length() == 0 && animPlayer.CurrentAnimation != "Idle")
                animPlayer.Play("Idle");
        }

        MoveAndSlide();
    }

    public override void NoMoreHealth()
    {
        GD.Print("Game Over!");
    }
}
