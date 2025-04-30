using Godot;
using Godot.Collections;
using System;
using System.Text.RegularExpressions;

public partial class CharacterCore : CharacterBody2D
{
	[Export]
	public int tile_size = 24;
	public Dictionary<string,Vector2> inputs = new Dictionary<string, Vector2>();

	[Export]
	public int baseHP = 100;
	[Export]
	public int baseATK = 50;
	[Export]
	public int baseDEF = 10;

	public int curHP = 100;

	[Export]
	public RayCast2D rayCast;

	[Export]
	public AnimatedSprite2D animatedSprite;

	[Export]
	public TileMapLayer floorData;

	[Signal]
	public delegate void PlayerActionEventHandler();

	[Signal]
	public delegate void PlayerAttackEventHandler();

	[Signal]
	public delegate void PlayerFailedAttackEventHandler();

	[Signal]
	public delegate void PlayerDiedEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		curHP = baseHP;

		animatedSprite.Play("idle_down");

		inputs.Add("move_right", Vector2.Right);
		inputs.Add("move_left", Vector2.Left);
		inputs.Add("move_up", Vector2.Up);
		inputs.Add("move_down", Vector2.Down);

		Position = Position.Snapped(Vector2.One * tile_size);
		Position += Vector2.One * tile_size / 2;

	}

	private bool GetHitIdiot(Enemy theHitter, int floornum)
	{
		int damage = ((int)theHitter.Get("baseATK") - baseDEF);
		GD.Print(damage);
		if (damage > 0)
		{

			baseHP -= damage;
			if (baseHP < 0)
			{
				//Gameover Idiot
				this.QueueFree();
				return true;
			}
		}
		return false;
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		foreach (var input in inputs.Keys)
		{
			if(@event.IsActionPressed(input)){
				move(input);
				curHP--;
				if (curHP <= 0)
				{
					EmitSignal(SignalName.PlayerDied);
					
				}
				else
				{
					EmitSignal(SignalName.PlayerAction);

				}
					
			}
		}
		if (@event.IsActionPressed("active_action"))
		{
			Attack();
			curHP--;
			if (curHP <= 0)
			{
				EmitSignal(SignalName.PlayerDied);
			}

			else
			{
				EmitSignal(SignalName.PlayerAction);
			}

		}

	}

	private void Attack()
	{
		if (!rayCast.IsColliding())
		{
			EmitSignal(SignalName.PlayerFailedAttack);

			GD.Print("The Attack Failed!");
		}
		else
		{
			//Play Attack Sprite Here


			EmitSignal(SignalName.PlayerAttack);
		}
	}

	private void move(string input)
	{
		switch (input)
		{
			case "move_right":
				animatedSprite.Play("idle_right");
				break;
			case "move_left":
				animatedSprite.Play("idle_left");
				break;
			case "move_up":
				animatedSprite.Play("idle_up");
				break;
			case "move_down":
				animatedSprite.Play("idle_down");
				break;
		}


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
