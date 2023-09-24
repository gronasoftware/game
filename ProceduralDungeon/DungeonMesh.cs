using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using static ProceduralDungeon;

[Tool]
public partial class DungeonMesh : Node3D
{
    ProceduralDungeon dungeon;

    [Export]
    public bool Generate { get => false; set =>  GenerateDungeon(); }

    private PackedScene dungeonScene;
    private GridMap gridMap;

    public override void _Ready()
    {
        GenerateDungeon();
    }

    private void Remove(Node3D instance, String name)
    {
        MeshInstance3D remove = instance.GetNode<MeshInstance3D>(name);
        instance.RemoveChild(remove);
        remove.Free();
    }

    public void GenerateDungeon()
    {
        dungeon = ((ProceduralDungeon)GetParentNode3D());
        dungeon.GenerateDungeon();

        dungeonScene = (PackedScene)ResourceLoader.Load("res://ProceduralDungeon/ExampleDungeonTiles.tscn");

        gridMap = dungeon.gridMap;

        RandomNumberGenerator random = new RandomNumberGenerator();
        // Clear created tiles
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.Free();
        }

        Node3D instance;

        // Add floors
        foreach (Vector3I cell in gridMap.GetUsedCells())
        {
            instance = (Node3D)dungeonScene.Instantiate();
            instance.Position = (Vector3)cell*3;
            instance.RotateY((float)Math.PI / 2 * random.RandiRange(0, 3));

            AddChild(instance);

            Remove(instance, "Floor" + random.RandiRange(1, 2).ToString());
            Remove(instance, "Wall");
            Remove(instance, "Ceil");
        }

        for(int x = -10; x < dungeon.BorderSize+10; x++)
            for (int z = -10; z < dungeon.BorderSize+10; z++)
            {
                Vector3I pos = new Vector3I(x, 0, z);
                if (gridMap.GetCellItem(pos) < 0)
                {
                    instance = (Node3D)dungeonScene.Instantiate();
                    instance.Position = (Vector3)pos * 3;

                    AddChild(instance);
                    Remove(instance, "Floor1");
                    Remove(instance, "Floor2");
                    Remove(instance, "Wall");
                }
            }
                
        
                
        List<Room> rooms =dungeon.rooms;
        
        
        foreach (Room room in rooms)
        {
            for (int x = 0; x < room.size.X; x++)
            {
                instance = (Node3D)dungeonScene.Instantiate();
                instance.Position = ((Vector3)room.pos + new Vector3(x, 0, 0)) * 3 + new Vector3(0, 0, -1.3f);
                AddChild(instance);
                Remove(instance, "Floor1");
                Remove(instance, "Floor2");
                Remove(instance, "Ceil");


                instance = (Node3D)dungeonScene.Instantiate();
                instance.Position = ((Vector3)room.pos + new Vector3(x, 0, room.size.Z-1)) * 3 + new Vector3(0, 0, 1.3f);
                AddChild(instance);
                Remove(instance, "Floor1");
                Remove(instance, "Floor2");
                Remove(instance, "Ceil");
            }


            for (int z = 0; z < room.size.Z; z++)
            {
                instance = (Node3D)dungeonScene.Instantiate();
                instance.Position = ((Vector3)room.pos + new Vector3(0, 0, z)) * 3 + new Vector3(-1.3f, 0, 0);
                instance.RotateY((float)Math.PI / 2);
                AddChild(instance);
                Remove(instance, "Floor1");
                Remove(instance, "Floor2");
                Remove(instance, "Ceil");


                instance = (Node3D)dungeonScene.Instantiate();
                instance.Position = ((Vector3)room.pos + new Vector3(room.size.X-1, 0, z)) * 3 + new Vector3(1.3f, 0, 0);
                instance.RotateY((float)Math.PI / 2);
                AddChild(instance);
                Remove(instance, "Floor1");
                Remove(instance, "Floor2");
                Remove(instance, "Ceil");
            }

        }

    }
}
