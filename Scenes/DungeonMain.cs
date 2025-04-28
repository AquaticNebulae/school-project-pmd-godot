using Godot;
using Godot.Collections;
using System;

public partial class DungeonMain : Node
{

	private CharacterBody2D _characterBody2D;

	private TileMapLayer _floor;
	private TileMapLayer _wall;

	private int turncounter;
	private float basePer = -25;
	private float spawnPer = -25;
	public int floornum = 1;

	AStarGrid2D pathFinder = new AStarGrid2D();

	[Export]
	public PackedScene Mob1 {  get; set; }

	[Export]
	public AnimatedSprite2D enemySprite;

	private Dictionary<string, Vector2> _inputs = new Dictionary<string, Vector2>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		Mob1 = GD.Load<PackedScene>("res://Enemy.tscn");

		_characterBody2D = GetNode<CharacterBody2D>("Character");


		_characterBody2D.Connect("PlayerAction", new Callable(this, nameof(MobSpawn)));

		_characterBody2D.Connect("PlayerAttack", new Callable(this, nameof(CalcAttack)));

		_inputs = (Dictionary<string,Vector2>)_characterBody2D.Get("inputs");

		_floor = GetNode<TileMapLayer>("Dungeon/Floor");
		_wall = GetNode<TileMapLayer>("Dungeon/Wall");

		

		pathFinder.Region = _wall.GetUsedRect();
		pathFinder.CellSize = new Vector2(24, 24);
		pathFinder.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		pathFinder.Update();

	}

	public override void _UnhandledInput(InputEvent @event)
	{
		foreach (var input in _inputs.Keys)
		{
			if (@event.IsActionPressed(input))
			{
				//Mob Spawning


				



				bool onStairsChk = (bool)_characterBody2D.Call("OnStairs", _characterBody2D.Position);
				if (onStairsChk)
				{
					//Regenerate Dungeon
					//Trigger UI Change
					_characterBody2D.Position = new Vector2(120, 120);
					_characterBody2D.Position = _characterBody2D.Position.Snapped(Vector2.One * (int)_characterBody2D.Get("tile_size"));
					_characterBody2D.Position += Vector2.One * (int)_characterBody2D.Get("tile_size") / 2;

					GetTree().CallGroup("enemies", Node.MethodName.QueueFree); 
					_wall._Ready();
					
					

				}
			}
		}
		
	}

	public bool CalcAttack()
	{

		Vector2 playerPos = _characterBody2D.Position;

		Array<Node> enemyList = GetTree().GetNodesInGroup("enemies");

		Array<Node> hitList = new Array<Node>();

		foreach (Enemy enemy in enemyList)
		{
			if(Math.Abs(enemy.GlobalPosition.DistanceTo(playerPos) / 24) == 1.00)
			{
				hitList.Add(enemy);
			}
		}

		foreach(Enemy enemy in hitList)
		{
			enemy.Call("GetHitIdiot");
		}

		return false;
	}

	public bool MobSpawn()
	{
		

		Random rand = new Random();

		Vector2I cordChk;
		Vector2I spawn = new Vector2I(-1, -1); ;

		for(int x = 0; x < 40; x++)
		{
			if(spawn != new Vector2I(-1, -1))
			{
				break;
			}

			for(int y = 0; y < 40; y++)
			{
				cordChk = new Vector2I(x, y);

				//If Empty
				if(_wall.GetCellAtlasCoords(cordChk) == new Vector2I(-1, -1))
				{
					

					if (GD.Randf() < 0.01)
					{
						
						spawn = new Vector2I(x, y); break;
					}
				}

			}
		}
		
		if(spawn == new Vector2I(-1, -1))
		{
			return false;
		}
		else
		{
			GD.Print(spawnPer);
			if(rand.Next(100) < spawnPer)
			{

				Enemy eny = Mob1.Instantiate<Enemy>();
				eny.GlobalPosition = spawn * 24;
				eny.Set("tileMapLayer", _wall);
				eny.Set("player", _characterBody2D);
				eny.Set("animatedSprite", enemySprite);

				eny.AddToGroup("enemies");
				AddChild(eny);

				
				spawnPer = basePer;
			}
			else
			{
				spawnPer += rand.Next(10);
			}
		}


			return false;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		



	}
}
