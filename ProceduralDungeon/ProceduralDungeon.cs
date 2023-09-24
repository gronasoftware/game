using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

[Tool]
public partial class ProceduralDungeon : Node3D
{
    public class Room
    {
        // Add more data here (type of room etc)
        public Vector3I pos, size;
        public Vector3 averagePos;
        public List<Vector3I> WallTiles;

        public Room(Vector3I pos, Vector3I size)
        {
            this.pos = pos;
            this.size = size;

            averagePos = new Vector3(pos.X + size.X / 2.0f, pos.Y + size.Y / 2.0f, pos.Z + size.Z / 2.0f);

            WallTiles = new List<Vector3I>();
            for (int x = 0; x < size.X; x++)
            {
                WallTiles.Add(pos + new Vector3I(x, 0, 0));
                WallTiles.Add(pos + new Vector3I(x, 0, size.Z-1));
            }

            for (int z = 0; z < size.Z; z++)
            {
                WallTiles.Add(pos + new Vector3I(0, 0, z));
                WallTiles.Add(pos + new Vector3I(size.X-1, 0, z));
            }
            
        }
    }


    [Export]
    public bool Generate
	{
		get => false;
		set
		{
			GenerateDungeon();
		}
	}

    [Export]
    public uint BorderSize = 80;
	[Export]
    public uint MaxRoomSize = 8;
	[Export]
    public uint MinRoomSize = 4;
	[Export]
    public uint RoomMargin = 0;
	[Export]
    public uint NumberOfRooms = 200;
    [Export]
    public uint RecAttempts = 30;

    public GridMap gridMap;
    RandomNumberGenerator random;
    bool[,] tileSet;
    public List<Room> rooms;

    public override void _Ready()
    {
        GenerateDungeon();
    }

    
    public void GenerateDungeon()
	{
        gridMap = GetNode<GridMap>("GridMap");
        random = new RandomNumberGenerator();
        gridMap.Clear();


        tileSet = new bool[BorderSize, BorderSize];
        rooms = new List<Room>();
        for (int i = 0; i < NumberOfRooms; i++)
        {
            GenerateRoom(RecAttempts);
        }


        AStar2D delauney = CreateDelauneyGraph();

        AStar2D mstGraph = CreateMSTGraph(delauney);

        // Todo, add some remaining edges of delauney graph to mstGraph

        GenerateCorridors(mstGraph);
    }

    private void GenerateRoom(uint depthLeft)
    {
        if (depthLeft == 0) return;

        // Random generation, in future look into if we get better results with algorithms like BSP
        int sizeX = random.RandiRange((int)MinRoomSize, (int)MaxRoomSize);
        int sizeZ = random.RandiRange((int)MinRoomSize, (int)MaxRoomSize);
        Vector3I pos = new Vector3I(random.RandiRange(0, (int)BorderSize - sizeX), 0, random.RandiRange(0, (int)BorderSize - sizeZ));

        
        for (int x = -(int)RoomMargin; x < sizeX + (int)RoomMargin; x++)
            for (int z = -(int)RoomMargin; z < sizeZ + (int)RoomMargin; z++)
            {
                try
                {
                    if (tileSet[pos.X+x, pos.Z+z])
                    {
                        GenerateRoom(depthLeft - 1);
                        return;
                    }
                // Lazy mans solution
                } catch(IndexOutOfRangeException e) { }
            }

        // No collision, free to create room!
        rooms.Add(new Room(pos, new Vector3I(sizeX, 0, sizeZ)));

        for (int x = 0; x < sizeX; x++)
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3I tilePos = pos + new Vector3I(x, 0, z);
                tileSet[tilePos.X, tilePos.Z] = true;

                // For debug only
                gridMap.SetCellItem(tilePos, 0);
            }

        
    }

    private AStar2D CreateDelauneyGraph()
    {
        AStar2D delGraph = new AStar2D();
        Vector2[] roomPositions2D = new Vector2[rooms.Count];
        for (int i = 0; i < rooms.Count; i++)
        {
            Vector2 pos = new Vector2(rooms[i].averagePos.X, rooms[i].averagePos.Z);
            roomPositions2D[i] = pos;
            delGraph.AddPoint(i, pos);
        }

        int[] delaunayIndices = Geometry2D.TriangulateDelaunay(roomPositions2D);
        for (int i = 0; i < delaunayIndices.Length / 3; i++)
        {
            int p1 = delaunayIndices[3 * i];
            int p2 = delaunayIndices[3 * i + 1];
            int p3 = delaunayIndices[3 * i + 2];

            delGraph.ConnectPoints(p1, p2);
            delGraph.ConnectPoints(p2, p3);
            delGraph.ConnectPoints(p1, p3);
        }

        return delGraph;
    }

    private AStar2D CreateMSTGraph(AStar2D delGraph)
    {
        AStar2D mstGraph = new AStar2D();
        for (int i = 0; i < rooms.Count; i++)
            mstGraph.AddPoint(i, new Vector2(rooms[i].averagePos.X, rooms[i].averagePos.Z));


        // Start with some random point
        List<int> visited = new List<int> { (int)(random.Randi() % rooms.Count) };

        // Iterate until all points are visited
        while (visited.Count != mstGraph.GetPointCount())
        {
            List<int[]> edges = new List<int[]>();

            foreach (int p in visited)
                foreach (int c in delGraph.GetPointConnections(p))
                    // Makes sure that we do not get a cycle
                    if (!visited.Contains(c))
                        edges.Add(new int[] { p, c });
            
            int[] u = edges[0];

            foreach (int[] v in edges)
            {
                if (mstGraph.GetPointPosition(v[0]).DistanceSquaredTo(mstGraph.GetPointPosition(v[1])) <
                    mstGraph.GetPointPosition(u[0]).DistanceSquaredTo(mstGraph.GetPointPosition(u[1]))) {
                    u = v;
                }
            }

            // Add edge to mstGraph, remove edge from the delauney graph
            visited.Add(u[1]);
            mstGraph.ConnectPoints(u[0], u[1]);
            delGraph.DisconnectPoints(u[0], u[1]);
        }

        return mstGraph;
    }

    private void GenerateCorridors(AStar2D corridorGraph)
	{
		List<Vector3I[]> corridors = new List<Vector3I[]>();

		foreach(int p in corridorGraph.GetPointIds()) 
		{
            foreach (int c in corridorGraph.GetPointConnections(p))
			{
				if (c>p)
				{
					// Vector3 tileFrom = new Vector3();
					// Vector3 tileTo = new Vector3();

                    List<Vector3I[]> doorsAlt = new List<Vector3I[]>();
                    float minDistance = float.MaxValue;

					// Naive brute-force approach, can improve to nlogn if necessary!
					foreach(Vector3I from in rooms[p].WallTiles)
                        foreach (Vector3I to in rooms[c].WallTiles)
						{
							float dist = ((Vector3)from).DistanceSquaredTo((Vector3)to);
							if(dist < minDistance)
							{
                                doorsAlt = new List<Vector3I[]> { new Vector3I[] { from, to } };
                                minDistance = dist;
							}

                            if(dist == minDistance)
                                doorsAlt.Add(new Vector3I[] { from, to });
						}

                    Vector3I[] doors = doorsAlt[random.RandiRange(0, doorsAlt.Count - 1)];
                    corridors.Add(doors);

                    // Add door tiles
                    gridMap.SetCellItem(doors[0], 1);
                    gridMap.SetCellItem(doors[1], 1);
                }
			}
        }

		AStarGrid2D astar = new AStarGrid2D();

		astar.Region = new Rect2I(Vector2I.Zero, Vector2I.One * (int)BorderSize);
		astar.Update();
		astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		astar.DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan;

		// Set obstacle in astar
		foreach(Vector3I t in gridMap.GetUsedCellsByItem(0))
			astar.SetPointSolid(new Vector2I(t.X, t.Z));

		foreach(Vector3I[] c in corridors)
		{
			Vector2I tileFrom = new Vector2I((int)c[0].X, (int)c[0].Z);
            Vector2I tileTo = new Vector2I((int)c[1].X, (int)c[1].Z);

			Vector2[] pointPath = astar.GetPointPath(tileFrom, tileTo);

			foreach(Vector2 p in pointPath)
			{
				Vector3I corridorTile = new Vector3I((int)p.X, 0, (int)p.Y);

                // astar.SetPointSolid(new Vector2I(corridorTile.X, corridorTile.Z));

                // If not a door, set tile as corridor
                if (gridMap.GetCellItem(corridorTile) != 1)
				{
					gridMap.SetCellItem(corridorTile, 2);
				}
			}
        }

    }
}
