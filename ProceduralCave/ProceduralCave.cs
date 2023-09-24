using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ProceduralCave : Node3D
{
    RandomNumberGenerator random;

    [Export]
    public PackedScene[] areas;

    private int _startArea = -1;
    [Export]
    public int StartArea { get => _startArea; set { _startArea = Math.Clamp(value, 0, areas.Length) - 1; } }

    [Export]
    public bool generate { get => false; set => Generate(); }

    [Export]
    public uint RecDepth = 3;

    private bool[,] occupiedTiles;
    private bool[,] connPositions;
    private Vector2I midPoint;

    private void Recurse(Area currentArea, Vector2 currentConnPos, uint recursionsLeft)
    {
        int attempts = 0;
        while(true)
        {
            // Should return false
            if (attempts++ > 20 || recursionsLeft == 0)
            {
                return;
            }

            // TODO; Use a set and remove random areas from the set instead of taking a new random area from whole set every time
            Area nextArea = (Area)areas[random.RandiRange(0, areas.Length - 1)].Instantiate();

            RotateArea(nextArea, random.RandiRange(0, 1));
            // Try matching connection with every connection of new area
            foreach (Vector2 nextConnPos in nextArea.connPos)
            {
                // Calculate position for the next area using connection offsets
                nextArea.Position = currentArea.Position + new Vector3(currentConnPos.X - nextConnPos.X, 0, currentConnPos.Y - nextConnPos.Y);

                // Check if we can place area
                if (!IsOccupied(nextArea))
                {
                    SetOccupied(nextArea);

                    List<Vector2> uncoveredConnections = new List<Vector2>();
                    foreach (Vector2 newConnPos in nextArea.connPos)
                        if (!ConnPosExists(nextArea, newConnPos)) uncoveredConnections.Add(newConnPos);


                    SetConnPositions(nextArea);

                    // TODO; Check that all recursive calls return true; Also should keep track which connections are satisfied
                    foreach (Vector2 newConnPos in uncoveredConnections)
                        Recurse(nextArea, newConnPos, recursionsLeft - 1);

                    AddChild(nextArea);

                    // Success, now we can return (should return true in future)
                    return;
                }
            }
        }


    }

    private void SetOccupied(Area area)
    {
        Vector2I pos = new Vector2I((int)area.Position.X, (int)area.Position.Z);
        for (int x = 0; x < area.sizeX; x++)
            for (int z = 0; z < area.sizeZ; z++)
            {
                occupiedTiles[midPoint.X + pos.X + x, midPoint.Y + pos.Y + z] = true;
            }
    }

    private bool IsOccupied(Area area)
    {
        Vector2I pos = new Vector2I((int)area.Position.X, (int)area.Position.Z);
        for (int x = 0; x < area.sizeX; x++)
            for (int z = 0; z < area.sizeZ; z++)
            {
                if(occupiedTiles[midPoint.X + pos.X + x, midPoint.Y + pos.Y + z])
                {
                    return true;
                }
            }

        return false;
    }

    private void SetConnPositions(Area area)
    {
        foreach (Vector2 connPos in area.connPos)
        {
            Vector2I pos = new Vector2I((int)area.Position.X + (int)connPos.X, (int)area.Position.Z + (int)connPos.Y);
            connPositions[midPoint.X + pos.X, midPoint.Y + pos.Y] = true;
        }
        
    }

    private bool ConnPosExists(Area area, Vector2 connPos)
    {
        Vector2I pos = new Vector2I((int)area.Position.X + (int)connPos.X, (int)area.Position.Z + (int)connPos.Y);

        return connPositions[midPoint.X + pos.X, midPoint.Y + pos.Y];
    }

    private void RotateArea(Area area, int timesNintety)
    {
        switch (timesNintety)
        {
            case 0: break; 
            case 1:
                uint newSizeX = area.sizeZ;
                uint newSizeZ = area.sizeX;

                area.sizeX = newSizeX;
                area.sizeZ = newSizeZ;
                for (int i = 0; i < area.connPos.Length; i++)
                {
                    float newX = area.connPos[i].Y;
                    float newZ = newSizeX - area.connPos[i].X;
                    area.connPos[i].X = newX;
                    area.connPos[i].Y = newZ;
                }

                Node3D node3d = area.GetNode<Node3D>("Node3D");

                node3d.GlobalPosition = new Vector3((float)newSizeX/2f, 0, (float)newSizeZ /2f);
                node3d.RotateY((float)Math.PI/2f);
                break;
        }
    }

    private void Generate()
    {

        // Todo, do this dynamically based on largest size of area and recursion depth
        occupiedTiles = new bool[2000, 2000];
        connPositions = new bool[2000, 2000];
        midPoint = new Vector2I(1000, 1000);

        random = new RandomNumberGenerator();

        // Clear previous
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.Free();
        }

        // Todo: Iterate through areas array, make sure that all packed scenes are using the Area script. Otherwise throw exception and return.
        // This check should not be handled in the recursion, that is messy

        if (_startArea < 0) return;
        Area area = (Area)areas[_startArea].Instantiate();
        SetOccupied(area);
        SetConnPositions(area);
        foreach (Vector2 connPos in area.connPos)
            Recurse(area, connPos, RecDepth);

        AddChild(area);


        

    }
}
