using Godot;
using System;

public partial class Cube : MeshInstance3D
{
    float moveSpeed = 5f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Cube Ready");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //float moveSpeed = 5f;
        Rotate(Vector3.Up, 2 * (float)delta);
        // if (Input.IsActionPressed("move_right"))
        // {
        // 	Position += Vector3.Right * (float)delta * moveSpeed;
        // }

        // if (Input.IsActionPressed("move_left"))
        // {
        // 	Position += Vector3.Left * (float)delta * moveSpeed;
        // }

        // if (Input.IsActionPressed("move_forward"))
        // {
        // 	Position += Vector3.Forward * (float)delta * moveSpeed;
        // }

        // if (Input.IsActionPressed("move_backward"))
        // {
        // 	Position += Vector3.Back * (float)delta * moveSpeed;
        // }

        // if (Input.IsActionPressed("ui_right"))
        // {
        // 	Rotate(Vector3.Up, 2 * (float)delta);
        // }
    }
}
