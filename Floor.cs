using Godot;
using System;

public partial class Floor : TileMapLayer
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





	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Random randinf = new Random();

		for(int x = 0; x < map_width; x++)
		{
			for (int y = 0; y < map_height; y++)
			{
				Vector2I pos = new Vector2I(x, y);
				Vector2I map = new Vector2I(GD.RandRange(12, 17), GD.RandRange(0, 6));
				SetCell(pos,0,map,0);
			}
		}

		//Warp Tile
		SetCell(new Vector2I(23, 25), 0, new Vector2I(7, 1), 0);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
