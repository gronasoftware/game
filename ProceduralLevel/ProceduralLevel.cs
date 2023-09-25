using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class ProceduralLevel : Node3D
{
    RandomNumberGenerator random;

    [Export]
    public PackedScene[] areas;

    private uint _startArea = 0;
    [Export]
    public uint StartArea { get => _startArea; set { _startArea = (uint)Math.Clamp(value, 0, areas.Length-1); } }

    [Export]
    public bool generate { get => false; set => Generate(); }

    [Export]
    public uint RecDepth = 3;

    private bool[,] occupiedTiles;
    private Vector2I midPoint;

    private bool Recurse(Area area, uint connID, uint recursionsLeft)
    {
        // Add base case, if connection already exists at this position, return true

        if (recursionsLeft == 0)
            return false;

        HashSet<uint> set = new HashSet<uint>();
        for (uint i = 0; i < areas.Length; i++) set.Add(i);

        while(set.Count > 0)
        {
            uint areaID = set.ElementAt(random.RandiRange(0, set.Count-1));
            set.Remove(areaID);

            HashSet<uint> rotationSet = new HashSet<uint>();
            for (uint i = 0; i < 4; i++) rotationSet.Add(i);

            while(rotationSet.Count > 0)
            {
                uint rotation = rotationSet.ElementAt(random.RandiRange(0, rotationSet.Count - 1));
                rotationSet.Remove(rotation);

                Area temp = CreateArea(areaID, rotation);

                // Match with new areas connection
                for (uint i = 0; i < temp.connPos.Length; i++)
                {
                    Area nextArea = CreateArea(areaID, rotation);

                    // Calculate position for the next area using connection offsets
                    nextArea.Position = area.Position + new Vector3(area.connPos[connID].X - nextArea.connPos[i].X, 0, area.connPos[connID].Y - nextArea.connPos[i].Y);

                    // Check if we can place area
                    if (!IsOccupied(nextArea))
                    {
                        SetOccupied(nextArea);
                        AddChild(nextArea);

                        bool success = true;
                        for (uint j = 0; j < nextArea.connPos.Length; j++)
                        {
                            if (j == i) continue;

                            if(!Recurse(nextArea, j, recursionsLeft - 1))
                            {
                                success = false; 
                                break;
                            }
                        }

                        if (!success)
                        {
                            RemoveArea(nextArea);
                            continue;
                        }

                        // Success, now we can return True
                        area.connectedAreas[connID] = nextArea;
                        temp.Free();
                        return true;
                    }
                }

                temp.Free();
            }
        }

        return false;


    }

    private Area CreateArea(uint areaID, uint rotation)
    {
        Area area = (Area)areas[areaID].Instantiate();

        area.connectedAreas = new Area[area.connPos.Length];
        RotateArea(area, rotation);

        return area;
    }

    private void RemoveArea(Area area)
    {
        for(int i = 0; i < area.connectedAreas.Length; i++)
        {
            if (area.connectedAreas[i] != null)
                RemoveArea(area.connectedAreas[i]);
        }

        RemoveOccupied(area);
        RemoveChild(area);
        area.Free();
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

    private void RemoveOccupied(Area area)
    {
        Vector2I pos = new Vector2I((int)area.Position.X, (int)area.Position.Z);
        for (int x = 0; x < area.sizeX; x++)
            for (int z = 0; z < area.sizeZ; z++)
            {
                occupiedTiles[midPoint.X + pos.X + x, midPoint.Y + pos.Y + z] = false;
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

    private void RotateArea(Area area, uint timesNintety)
    {
        uint newSizeX;
        uint newSizeZ;
        Node3D node3d;

        switch (timesNintety)
        {
            case 0:
                break;
            case 1:
                newSizeX = area.sizeZ;
                newSizeZ = area.sizeX;
                area.sizeX = newSizeX;
                area.sizeZ = newSizeZ;
                for (int i = 0; i < area.connPos.Length; i++)
                {
                    float newX = area.connPos[i].Y;
                    float newZ = newSizeX - area.connPos[i].X;
                    area.connPos[i].X = newX;
                    area.connPos[i].Y = newZ;
                }

                node3d = area.GetNode<Node3D>("Node3D");

                node3d.Position = new Vector3((float)newSizeX / 2f, 0, (float)newSizeZ / 2f);
                node3d.RotateY((float)Math.PI / 2f);

                break;
            case 2:
                for (int i = 0; i < area.connPos.Length; i++)
                {
                    float newX = area.sizeX - area.connPos[i].X;
                    float newZ = area.sizeZ - area.connPos[i].Y;
                    area.connPos[i].X = newX;
                    area.connPos[i].Y = newZ;
                }

                node3d = area.GetNode<Node3D>("Node3D");

                node3d.Position = new Vector3((float)area.sizeX / 2f, 0, (float)area.sizeZ / 2f);
                node3d.RotateY(2f * (float)Math.PI / 2f);
                break;

            case 3:
                newSizeX = area.sizeZ;
                newSizeZ = area.sizeX;
                area.sizeX = newSizeX;
                area.sizeZ = newSizeZ;
                for (int i = 0; i < area.connPos.Length; i++)
                {
                    float newX = newSizeZ - area.connPos[i].Y;
                    float newZ = area.connPos[i].X;
                    area.connPos[i].X = newX;
                    area.connPos[i].Y = newZ;
                }

                node3d = area.GetNode<Node3D>("Node3D");

                node3d.Position = new Vector3((float)newSizeX / 2f, 0, (float)newSizeZ / 2f);
                node3d.RotateY(3f*(float)Math.PI / 2f);
                break;
        }

    }

    private void Generate()
    {
        // Todo, do this dynamically based on largest size of area and recursion depth
        occupiedTiles = new bool[2000, 2000];
        midPoint = new Vector2I(1000, 1000);

        random = new RandomNumberGenerator();

        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.Free();
        }

        // Todo: Iterate through areas array, make sure that all packed scenes are using the Area script. Otherwise throw exception and return.
        // This check should not be handled in the recursion, that is messy

        if (_startArea < 0) return;

        Area area = CreateArea(_startArea, 0);
        SetOccupied(area);

        for(uint i = 0; i < area.connPos.Length; i++) Recurse(area, i, RecDepth);

        AddChild(area);
        

    }
}
