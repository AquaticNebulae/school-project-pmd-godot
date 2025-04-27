using Godot;
using Godot.Collections;
using System;

public partial class DungeonMain : Node
{

	private CharacterBody2D _characterBody2D;

	private TileMapLayer _floor;
	private TileMapLayer _wall;
	

	private Dictionary<string, Vector2> _inputs = new Dictionary<string, Vector2>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_characterBody2D = GetNode<CharacterBody2D>("Character");

		_inputs = (Dictionary<string,Vector2>)_characterBody2D.Get("inputs");

		_floor = GetNode<TileMapLayer>("Dungeon/Floor");
		_wall = GetNode<TileMapLayer>("Dungeon/Wall");

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		foreach (var input in _inputs.Keys)
		{
			if (@event.IsActionPressed(input))
			{
				bool onStairsChk = (bool)_characterBody2D.Call("OnStairs", _characterBody2D.Position);
				if (onStairsChk)
				{
					//Regenerate Dungeon
					//Trigger UI Change
					_characterBody2D.Position = new Vector2(120, 120);
					_characterBody2D.Position = _characterBody2D.Position.Snapped(Vector2.One * (int)_characterBody2D.Get("tile_size"));
					_characterBody2D.Position += Vector2.One * (int)_characterBody2D.Get("tile_size") / 2;
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
