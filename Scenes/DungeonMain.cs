using Godot;
using Godot.Collections;
using System;

public partial class DungeonMain : Node
{

	private Area2D _area2D;

	private TileMapLayer _floor;
	private TileMapLayer _wall;
	

	private Dictionary<string, Vector2> _inputs = new Dictionary<string, Vector2>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_area2D = GetNode<Area2D>("Character");

		_inputs = (Dictionary<string,Vector2>)_area2D.Get("inputs");

		_floor = GetNode<TileMapLayer>("Dungeon/Floor");
		_wall = GetNode<TileMapLayer>("Dungeon/Wall");

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		foreach (var input in _inputs.Keys)
		{
			if (@event.IsActionPressed(input))
			{
				bool onStairsChk = (bool)_area2D.Call("OnStairs", _area2D.Position);
				if (onStairsChk)
				{
					//Regenerate Dungeon
					//Trigger UI Change
					_area2D.Position = new Vector2(120, 120);
					_area2D.Position = _area2D.Position.Snapped(Vector2.One * (int)_area2D.Get("tile_size"));
					_area2D.Position += Vector2.One * (int)_area2D.Get("tile_size") / 2;
					_wall._Ready();

				}
			}
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		



	}
}
