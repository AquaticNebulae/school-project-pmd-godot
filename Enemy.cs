using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public int tileSize = 24;
	[Export]
	public int MoveTurn = 2;



	[Export]
	TileMapLayer tileMapLayer { get; set; }
	[Export]
	CharacterBody2D player {  get; set; }
	[Export]
	Line2D visualPath { get; set; }
	[Export]
	AnimatedSprite2D animatedSprite { get; set; }

	AStarGrid2D pathFinder = new AStarGrid2D();

	List<Vector2> PathtoPlayer;
	int turnCounter = 1;

	public override void _Ready()
	{
		visualPath.GlobalPosition = new Vector2((float)tileSize / (float)2.0, (float)tileSize / (float)2.0);

		player.Connect("PlayerAction", new Callable(this, nameof(_moveAi)));

		pathFinder.Region = tileMapLayer.GetUsedRect();
		pathFinder.CellSize = new Vector2(tileSize, tileSize);
		pathFinder.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		pathFinder.Update();

		foreach(Vector2I cell in tileMapLayer.GetUsedCells())
		{
			pathFinder.SetPointSolid(cell, true);
		}

		_moveAi();
	}

	private void _moveAi()
	{
		PathtoPlayer = pathFinder.GetPointPath((Vector2I) GlobalPosition / tileSize, (Vector2I) player.GlobalPosition / tileSize).ToList<Vector2>();
		visualPath.Points = PathtoPlayer.ToArray();


		if(turnCounter != MoveTurn)
		{
			turnCounter++;
		}
		else
		{
			if(PathtoPlayer.Count > 1)
			{
				PathtoPlayer.RemoveAt(0);
				Vector2 goToPos = PathtoPlayer.First() + new Vector2((float)tileSize / (float)2.0, (float)tileSize / (float)2.0);

				//Animation
				if (goToPos.X < GlobalPosition.X)
				{
					animatedSprite.Play("idle_left");
				}
				else if (goToPos.X > GlobalPosition.X)
				{
					animatedSprite.Play("idle_right");
				}
				else if (goToPos.Y < GlobalPosition.Y)
				{
					animatedSprite.Play("idle_up");
				}
				else if(goToPos.Y > GlobalPosition.Y)
				{
					animatedSprite.Play("idle_down");
				}


					GlobalPosition = goToPos;

				visualPath.Points = PathtoPlayer.ToArray();

				turnCounter = 1;


			}
			else //Action to hit the player
			{

			}
		}

	}

}
