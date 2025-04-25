using Godot;
using Godot.Collections;
using System;

public partial class CharacterCore : Area2D
{
	[Export]
	public int tile_size = 24;
	public Dictionary<string,Vector2> inputs = new Dictionary<string, Vector2>();
	

	[Export]
	public RayCast2D rayCast;


	[Export]
	public TileMapLayer floorData;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		inputs.Add("move_right", Vector2.Right);
		inputs.Add("move_left", Vector2.Left);
		inputs.Add("move_up", Vector2.Up);
		inputs.Add("move_down", Vector2.Down);

		Position = Position.Snapped(Vector2.One * tile_size);
		Position += Vector2.One * tile_size / 2;

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		foreach (var input in inputs.Keys)
		{
			if(@event.IsActionPressed(input)){
				move(input);
				OnStairs(Position);
			}
		}


	}

	private void move(string input)
	{
		rayCast.TargetPosition = inputs[input] * tile_size;
		rayCast.ForceRaycastUpdate();
		if(!rayCast.IsColliding())
			Position += inputs[input] * tile_size;
		
	}


	private bool OnStairs(Vector2 pos)
	{
		//Character Cords are 24* the size of a regular tile internally.
		Vector2 newPos = new Vector2((int)pos.X / tile_size, (int)pos.Y / tile_size);
		TileData data = floorData.GetCellTileData((Vector2I)newPos);

		//Debug Print Statements to see if it worked.
		//GD.Print(newPos);
		//GD.Print(data);

		if ((bool)data.HasCustomData("Stair")){
			if ((bool)data.GetCustomData("Stair"))
			{
				GD.Print("Found the Stairs!");
				return true;
			}
		}

		return false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
