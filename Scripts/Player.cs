using Godot;
using System;
using System.Diagnostics;

public partial class Player : CharacterBase
{
    private MeshInstance3D mousePositionSphere = null;

    [Export]
    private Vector2 inputDirection;

    [Export]
    private Vector3 velocity;

    [Export]
    private Vector3 direction;

    [Export]
    private const float gravity = 9.8f;

    [Export]
    public const float moveSpeed = 5f;

    [Export]
    private const float terminalVelocity = 600f;

    [Export] //max time between dash presses in seconds
    private const float maxDashDelay = 0.5f;

    [Export] //how long to wait before the player can dash again in seconds
    private const float maxDashTimeOut = 2.0f;

    [Export] //how long each dash lasts in seconds
    private const float maxDashTime = 0.1f;

    [Export] //how fast the player will dash
    private const float dashSpeed = 20f;

    [Export] //timer for dash
    private float dashTimer = 0.0f;

    [Export] //timer for dash time out
    private float dashTimeOut = 0.0f;

    [Export] //how many times the player has pressed the dash button
    private int dashPressCount = 0;

    [Export] //is the player dashing
    private bool dashing = false;

    [Export] //how fast the player will move
    private float speed = moveSpeed;

    private InputStates spaceDash;

    public override void _Ready()
    {
        spaceDash = new InputStates("ui_select");
    }

    public override void _PhysicsProcess(double delta)
    {
        velocity = Velocity;
        //get input vector
        inputDirection = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_backward"
        );

        // lock the direction if the player is dashing to prevent movement while dashing
        if (dashing)
            direction = new Vector3(direction.X, 0, direction.Z).Normalized();
        else
            direction = new Vector3(inputDirection.X, 0, inputDirection.Y).Normalized();

        if (!IsOnFloor())
        {
            float previousVelocityY = velocity.Y;
            float newVelocityY = velocity.Y - gravity * (float)delta;
            newVelocityY = Mathf.Clamp(newVelocityY, -Mathf.Inf, terminalVelocity);
            velocity.Y = (previousVelocityY + newVelocityY) * .5f;
        }

        // keep track of the time since the player last dashed
        if (dashTimeOut > 0.0f)
        {
            dashTimeOut -= (float)delta;
            GD.Print(dashTimeOut);
        }
        //  only attempt to dash if the player's not dashing
        if (spaceDash.CheckState() == InputStates.INPUT_JUST_PRESSED && !dashing)
            dashPressCount++; // increment dash count
        // start the dash timer when player just presses the dash button
        if (dashPressCount > 0 && !dashing)
        {
            dashTimer += (float)delta; //increment dash timer
            // the player took too long to perform the second tap so reset values
            if (dashTimer > maxDashDelay)
            {
                dashPressCount = 0;
                dashTimer = 0.0f;
            }
        }
        //if the player has pressed the dash button once(or twice idunno) and is not dashing adn the dash timeout is over
        if (dashPressCount >= 2 && !dashing && dashTimeOut <= 0.0f)
        {
            dashing = true;
            dashPressCount = 0;
            dashTimer = 0.0f;
            speed = dashSpeed;
        }
        if (dashing)
        {
            dashTimer += (float)delta;
            GD.Print(dashTimer);
            if (dashTimer >= maxDashTime)
            {
                dashing = false;
                dashTimer = 0.0f;
                speed = moveSpeed;
                dashTimeOut = maxDashTimeOut;
            }
        }

        float previuosVelocityX = velocity.X;
        float previuosVelocityZ = velocity.Z;
        float newVelocityX = direction.X * speed;
        float newVelocityZ = direction.Z * speed;
        velocity.X = (previuosVelocityX + newVelocityX) * .5f;
        velocity.Z = (previuosVelocityZ + newVelocityZ) * .5f;

        Velocity = velocity;
        MoveAndSlide();

        AnimationTree animationTree = GetNode<AnimationTree>("AnimationTree");
        // Set the state of the "Idle" animation to true if the player is not moving
        animationTree.Set("parameters/conditions/Idle", direction == Vector3.Zero);
        // Set the state of the "Run" animation to true if the player is moving
        animationTree.Set("parameters/conditions/run", direction != Vector3.Zero);

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
    }

    public override void NoMoreHealth()
    {
        GD.Print("Game Over!");
    }
}
