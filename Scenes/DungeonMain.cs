using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

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
	public RichTextLabel label {  get; set; }

	[Export]
	public Label HP {  get; set; }
	[Export]
	public Label floor {  get; set; }

	[Export]
	public Label GAMEOVERTXT { get; set; }
	[Export] 
	public ColorRect GAMEOVERCOL { get; set; }

	[Export]
	public PackedScene Mob1 {  get; set; }
	[Export]
	public Button strButton { get; set; }
	[Export]
	public Label introLBL { get; set; }

	private CharacterBody2D mob3 {  get; set; }

	[Export]
	public Enemy mob2 { get; set; }
	
	private AudioStreamPlayer dungOST;
	
	[Export]
	public AnimatedSprite2D enemySprite;

	private Dictionary<string, Vector2> _inputs = new Dictionary<string, Vector2>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		dungOST = GetNode<AudioStreamPlayer>("DungeonOST");
		dungOST.Stop();
		GAMEOVERCOL.Visible = false;
		GAMEOVERTXT.Visible = false;
		_characterBody2D = GetNode<CharacterBody2D>("Character");
		_inputs = (Dictionary<string, Vector2>)_characterBody2D.Get("inputs");

		

		mob3 = GetNode<CharacterBody2D>("Enemy");


		_floor = GetNode<TileMapLayer>("Dungeon/Floor");
		_wall = GetNode<TileMapLayer>("Dungeon/Wall");
		Mob1 = GD.Load<PackedScene>("res://Enemy.tscn");
		
		_characterBody2D.Visible = false;
		_floor.Visible = false;
		_wall.Visible = false;



		_characterBody2D.Connect(CharacterCore.SignalName.PlayerAction, new Callable(this, nameof(MobSpawn)));
		_characterBody2D.Connect(CharacterCore.SignalName.PlayerAttack, new Callable(this, nameof(CalcPlayerAttack)));
		_characterBody2D.Connect(CharacterCore.SignalName.PlayerFailedAttack, new Callable(this, nameof(FailedAttack)));
		mob3.Connect(Enemy.SignalName.EnemyHitYOU, new Callable(this, nameof(EnemyAttack)));
		_characterBody2D.Connect(CharacterCore.SignalName.PlayerDied, new Callable(this, nameof(GameOver)));	
	}

	public void GameOver()
	{
		dungOST.Stop();
		_characterBody2D.Visible = false;
		_floor.Visible = false;
		_wall.Visible = false;
		GAMEOVERCOL.Visible = true;
		GAMEOVERTXT.Visible = true;

		strButton.Visible = true;
		introLBL.Visible = true;
	}
	public void NewGame()
	{
		dungOST.Play();
		_characterBody2D.Position = new Vector2(120, 120);
		_characterBody2D.Position = _characterBody2D.Position.Snapped(Vector2.One * (int)_characterBody2D.Get("tile_size"));
		_characterBody2D.Position += Vector2.One * (int)_characterBody2D.Get("tile_size") / 2;

		GetTree().CallGroup("enemies", Node.MethodName.QueueFree);
		_wall._Ready();
		_characterBody2D.Set(CharacterCore.PropertyName.curHP,_characterBody2D.Get(CharacterCore.PropertyName.baseHP));
		GAMEOVERCOL.Visible = false;
		GAMEOVERTXT.Visible = false;
		strButton.Visible = false;
		introLBL.Visible =  false;
		label.Text = $"Floor {floornum}";
		floor.Text = $"{floornum}F";

		HP.Text = (string)_characterBody2D.Get(CharacterCore.PropertyName.curHP);

		pathFinder.Region = _wall.GetUsedRect();
		pathFinder.CellSize = new Vector2(24, 24);
		pathFinder.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		pathFinder.Update();

		_characterBody2D.Visible = true;
		_floor.Visible = true;
		_wall.Visible = true;


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

					GetTree().CallGroup("enemies", Node.MethodName.QueueFree); 
					_wall._Ready();

					floornum++;
					label.AddText($"\nFloor {floornum}");
					floor.Text = $"{floornum}F";

				}
			}
		}
		
	}

	public void FailedAttack()
	{
		label.AddText($"\nThe Attack Failed!");
	}

	public bool EnemyAttack()
	{
		GD.Print("Get hit idiot");
		if ((bool)_characterBody2D.Call(CharacterCore.MethodName.GetHitIdiot, [new Enemy(),floornum])){
			//YOU DIE
			GetTree().CallGroup("enemies", Node.MethodName.QueueFree);

			GAMEOVERCOL.Visible = true;
			GAMEOVERTXT.Visible = true;

			GameOver();
		}
		HP.Text = (string)_characterBody2D.Get(CharacterCore.PropertyName.curHP);
		return false;
	}

	public bool CalcPlayerAttack()
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
			label.AddText($"\nEris hit nearby enemies for {_characterBody2D.Get("baseATK")} damage!");
			if ((bool)enemy.Call("GetHitIdiot"))
			{
				_characterBody2D.Set(CharacterCore.PropertyName.curHP, (int)_characterBody2D.Get(CharacterCore.PropertyName.curHP) + 50);

				label.AddText($"\nAn Enemy was defeated!");
			}
			
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
			if(rand.Next(100) < spawnPer)
			{

				
				Enemy eny = (Enemy)mob3.Duplicate();
				eny.Set(Enemy.PropertyName.baseHP, (eny.baseHP + ((floornum-1) * 5 )));
				eny.Set(Enemy.PropertyName.baseDEF, eny.baseDEF + ((floornum-1) * 5));

				eny.GlobalPosition = spawn * 24;
				eny.Set("tileMapLayer", _wall);
				eny.Set("player", _characterBody2D);
				eny.Set("animatedSprite", enemySprite);

				eny.AddToGroup("enemies");
				eny.Visible = true;
				AddChild(eny);

				
				spawnPer = basePer + (floornum * 3);
			}
			else
			{
				spawnPer += rand.Next(10) + (floornum-1);
			}
		}


			return false;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		HP.Text = (string)_characterBody2D.Get(CharacterCore.PropertyName.curHP);



	}
}
