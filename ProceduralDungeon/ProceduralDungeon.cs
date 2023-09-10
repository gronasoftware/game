using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

[Tool]
public partial class ProceduralDungeon : Node3D
{
	public GridMap gridMap;
	RandomNumberGenerator random;


    private bool _generate = false;
	[Export]
	public bool Generate
	{
		get
		{
			return _generate;
		}
		set
		{
			GenerateDungeon();
		}
	}

	private uint _borderSize = 50;
	[Export]
	public uint BorderSize
	{
		get
		{
			return _borderSize;
		}
		set { 
			_borderSize = value;
			// gridMap.Clear();
			// GenerateBorder();


		}
	}

	[Export]
	private uint _maxRoomSize = 10;
	[Export]
	private uint _minRoomSize = 5;
	[Export]
	private uint RoomMargin = 2;
	[Export]
	private uint NumberOfRooms = 5;

	public override void _Ready()
	{
		gridMap = GetNode<GridMap>("GridMap");
	}

	public List<Vector3I[]> WallTiles;

    public List<Vector3> RoomPositions;

    public void GenerateDungeon()
	{
        random = new RandomNumberGenerator();
        gridMap.Clear();

		// GenerateBorder();

        WallTiles = new List<Vector3I[]>();
		RoomPositions = new List<Vector3>();

        for (int i = 0; i < NumberOfRooms; i++)
		{
            GenerateRoom(0);
        }

		Vector2[] roomPositions2D = new Vector2[RoomPositions.Count];

		AStar2D delGraph = new AStar2D();
        AStar2D mstGraph = new AStar2D();

		for(int i = 0; i < RoomPositions.Count; i++) {
			Vector2 pos = new Vector2(RoomPositions[i].X, RoomPositions[i].Z);
			roomPositions2D[i] = pos;
			delGraph.AddPoint(i, pos);
			mstGraph.AddPoint(i, pos);
        }

		int[] delaunayIndices = Geometry2D.TriangulateDelaunay(roomPositions2D);

		for(int i = 0;i < delaunayIndices.Length/3;i++) { 
			int p1 = delaunayIndices[3 * i];
            int p2 = delaunayIndices[3 * i + 1];
            int p3 = delaunayIndices[3 * i + 2];

            delGraph.ConnectPoints(p1, p2);
            delGraph.ConnectPoints(p2, p3);
            delGraph.ConnectPoints(p1, p3);
        }

		// MST
		List<int> visited = new List<int>();
		visited.Add((int)(random.Randi() % RoomPositions.Count));
        while (visited.Count != mstGraph.GetPointCount())
		{
			List<int[]> possibleConnections = new List<int[]>();

			foreach (int point in visited)
			{
                foreach (int conn in delGraph.GetPointConnections(point))
				{
                    // Makes sure that we do not get a cycle
                    if (!visited.Contains(conn))
					{
						possibleConnections.Add(new int[] { point, conn });
					}
				}
			}

			int[] connection = possibleConnections[(int)(random.Randi() % possibleConnections.Count())];
			
			foreach (int[] conn in possibleConnections)
			{
				if (roomPositions2D[conn[0]].DistanceSquaredTo(roomPositions2D[conn[1]]) <
					roomPositions2D[connection[0]].DistanceSquaredTo(roomPositions2D[connection[1]])){
                    connection = conn;
				}
			}

			visited.Add(connection[1]);
			mstGraph.ConnectPoints(connection[0], connection[1]);
            delGraph.DisconnectPoints(connection[0], connection[1]);
        }

		GenerateCorridors(mstGraph);
    }

    private void GenerateBorder()
	{
		// Only whilst in editor, for debugging
		if (Engine.IsEditorHint())
			for (int i = -1; i < BorderSize + 1; i++)
			{
				gridMap.SetCellItem(new Vector3I(i, 0, -1), 3);
				gridMap.SetCellItem(new Vector3I(i, 0, (int)BorderSize), 3);
				gridMap.SetCellItem(new Vector3I((int)BorderSize, 0, i), 3);
				gridMap.SetCellItem(new Vector3I(-1, 0, i), 3);
			}
	}

	private void GenerateRoom(int depth)
	{
		uint sizeX, sizeZ;

        if (_maxRoomSize > _minRoomSize)
		{
			sizeX = random.Randi() % (_maxRoomSize - _minRoomSize) + _minRoomSize;
            sizeZ = random.Randi() % (_maxRoomSize - _minRoomSize) + _minRoomSize;
        } 
		else
		{
			sizeX = _minRoomSize;
			sizeZ = _minRoomSize;
		}

		Vector3I startPos = new Vector3I();
		startPos.X = (int)(random.Randi() % (BorderSize - sizeX + 1 ));
		startPos.Z = (int)(random.Randi() % (BorderSize - sizeZ + 1));


		for (int z = -(int)RoomMargin; z < sizeZ + (int)RoomMargin; z++)
		{
			for (int x = -(int)RoomMargin; x < sizeX + (int)RoomMargin; x++)
			{
				if(gridMap.GetCellItem(startPos + new Vector3I(x, 0, z)) == 0)
				{
					if (depth < 5) // Arbitrary recursion limit when to stop trying to place a room
						GenerateRoom(depth + 1);
					return;
				}
				
			}
		}

		List<Vector3I> wallTiles = new List<Vector3I>();

        for (int x = 1; x < sizeX-1; x++)
        {
            Vector3I pos = startPos + new Vector3I(x, 0, 0);
			wallTiles.Add(pos);

            pos = startPos + new Vector3I(x, 0, (int)sizeZ - 1);
            wallTiles.Add(pos);
        }

        for (int z = 1; z < sizeZ - 1; z++)
        {
            Vector3I pos = startPos + new Vector3I(0, 0, z);
            wallTiles.Add(pos);

            pos = startPos + new Vector3I((int)sizeX -1, 0, z);
            wallTiles.Add(pos);
        }

        for (int z = 0; z < sizeZ; z++)
		{
			for (int x = 0; x < sizeX; x++)
			{
				Vector3I pos = startPos + new Vector3I(x, 0, z);
                gridMap.SetCellItem(pos, 0);
			}
		}

        Vector3I[] wallTilesArray = new Vector3I[wallTiles.Count];
		for (int i = 0; i < wallTiles.Count; i++)
			wallTilesArray[i] = wallTiles[i];
        WallTiles.Add(wallTilesArray);

        float avgX = startPos.X + sizeX / 2f;
        float avgZ = startPos.Z + sizeZ / 2f;

		RoomPositions.Add(new Vector3(avgX, 0, avgZ));
    }


    private void GenerateCorridors(AStar2D corridorGraph)
	{
		List<Vector3[]> corridors = new List<Vector3[]>();

		foreach(int p in corridorGraph.GetPointIds()) 
		{
            foreach (int c in corridorGraph.GetPointConnections(p))
			{
				if (c>p)
				{
					Vector3 tileFrom = new Vector3();
					Vector3 tileTo = new Vector3();
					float minDistance = float.MaxValue;

					// Naive brute-force approach, can improve to nlogn if necessary!
					foreach(Vector3 from in WallTiles[p])
                        foreach (Vector3 to in WallTiles[c])
						{
							float dist = from.DistanceSquaredTo(to);
							if(dist < minDistance)
							{
								minDistance = dist;
								tileFrom = from;
								tileTo = to;
							}
						}

					corridors.Add(new Vector3[] { tileFrom, tileTo });

					// Add door tiles
					gridMap.SetCellItem((Vector3I)tileFrom, 2);
                    gridMap.SetCellItem((Vector3I)tileTo, 2);
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

		foreach(Vector3[] c in corridors)
		{
			Vector2I tileFrom = new Vector2I((int)c[0].X, (int)c[0].Z);
            Vector2I tileTo = new Vector2I((int)c[1].X, (int)c[1].Z);

			Vector2[] pointPath = astar.GetPointPath(tileFrom, tileTo);

			foreach(Vector2 p in pointPath)
			{
				Vector3I corridorTile = new Vector3I((int)p.X, 0, (int)p.Y);

                astar.SetPointSolid(new Vector2I(corridorTile.X, corridorTile.Z));

                // If not a door, set tile as corridor
                if (gridMap.GetCellItem(corridorTile) != 2)
				{
					gridMap.SetCellItem(corridorTile, 1);
				}
			}
        }

    }
}
