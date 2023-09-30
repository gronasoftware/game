using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBase
{
    private MeshInstance3D mousePositionSphere = null;

    [Export]
    private AnimationPlayer animPlayer;

    public override void _PhysicsProcess(double delta)
    {
        ProcessInput((float)delta);
        CalculateVelocity((float)delta);
        CalculateDirection();
        // https://ask.godotengine.org/25922/how-to-get-3d-position-of-the-mouse-cursor

        var camera = GetViewport().GetCamera3D();
        var mousePosition2D = GetViewport().GetMousePosition();
        // After we turn on gravity, we should update this to use the plane that the player is standing on (instead of the Y=0 plane)
        //var dropPlane = new Plane(Vector3.Up, 0);
        var dropPlane = new Plane(Vector3.Up, Transform.Origin.Y);

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
        sphereInstance.Position = new Vector3(
            mousePosition3D.X,
            mousePosition3D.Y,
            mousePosition3D.Z
        );
        mousePositionSphere?.QueueFree();
        mousePositionSphere = sphereInstance;
        GetParent().AddChild(sphereInstance);

        Transform = Transform.LookingAt(
            new Vector3(mousePosition3D.X, Transform.Origin.Y, mousePosition3D.Z),
            Vector3.Up
        );

        if (Input.IsActionJustPressed("shoot"))
        {
            EmitSignal(SignalName.ShotFired, Transform.Origin, -Transform.Basis.Z);
        }
        AnimationTree animationTree = GetNode<AnimationTree>("AnimationTree");

        // Set the state of the "Idle" animation to true if the player is not moving
        animationTree.Set("parameters/conditions/Idle", direction == Vector3.Zero);
        // Set the state of the "Run" animation to true if the player is moving
        animationTree.Set("parameters/conditions/run", direction != Vector3.Zero);

        MoveAndSlide();
    }

    public override void NoMoreHealth()
    {
        GD.Print("Game Over!");
    }

    #region Input

    private Vector2 inputDir;

    private void ProcessInput(float delta)
    {
        inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
    }

    #endregion

    #region Velocity
    private Vector3 velocity;
    private float previousVelocityY,
        newVelocityY;
    private float previuosVelocityX,
        newVelocityX;
    private float previuosVelocityZ,
        newVelocityZ;

    [Export]
    public const float speed = 5f;

    /// <summary>
    /// Calculate the bi-velocity and apply it to the character body
    /// </summary>
    /// <param name="delta"></param>
    private void CalculateVelocity(float delta)
    {
        velocity = Velocity;
        CalculateVelocityY(ref velocity, delta);
        CalculateVelocityXZ(ref velocity, delta);
        Velocity = velocity;
    }

    private void CalculateVelocityY(ref Vector3 vel, float delta)
    {
        ApplyGravity(ref vel, delta);
    }

    private void CalculateVelocityXZ(ref Vector3 vel, float delta)
    {
        previuosVelocityX = velocity.X;
        previuosVelocityZ = velocity.Z;
        newVelocityX = direction.X * speed;
        newVelocityZ = direction.Z * speed;
        vel.X = (previuosVelocityX + newVelocityX) * .5f;
        vel.Z = (previuosVelocityZ + newVelocityZ) * .5f;
    }
    #endregion

    #region Gravity
    [Export]
    private const float gravity = 9.8f;

    [Export]
    private const float terminalVelocity = 600f;

    /// <summary>
    /// Apply gravity to the velocity
    /// </summary>
    /// <param name="vel"></param>
    /// <param name="delta"></param>
    private void ApplyGravity(ref Vector3 vel, float delta)
    {
        if (IsOnFloor())
            return;
        previousVelocityY = velocity.Y;
        newVelocityY = vel.Y - gravity * delta;
        newVelocityY = Mathf.Clamp(newVelocityY, -Mathf.Inf, terminalVelocity);
        vel.Y = (previousVelocityY + newVelocityY) * .5f;
    }
    #endregion

    #region Direction
    private Vector3 direction;

    private void CalculateDirection()
    {
        direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();
    }
    #endregion
}
