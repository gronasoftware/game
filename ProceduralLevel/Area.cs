using Godot;
using System;

[Tool]
public partial class Area : Node3D
{
    [Export]
    public uint sizeX;
    [Export]
    public uint sizeZ;

    [Export]
    public Vector2[] connPos;


    public Area[] connectedAreas;

}
