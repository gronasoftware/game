using Godot;
using System;

public partial class CameraController : Camera3D
{
    [Export]
    public Node3D target;

    [Export]
    public Vector3 offset = new Vector3(0, 5, 5);

    [Export]
    public float TrailingSpeed = 5.0f;

    private Vector3 pivot = new Vector3();

    public override void _Process(double delta)
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.GlobalPosition;

            pivot = pivot.Lerp(desiredPosition, (float)delta * TrailingSpeed);

            // Update cameras global position
            GlobalPosition = pivot + offset;

            LookAt(pivot, Vector3.Up);
        }
    }
}
