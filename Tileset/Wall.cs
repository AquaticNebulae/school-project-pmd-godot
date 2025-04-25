using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Wall : TileMapLayer
{
	[Export]
	public int fill_percent = 45;
	[Export]
	public int smoothing_iterations = 4;
	[Export]
	public int min_cave_size = 50;
	[Export]
	public int room_radius = 4;



	[Export]
	public int map_width = 40;
	[Export]
	public int map_height = 40;
	[Export]
	public int max_gen_attempts = 16;


	[Export]
	public bool should_be_closed = true;

	public const int wall_tile = 0;
	public const int empty = 0;


	public Godot.Collections.Dictionary<Vector2,bool> map = new Godot.Collections.Dictionary<Vector2,bool>();
	public Vector2 start_point = new Vector2();
	public Vector2 end_point = new Vector2();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector2 startPoint = new Vector2(5 , 5);
		Vector2 endPoint = new Vector2(25,25);
		GenerateWallLevel(startPoint, endPoint);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//Dungeon Generation
	private void GenerateWallLevel(Vector2 start, Vector2 end)
	{
		start_point = start;
		end_point = end;

		int attempt = 0;
		bool validMap = false;

		while (!validMap &&  attempt < max_gen_attempts)
		{
			attempt++;

			Clear();
			map.Clear();

			InitializeWallMap((ulong)attempt * GD.Randi());

			for(int i = 0; i < smoothing_iterations; i++ )
			{
				SmoothWallMap();
			}

			CreateRoom(start_point, room_radius);
			CreateRoom(end_point, room_radius);

			validMap = CheckPathExists();

		}

		if (!validMap)
		{
			throw new Exception(@"Failed to generate a valid map after "+max_gen_attempts+" attempts");
		}

		if (should_be_closed)
		{
			ForceWalls();
		}

		ApplyToTilemap();

	}


	private void InitializeWallMap(ulong seed)
	{
		GD.Seed(seed);

		for(int x = 0; x < map_width; x++)
		{
			for(int y = 0; y < map_height; y++)
			{
				Vector2 pos = new Vector2(x, y);
				if(x == 0 || x == map_width - 1 || y == 0 || y == map_height - 1)
				{
					map[pos] = true;
				}
				else
				{
					map[pos] = GD.Randf() * 100.0 < (float)fill_percent;
				}
			}
		}
	}


	private void SmoothWallMap()
	{
		Godot.Collections.Dictionary<Vector2,bool> newMap = new Godot.Collections.Dictionary<Vector2, bool>();
		for(int x = 0;x < map_width; x++)
		{
			for (int y = 0;y < map_height; y++)
			{
				Vector2 pos = new Vector2(x, y);
				int wallCount = GetSurroundingWallCnt(pos);

				if (wallCount > 4)
					newMap[pos] = true;
				else if (wallCount < 4)
					newMap[pos] = false;
				else
					newMap[pos] = map[pos];
			}
		}
		map = newMap;
	}


	private int GetSurroundingWallCnt(Vector2 pos)
	{
		int wallCount = 0;

		for (int x = -1; x < 2; x++)
		{
			for (int y = -1; y < 2; y++)
			{
				Vector2 checkPos = new Vector2(pos.X + x, pos.Y + y);
				if (checkPos != pos)
				{
					if (map.TryGetValue(checkPos, out bool val) || !val)
					{
							wallCount++;
					}
				}
			}

		}
		return wallCount;
	}

	private void CreateRoom(Vector2 center, int radius)
	{
		for (int x = -radius; x <= radius; x++)
		{
			for(int y = -radius; y <= radius; y++)
			{
				Vector2 pos = new Vector2(center.X + x, center.Y + y);
				if (pos.X >= 0 && pos.X < map_width && pos.Y >= 0 && pos.Y < map_height)
				{
					if(new Vector2(x,y).Length() <= radius)
					{
						map[pos] = false;
					}
				}
			}
		}
	}


	private bool CheckPathExists()
	{
		AStar2D asatar = new AStar2D();

		for(int x = 0; x < map_width; x++)
		{
			for(int y =0; y < map_height; y++)
			{
				Vector2 pos = new Vector2(x,y);

				if (!map[pos]) {
					int pointId = GetPointID(pos);
					asatar.AddPoint(pointId, pos);
				}
			}
		}
		for (int x = 0; x < map_width; x++)
		{
			for (int y = 0; y < map_height; y++)
			{
				Vector2 pos = new Vector2(x, y);

				if (!map[pos]) { 
					int pointId = GetPointID(pos);
					Vector2[] veclist = [new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1)];
					foreach (Vector2 point in veclist)
					{
						Vector2 nextPos = pos + point;
						if (IsValidEmptyPos(nextPos))
						{
							int nextID = GetPointID(nextPos);
							if (!asatar.ArePointsConnected(pointId, nextID))
								asatar.ConnectPoints(pointId, nextID);
						}
					} 
				}
			}
		}

		int startID = GetPointID(start_point);
		int endID = GetPointID(end_point);

		return asatar.HasPoint(startID) && asatar.HasPoint(endID) && asatar.GetPointPath(startID, endID).Length > 0;


	}

	private bool IsValidEmptyPos(Vector2 pos)
	{
		return pos.X >= 0 && pos.X < map_width &&
			pos.Y >= 0 && pos.Y < map_height && !map[pos];
	}

	private int GetPointID(Vector2 pos)
	{
		return (int)(pos.X + pos.Y * map_width);
	}


	private void ForceWalls()
	{
		if (!should_be_closed)
		{
			return;
		}

		for(int x = 0; x < map_width; x++)
		{
			map[new Vector2(x, 0)] = true;
			map[new Vector2(x, map_height - 1)] = true;
		}

		for(int y = 0; y < map_height; y++)
		{
			map[new Vector2(0,y)] = true;
			map[new Vector2(map_width - 1,y)] = true;

		}
	}

	private void ApplyToTilemap()
	{
		for (int x = 0; x < map_width; x++)
		{
			for (int y = 0; y < map_height; y++)
			{
				Vector2I pos = new Vector2I(x, y);

				if (map[pos])
				{
					SetCell(pos, 0, new Vector2I(0 , GD.RandRange(0,1)), 0);
				}
				else
				{
					SetCell(pos, -1);
				}

			}
		}
	}

}
