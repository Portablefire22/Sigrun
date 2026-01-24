using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sigrun.Engine.Entity;
using Sigrun.Engine.Entity.Components;
using Sigrun.Engine.Entity.Components.Physics;
using Sigrun.Engine.Entity.Components.Physics.Colliders;
using Sigrun.Engine.Logging;
using Sigrun.Engine.Maths;
using Sigrun.Engine.Rendering;
using Sigrun.Engine.Rendering.Primitives;
using Sigrun.Engine.Scenes;
using Sigrun.Game.Blitz;

namespace Sigrun.Game.World;

public class MapGenerator : Component
{

	public MapGenerator(GameObject parent) : base(parent) {}

	private int MapWidth { get; set; } = 18;
	private int MapHeight { get; set; } = 18;
	private int MapCentre { get; set; }
	private int MapSeed { get; set; }
	
	private Dictionary<Vector3, GameObject> _specialRooms;
	private readonly float _scalingFactor = 204.5f * 3;
	// private readonly float _positionScaler = 204.5f * 3;
	private readonly float _positionScaler = 204.5f * 0.03f;
	public GameObject[,] GeneratedMap { get; private set; }
	private int _updateMesh = 0;
	
	// The Actual map
	 private RoomType[,] MapTemp { get; set; }
	
	public int[] SNavMap { get; private set; }
	public int[] NavigatedMap { get; private set; }

	private static int ZoneAmount = 3;
	private static int MtSize = 19;
	
	// Room count per zone
	private int[] Room1Amount = new int[3];
	private int[] Room2Amount = new int[3];
	private int[] Room2CAmount = new int[3];
	
	private int[] Room3Amount = new int[3];
	private int[] Room4Amount = new int[3];

	private HashSet<GameObject> SavedRooms;
	private HashSet<GameObject> SavedEventsNormal; 
	private HashSet<GameObject> SavedEventsKeter;

	private Dictionary<string, RoomInfo> RoomInformation;

	private GameSettings _settings;

	private string[,] MapRoom; // Fill queue


	private ILogger _logger;
	
	public int state106;
	public int playerAngle;
	public string loadingScreen;

	public override void Startup()
	{
		_logger = LoggingProvider.NewLogger<MapGenerator>();
		
		_settings = new GameSettings()
		{
			Name = "Help",
			Difficulty = "Euclid",
			Seed = "sdahusa"
		};
		var file = new FileStream("Assets/rooms.json", FileMode.Open);
		var tmp = JsonSerializer.Deserialize<Dictionary<string, RoomInfoTemp>>(file);	
		RoomInformation = tmp.Select( x => new KeyValuePair<string,RoomInfo>(x.Key.ToLower(), x.Value.ToInfo()) ).ToDictionary();
		
		MapTemp = new RoomType[MapWidth + 1, MapHeight + 1];
		MapCentre = (MapWidth + 1) / 2; 
		GeneratedMap = new GameObject[MapWidth + 1, MapHeight + 1];

		if ( _settings.Seed.Length > 0 )
		{
			MapSeed = BlitzRng.generateSeedNumber(_settings.Seed.ToArray() );
		}
		else
		{
			if ( MapSeed == 0 )
			{
				var rnd = new Random();
				MapSeed = rnd.Next();
			}
		}

		_specialRooms = new Dictionary<Vector3, GameObject>();
			
		CreateMap( MapSeed );

		SNavMap = new int[MapWidth *MapHeight];
		NavigatedMap = new int[MapWidth *MapHeight];
		
		for ( int y = 0; y < MapHeight; y++ )
		{
			for ( int x = 0; x < MapWidth; x++ )
			{
				SNavMap[x + y * MapWidth] = (int)MapTemp[x, y];
				NavigatedMap[x + y * MapWidth] = MapTemp[x, y] > 0 ? 1 : 0;
			}
		}
	}
	
	private void CreateMap( int seed )
	{
		BlitzRng.BlitzSeedRnd(seed);
		GenerateMaze();
		CountRooms();
		// Ensure we have at least 5 room1s per zone
		ForceRoom1s();
		// Ensure we have a room2c and room4 in each zone;
		ForceRoom2c4s();

		MapRoom = new string[(int)RoomType.Room4 + 1, Room2Amount[0] + Room2Amount[1] + Room2Amount[2] + 3];
		DefineRooms();
		CreateRooms();
		CreateDoors();
		
		state106 = BlitzRng.BlitzRand( 160, 200 );
		// Scene.GetAllObjects(true).First( x => x.Name == "Spawn" ).Rotation = Rotation.From( 0, BlitzRng.BlitzRand( 160,200 ), 0 );
	}

	private void GenerateMaze()
	{
		int x = MapWidth / 2;
		int y = MapHeight - 2;
		
		// Starting room
		MapTemp[x, MapHeight - 1] = RoomType.Room1;
		int width, height;
		int temp = 0;
		do
		{
			width = BlitzRng.BlitzRand( 10, 15 );

			if ( x > MapWidth * 0.6 ) width = -width;
			else if ( x > MapWidth * 0.4 ) x = x - width / 2;
			
			// Ensure hallways dont exceed map array
			if ( x + width > MapWidth - 3 ) width = MapWidth - 3 - x;
			else if ( x + width < 2 ) width = -x + 2;

			x = Math.Min( x, x + width );
			width = Math.Abs( width );

			for ( int i = x; i <= x + width; i++ )
			{
				MapTemp[i, y] = RoomType.Room1;
			}

			// Vertical connections
			height = BlitzRng.BlitzRand( 3, 4 );
			if ( y - height < 1 ) height = y - 1;

			int yHallways = BlitzRng.BlitzRand( 4, 5 );
			if ( GetZone( y - height ) != GetZone( y - height + 1 ) ) height--;

			for ( int i = 1; i <= yHallways; i++ )
			{
				int x2 = Math.Max( 2, Math.Min( MapWidth - 2, BlitzRng.BlitzRand( x, x + width - 1 )));
				while ( MapTemp[x2 - 1, y - 1] != 0 || MapTemp[x2, y - 1] != 0 || MapTemp[x2 + 1, y - 1] != 0 ) x2++;

				if ( x2 < x + width )
				{
					int tempHeight;
					if ( i == 1 )
					{
						// Generate at least one connection to the next horizontal line
						tempHeight = height;
						if ( BlitzRng.BlitzRand( 1, 2 ) == 1 ) x2 = x;
						else x2 = x + width;
					}
					else tempHeight = BlitzRng.BlitzRand( 1, height );
					
					for ( int y2 = y - tempHeight; y2 <= y; y2++ )
					{
						if ( GetZone( y2 ) != GetZone( y2 + 1 ) )
							MapTemp[x2, y2] = RoomType.ZoneTransition; // Zone transition
						else MapTemp[x2, y2] = RoomType.Room1;
					}

					if ( tempHeight == height ) temp = x2;
				}
			}
			x = temp;
			y -= height;
		} while ( y >= 2 );
	}

	private void CountRooms()
	{
		int zone;
		for ( int y = 1; y <= MapHeight; y++ )
		{
			zone = GetZone( y );
			for ( int x = 1; x < MapWidth - 1; x++ )
			{
				if ( MapTemp[x, y] <= 0 ) continue;
				if ( MapTemp[x, y] < RoomType.ZoneTransition) MapTemp[x, y] = (RoomType) GetConnections( MapTemp, x, y );
				switch ( MapTemp[x, y] )
				{
					case RoomType.Room1:
						Room1Amount[zone]++;
						break;
					case RoomType.Room2:
						int connections = GetHorizontalConnections( MapTemp, x, y );
						if ( connections == 1 ) Room2CAmount[zone]++;
						else Room2Amount[zone]++;
						break;
					case RoomType.Room3:
						Room3Amount[zone]++;
						break;
					case RoomType.Room4:
						Room4Amount[zone]++;
						break;
				}
			}
		}
	}

	private int GetHorizontalConnections( RoomType[,] map, int x, int y )
	{
		return Math.Min( 1, (int)map[x + 1, y] ) + Math.Min( 1, (int)map[x - 1, y] );
	}
	private int GetVerticalConnections( RoomType[,] map, int x, int y )
	{
		return Math.Min( 1, (int)map[x, y + 1] ) + Math.Min( 1, (int)map[x, y - 1] );
	}

	private int GetConnections( RoomType[,] map, int x, int y )
	{
		return GetVerticalConnections( map, x, y ) + GetHorizontalConnections( map, x, y );
	}

	private int GetZone( int y )
	{
		return (int)Math.Min( ZoneAmount - 1, Math.Floor( (double)(MapWidth - y) / MapWidth * ZoneAmount ) );
	}
	
	private void ForceRoom1s()
	{
		int x2 = 0, y2 = 0, roomsLeft;
		
		// Force more room1s where needed
		for ( int i = 0; i <= ZoneAmount - 1; i++ )
		{
			roomsLeft = 5 - Room1Amount[i];
			if ( roomsLeft > 0 )
			{
				for ( int y = (MapHeight / ZoneAmount) * (2 - i) + 1;
				     y <= ((MapHeight / ZoneAmount) * ((2 - i) + 1)) - 2;
				     y++ )
				{
					for ( int x = 2; x <= MapWidth - 2; x++ )
					{
						if ( MapTemp[x, y] == 0 )
						{
							if ( GetConnections( MapTemp, x, y ) == 1 )
							{
								if ( MapTemp[x + 1, y] != 0 )
								{
									x2 = x + 1;
									y2 = y;
								} else if ( MapTemp[x - 1, y] != 0 )
								{
									x2 = x - 1;
									y2 = y;
								} else if ( MapTemp[x, y + 1] != 0 )
								{
									x2 = x;
									y2 = y + 1;
								} else if ( MapTemp[x, y - 1] != 0 )
								{
									x2 = x;
									y2 = y - 1;
								}

								var placed = false;
								if ( MapTemp[x2, y2] > RoomType.Room1 && MapTemp[x2, y2] < RoomType.Room4 )
								{
									switch ( MapTemp[x2, y2] )
									{
										case RoomType.Room2:
											if ( GetHorizontalConnections( MapTemp, x2, y2 ) == 2 )
											{
												Room2Amount[i]--;
												Room3Amount[i]++;
												placed = true;
											} else if ( GetVerticalConnections( MapTemp, x2, y2 ) == 2 )
											{
												Room2Amount[i]--;
												Room3Amount[i]++;
												placed = true;
											}

											break;
										case RoomType.Room3:
											Room3Amount[i]--;
											Room4Amount[i]++;
											placed = true;
											break;
									}

									if ( placed )
									{
										MapTemp[x2, y2]++;
										MapTemp[x, y] = RoomType.Room1;
										Room1Amount[i]++;
										roomsLeft--;
									}
								}
							}
						}
						if (roomsLeft == 0) break;
					}
					if (roomsLeft == 0) break;
				}
			}
		}
	}

	private void ForceRoom2c4s()
	{
		int zoneStart = 0, zoneEnd = 0;
		bool placed;
		for ( int i = 0; i <= 2; i++ )
		{
			switch ( i )
			{
				case 2:
					zoneStart = 2;
					zoneEnd = MapHeight / 3;
					break;
				case 1:
					zoneStart = MapHeight / 3 + 1;
					zoneEnd = (int)(MapHeight * (2.0 / 3.0) - 1);
					break;
				case 0:
					zoneStart = (int)(MapHeight * (2.0 / 3.0) + 1);
					zoneEnd = MapHeight - 2;
					break;
			}

			// We need at least 1 Room4
			if ( Room4Amount[i] < 1 )
			{
				placed = false;

				for ( int y = zoneStart; y <= zoneEnd; y++ )
				{
					for ( int x = 2; x <= MapWidth - 2; x++ )
					{
						// Check if adding a room1 is even possible
						if ( MapTemp[x, y] == RoomType.Room3 )
						{
							if ( MapTemp[x + 1, y] == 0 && MapTemp[x + 1, y + 1] == 0 && MapTemp[x + 1, y - 1] == 0 &&
							     MapTemp[x + 2, y] == 0 )
							{
								MapTemp[x + 1, y] = RoomType.Room1;
								placed = true;
							} else if ( MapTemp[x - 1, y] == 0 && MapTemp[x - 1, y + 1] == 0 &&
							            MapTemp[x - 1, y - 1] == 0 && MapTemp[x - 2, y] == 0 )
							{
								MapTemp[x - 1, y] = RoomType.Room1;
								placed = true;
							} else if ( MapTemp[x, y + 1] == 0 && MapTemp[x + 1, y + 1] == 0 &&
							            MapTemp[x - 1, y + 1] == 0 && MapTemp[x, y + 2] == 0 )
							{
								MapTemp[x, y + 1] = RoomType.Room1;
								placed = true;
							} else if ( MapTemp[x, y - 1] == 0 && MapTemp[x + 1, y - 1] == 0 &&
							            MapTemp[x - 1, y - 1] == 0 && MapTemp[x, y - 2] == 0 )
							{
								MapTemp[x, y - 1] = RoomType.Room1;
								placed = true;
							}

							if ( placed )
							{
								MapTemp[x, y] = RoomType.Room4;
								Room4Amount[i]++;
								Room3Amount[i]--;
								Room1Amount[i]++;
							}
						}
						if ( placed ) break;
					}
					if ( placed ) break;
				}
			}
			// We want at least one Room 2c
			if ( Room2CAmount[i] < 1 )
			{
				placed = false;
				zoneStart++;
				zoneEnd--;

				for ( int y = zoneStart; y <= zoneEnd; y++ )
				{
					for ( int x = 3; x <= MapWidth - 3; x++ )
					{
						if ( MapTemp[x, y] == RoomType.Room1 )
						{
							if ( MapTemp[x - 1, y] > 0 )
							{
								if ( (int) MapTemp[x, y - 1] + (int) MapTemp[x, y + 1] + (int) MapTemp[x + 2, y] == 0 )
								{
									if ( (int)MapTemp[x + 1, y - 2] + (int)MapTemp[x + 2, y - 1] +
									    (int)MapTemp[x + 1, y - 1] == 0 )
									{
										MapTemp[x, y] = RoomType.Room2;
										MapTemp[x + 1, y] = RoomType.Room2;
										MapTemp[x + 1, y - 1] = RoomType.Room1;
										placed = true;
									} else if ( (int)MapTemp[x + 1, y + 2] + (int)MapTemp[x + 2, y + 1] +
									           (int)MapTemp[x + 1, y + 1] == 0 )
									{
										MapTemp[x, y] = RoomType.Room2;
										MapTemp[x + 1, y] = RoomType.Room2;
										MapTemp[x + 1, y + 1] = RoomType.Room1;
										placed = true;
									}
								}
							} else if ( MapTemp[x + 1, y] > 0 )
							{
								if ( (int)MapTemp[x, y - 1] + (int)MapTemp[x, y + 1] + (int)MapTemp[x - 2, y] == 0 )
								{
									if ( (int)MapTemp[x - 1, y - 2] + (int)MapTemp[x - 2, y - 1] +
									    (int)MapTemp[x - 1, y - 1] == 0 )
									{
										MapTemp[x, y] = RoomType.Room2;
										MapTemp[x - 1, y] = RoomType.Room2;
										MapTemp[x - 1, y - 1] = RoomType.Room1;
										placed = true;
									}
									else if ((int)MapTemp[x - 1,y + 2] + (int) MapTemp[x - 2,y + 1] + (int)MapTemp[x - 1,y + 1] == 0) {
											MapTemp[x,y] = RoomType.Room2;
											MapTemp[x - 1,y] = RoomType.Room2;
											MapTemp[x - 1,y + 1] = RoomType.Room1;
											placed = true;
									}
								}
							} else if ((int)MapTemp[x,y - 1] > 0) {
								if ((int)MapTemp[x - 1,y] + (int) MapTemp[x + 1,y] + (int)MapTemp[x,y + 2] == 0) {
									if ((int)MapTemp[x - 2,y + 1] + (int) MapTemp[x - 1,y + 2] + (int)MapTemp[x - 1,y + 1] == 0) {
										MapTemp[x,y] = RoomType.Room2;
										MapTemp[x,y + 1] = RoomType.Room2;
										MapTemp[x - 1,y + 1] = RoomType.Room1;
										placed = true;
									} else if ((int)MapTemp[x + 2,y + 1] + (int) MapTemp[x + 1,y + 2] + (int)MapTemp[x + 1,y + 1] == 0) {
										MapTemp[x,y] = RoomType.Room2;
										MapTemp[x,y + 1] = RoomType.Room2;
										MapTemp[x + 1,y + 1] = RoomType.Room1;
										placed = true;
									}
								}
							}else if ((int)MapTemp[x,y + 1] > 0) {
								if ((int)MapTemp[x - 1,y] + (int)MapTemp[x + 1,y] + (int)MapTemp[x,y - 2] == 0) {
									if ((int)MapTemp[x - 2,y - 1] + (int)MapTemp[x - 1,y - 2] + (int)MapTemp[x - 1,y - 1] == 0) {
										MapTemp[x,y] = RoomType.Room2;
										MapTemp[x,y - 1] = RoomType.Room2;
										MapTemp[x - 1,y - 1] = RoomType.Room1;
										placed = true;
									} else if ((int)MapTemp[x + 2,y - 1] + (int)MapTemp[x + 1,y - 2] + (int)MapTemp[x + 1,y - 1] == 0) {
										MapTemp[x,y] = RoomType.Room2;
										MapTemp[x,y - 1] = RoomType.Room2;
										MapTemp[x + 1,y - 1] = RoomType.Room1;
										placed = true;
									}
								}
							}

							if ( placed )
							{
								Room2CAmount[i]++;
								Room2Amount[i]++;
							}
						}
						if(placed) break;
					}
					if(placed) break;
				}
			}
		}
	}

	private void DefineRooms()
	{
		// Zone 1
		int minPos = 1;
		int maxPos = Room1Amount[0] - 1;
		MapRoom[(int)RoomType.Room1, 0] = "start";
		SetRoom( "roompj", RoomType.Room1, (int) (Math.Floor( 0.1 * Room1Amount[0])), minPos, maxPos);
		SetRoom( "914", RoomType.Room1, (int) (Math.Floor( 0.3 * Room1Amount[0])), minPos, maxPos);
		SetRoom( "room1archive", RoomType.Room1, (int) (Math.Floor( 0.5 * Room1Amount[0])), minPos, maxPos);
		SetRoom( "room205", RoomType.Room1, (int) (Math.Floor( 0.6 * Room1Amount[0])), minPos, maxPos);

		MapRoom[(int)RoomType.Room2C, 0] = "lockroom";
		maxPos = Room2Amount[0] - 1;
		MapRoom[(int)RoomType.Room2, 0] = "room2closets";
		SetRoom( "room2testroom2", RoomType.Room2, (int) (Math.Floor(0.1 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2scps", RoomType.Room2, (int) (Math.Floor(0.2 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2storage", RoomType.Room2, (int) (Math.Floor(0.3 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2gw_b", RoomType.Room2, (int) (Math.Floor(0.4 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2sl", RoomType.Room2, (int) (Math.Floor(0.5 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room012", RoomType.Room2, (int) (Math.Floor(0.55 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2scps2", RoomType.Room2, (int) (Math.Floor(0.6 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room1123", RoomType.Room2, (int) (Math.Floor(0.7 * Room2Amount[0]  )), minPos, maxPos );
		SetRoom( "room2elevator", RoomType.Room2, (int) (Math.Floor(0.85 * Room2Amount[0]  )), minPos, maxPos );

		MapRoom[(int)RoomType.Room3, (int) Math.Floor( BlitzRng.BlitzRand( 0.2f, 0.8f ) * Room3Amount[0] )] = "room3storage";
		MapRoom[(int)RoomType.Room2C, (int) Math.Floor( 0.5 * Room3Amount[0] )] = "room1162";
		MapRoom[(int)RoomType.Room4, (int) Math.Floor( 0.3 * Room3Amount[0] )] = "room4info";
		
		// Zone 2 
		minPos = Room1Amount[0];
		maxPos = minPos + Room1Amount[1] - 1;
		
		SetRoom("room079", RoomType.Room1, Room1Amount[0] + (int)(Math.Floor( 0.15 * Room1Amount[1])), minPos, maxPos  );
		SetRoom("room106", RoomType.Room1, Room1Amount[0] + (int)(Math.Floor( 0.3 * Room1Amount[1])), minPos, maxPos  );
		SetRoom("008", RoomType.Room1, Room1Amount[0] + (int)(Math.Floor( 0.4 * Room1Amount[1])), minPos, maxPos  );
		SetRoom("room035", RoomType.Room1, Room1Amount[0] + (int)(Math.Floor( 0.5 * Room1Amount[1])), minPos, maxPos  );
		SetRoom("coffin", RoomType.Room1, Room1Amount[0] + (int)(Math.Floor( 0.7 * Room1Amount[1])), minPos, maxPos  );

		minPos = Room2Amount[0];
		maxPos = minPos + Room2Amount[1] - 1;
		
		MapRoom[(int)RoomType.Room2, minPos + (int) Math.Floor( 0.1* Room2Amount[1] )] = "room2nuke";
		SetRoom( "room2tunnel", RoomType.Room2, minPos + (int) Math.Floor( 0.25 * Room2Amount[1] ), minPos, maxPos );
		SetRoom( "room049", RoomType.Room2, minPos + (int) Math.Floor( 0.4 * Room2Amount[1] ), minPos, maxPos );
		SetRoom( "room2shaft", RoomType.Room2, minPos + (int) Math.Floor( 0.6 * Room2Amount[1] ), minPos, maxPos );
		SetRoom( "testroom", RoomType.Room2, minPos + (int) Math.Floor( 0.7 * Room2Amount[1] ), minPos, maxPos );
		SetRoom( "room2servers", RoomType.Room2, minPos + (int) Math.Floor( 0.9 * Room2Amount[1] ), minPos, maxPos );

		MapRoom[(int)RoomType.Room3, Room3Amount[0] + (int)Math.Floor( 0.3 * Room3Amount[1] )] = "room513";
		MapRoom[(int)RoomType.Room3, Room3Amount[0] + (int)Math.Floor( 0.6 * Room3Amount[1] )] = "room966";
		
		MapRoom[(int)RoomType.Room2C, Room2CAmount[0] + (int)Math.Floor( 0.5 * Room2CAmount[1] )] = "room2cpit";
		
		// Zone 3
		MapRoom[(int)RoomType.Room1, Room1Amount[0] + Room1Amount[1] + Room1Amount[2] - 2] = "exit1";
		MapRoom[(int)RoomType.Room1, Room1Amount[0] + Room1Amount[1] + Room1Amount[2] - 1] = "gateaentrance";
		MapRoom[(int)RoomType.Room1, Room1Amount[0] + Room1Amount[1] + Room1Amount[2]] = "room1lifts";

		minPos = Room2Amount[0] + Room2Amount[1];
		maxPos = minPos + Room2Amount[2] - 1;

		MapRoom[(int)RoomType.Room2, minPos + (int)Math.Floor( 0.1 * Room2Amount[2] )] = "room2poffices";
		SetRoom( "room2cafeteria", RoomType.Room2, minPos + (int) Math.Floor( (0.2 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2sroom", RoomType.Room2, minPos + (int) Math.Floor( (0.3 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2servers2", RoomType.Room2, minPos + (int) Math.Floor( (0.4 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2offices", RoomType.Room2, minPos + (int) Math.Floor( (0.45 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2offices4", RoomType.Room2, minPos + (int) Math.Floor( (0.5 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room860", RoomType.Room2, minPos + (int) Math.Floor( (0.6 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "medibay", RoomType.Room2, minPos + (int) Math.Floor( (0.7 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2poffices2", RoomType.Room2, minPos + (int) Math.Floor( (0.8 * Room2Amount[2])), minPos, maxPos );
		SetRoom( "room2offices2", RoomType.Room2, minPos + (int) Math.Floor( (0.9 * Room2Amount[2])), minPos, maxPos );

		int r2c = Room2CAmount[0] + Room2CAmount[1];
		MapRoom[(int)RoomType.Room2C, r2c] = "room2ccont";
		MapRoom[(int)RoomType.Room2C, r2c + 1] = "lockroom2";
		
		int r3 = Room3Amount[0] + Room3Amount[1];
		MapRoom[(int)RoomType.Room3, r3 + (int)Math.Floor( 0.3 * Room3Amount[2] )] = "room3servers";
		MapRoom[(int)RoomType.Room3, r3 + (int)Math.Floor( 0.7 * Room3Amount[2] )] = "room3servers2";
		MapRoom[(int)RoomType.Room3, r3 + (int)Math.Floor( 0.5 * Room3Amount[2] )] = "room3offices";
	}

	private void CreateRooms()
	{
		int zone, connections;
		int[] mapRoomId = new int[(int)RoomType.Room4 + 1];
		SavedRooms = new HashSet<GameObject>();
		GameObject room = null;

		for ( int y = MapHeight - 1; y >= 1; y-- )
		{
			if ( y < MapHeight / 3 + 1 ) zone = 3;
			else if ( y < MapHeight * (2f / 3f) ) zone = 2;
			else zone = 1;

			for ( int x = 1; x <= MapWidth - 2; x++ )
			{
				connections = GetConnections( MapTemp, x, y );
				if ( MapTemp[x, y] == RoomType.ZoneTransition)
				{
					RoomType type = connections == 2 ? RoomType.Room2 : RoomType.Room1;
					if ( y > MapHeight / 2 ) // Zone 2
						room = CreateRoom( zone, type, new Vector2(x * _positionScaler, y * _positionScaler), "checkpoint1" );
					else // Zone 3
						room = CreateRoom( zone, type, new Vector2(x * _positionScaler, y * _positionScaler), "checkpoint2" );
				} else if ( MapTemp[x, y] > 0 )
				{
					string? mapName = null;
					Rotation rot;
					switch ( connections )
					{
						case 1:
							if ( MapRoom[(int)RoomType.Room1, mapRoomId[(int)RoomType.Room1]] != null )
								mapName = MapRoom[(int)RoomType.Room1, mapRoomId[(int)RoomType.Room1]];
							room = CreateRoom( zone, RoomType.Room1, new Vector2(  x * _positionScaler, y * _positionScaler ), mapName);
							if (MapTemp[x, y + 1] > 0) rot = Rotation.From(0, 180f, 0);
							else if (MapTemp[x - 1, y] > 0) rot = Rotation.From(0, 270f, 0);
							else if ( MapTemp[x + 1, y] > 0 ) rot = Rotation.From( 0, 90f, 0 );
							else rot = Rotation.From( 0, 0f, 0 );
							room.Rotation = rot;
							mapRoomId[(int)RoomType.Room1]++;	
							break;
						case 2:
							if ( GetHorizontalConnections( MapTemp, x, y ) == 2 )
							{
								if ( MapRoom[(int)RoomType.Room2, mapRoomId[(int)RoomType.Room2]] != null )
									mapName = MapRoom[(int)RoomType.Room2, mapRoomId[(int)RoomType.Room2]];
								room = CreateRoom( zone, RoomType.Room2,
									new Vector2( x * _positionScaler, y * _positionScaler ), mapName );
								if ( BlitzRng.BlitzRand( 1, 2 ) == 1 ) rot = Rotation.From( 0, 90f, 0 );
								else rot = Rotation.From( 0, 0, 0 );
								room.Rotation = rot;
								mapRoomId[(int)RoomType.Room2]++;
							} else if ( GetVerticalConnections( MapTemp, x, y ) == 2 )
							{
								if ( MapRoom[(int)RoomType.Room2, mapRoomId[(int)RoomType.Room2]] != null )
									mapName = MapRoom[(int)RoomType.Room2, mapRoomId[(int)RoomType.Room2]];
								room = CreateRoom( zone, RoomType.Room2,
									new Vector2( x * _positionScaler, y * _positionScaler ), mapName );
								if ( BlitzRng.BlitzRand( 1, 2 ) == 1 ) rot = Rotation.From( 0, 180f, 0 );
								else rot = Rotation.From( 0, 0, 0 );
								room.Rotation = rot;
								mapRoomId[(int)RoomType.Room2]++;
							}
							else // Corner
							{
								if ( MapRoom[(int)RoomType.Room2C, mapRoomId[(int)RoomType.Room2C]] != null )
									mapName = MapRoom[(int)RoomType.Room2C, mapRoomId[(int)RoomType.Room2C]];
								room = CreateRoom( zone, RoomType.Room2C,
									new Vector2( x * _positionScaler, y * _positionScaler ), mapName );
								if ( MapTemp[x - 1, y] > 0 && MapTemp[x, y + 1] > 0 )
								{
									rot = Rotation.From( 0, 180, 0 );
								} else 	if ( MapTemp[x + 1, y] > 0 && MapTemp[x, y + 1] > 0 )
								{
									rot = Rotation.From( 0, 90, 0 );
								} else 	if ( MapTemp[x - 1, y] > 0 && MapTemp[x, y - 1] > 0 )
								{
									rot = Rotation.From( 0, 270, 0 );
								}
								else
								{
									rot = Rotation.From( 0, 0, 0 );
								}
								room.Rotation = rot;
								mapRoomId[(int)RoomType.Room2C]++;
							}
							break;
						case 3:	
							if ( MapRoom[(int)RoomType.Room3, mapRoomId[(int)RoomType.Room3]] != null )
								mapName = MapRoom[(int)RoomType.Room3, mapRoomId[(int)RoomType.Room3]];
							room = CreateRoom( zone, RoomType.Room3,
								new Vector2( x * _positionScaler, y * _positionScaler ), mapName );
							if ( MapTemp[x, y - 1] == 0 ) rot = Rotation.From( 0, 180, 0 );
							else if ( MapTemp[x - 1, y] == 0 ) rot = Rotation.From( 0, 90, 0 );
							else if ( MapTemp[x + 1, y] == 0 ) rot = Rotation.From( 0, 270, 0 );
							else rot = Rotation.From( 0,0,0 );
							room.Rotation = rot;
							mapRoomId[(int)RoomType.Room3]++;
							break;
						case 4:
							if ( MapRoom[(int)RoomType.Room4, mapRoomId[(int)RoomType.Room4]] != null )
								mapName = MapRoom[(int)RoomType.Room4, mapRoomId[(int)RoomType.Room4]];
							room = CreateRoom( zone, RoomType.Room4,
								new Vector2( x * _positionScaler, y * _positionScaler ), mapName );
							mapRoomId[(int)RoomType.Room4]++;
							break;
					}
				}

				if ( room != null )
				{
					SavedRooms.Add( room );
					room.Rotation *= Rotation.From( 0f,90f,0f );
				}
			}
		}

		CreateRoom( 0, RoomType.Room1, new Vector2( 62.5f * _positionScaler, 62.5f * _positionScaler), "gatea" );
		mapRoomId[(int)RoomType.Room1]++;
		CreateRoom( 0, RoomType.Room1, new Vector2( (MapWidth - 1)* _positionScaler, (MapHeight - 1) * _positionScaler), "pocketdimension" );
		mapRoomId[(int)RoomType.Room1]++;
		
		// Intro skipped because im not adding that 

		room = CreateRoom( 0, RoomType.Room1, new Vector2( 8, 0 ), "dimension1499" );
		SavedRooms.Add( room );
		mapRoomId[(int)RoomType.Room1]++;

		foreach ( var r in SavedRooms )
		{
			PreventRoomOverlap( r);
		}
		SavedRooms.Remove( room ); // Remove 1499 from map after checking for overlaps
	}

	private void PreventRoomOverlap( GameObject room )
	{
		if ( room.Tags.Contains( "disableOverlapCheck" ) ) return;
		var isIntersecting = false;

		if ( room.Tags.Contains( "checkpoint" ) || room.Tags.Contains( "start" ) ) return;

		foreach ( var r2 in SavedRooms )
		{
			if ( r2 != room && !r2.Tags.Contains( "disableOverlapCheck" ) )
			{
				if ( CheckRoomOverlap( room, r2 ) )
				{
					isIntersecting = true;
					break;
				}
			}
		}

		if ( !isIntersecting ) return;

		isIntersecting = false;
		if ( room.Tags.Contains( "room_room2" ) )
		{
			room.Rotation *= Rotation.From( 0,180, 0 );
			foreach ( var r2 in SavedRooms )
			{
				if ( r2 != room && !r2.Tags.Contains( "disableOverlapCheck" ) )
				{
					if ( CheckRoomOverlap( room, r2 ) )
					{
						// Rotating didnt work
						isIntersecting = true;
						room.Rotation *= Rotation.From(0, 180,  0 );
						break;
					}
				}
			}
		}
		else isIntersecting = true;

		if ( !isIntersecting ) return;

	}

	private bool CheckRoomOverlap( GameObject room, GameObject room2 )
	{
		var b = room.GetBounds();
		var b2 = room2.GetBounds();
		if (b == null || b2 == null) return false;
		if ( b.Maxs.X <= b2.Maxs.X || b.Maxs.Z <= b2.Maxs.Z ) return false;
		if ( b.Mins.X >= b2.Mins.X || b.Mins.Z >= b2.Mins.Z ) return false;
		return true;
	}

	private GameObject RoomObjectFromName( int zone, RoomType roomType, Vector2 position, string name )
	{
		GameObject roomObject;
		name = name.ToLower();
		string prefab = GetRoomByName( name );
		
		// roomObject = GameObject.Clone( prefab );
		roomObject = GameObject.FromRMeshFile(prefab, name);
		if ( roomObject == null )
		{
			_logger.LogError( $"Failed finding {name} @ {prefab}" );
		}
		roomObject.Position = new Vector3( position.X, 0, position.Y);
		roomObject.Tags.Add( name );
		roomObject.Tags.Add( "roomMesh" );
		roomObject.Tags.Add( $"zone_{zone}" );
		roomObject.Tags.Add( $"room_{roomType}" );
		roomObject.Scale = new Vector3( 0.003f);
		if ( name.Contains( "checkpoint" ) ) roomObject.Tags.Add( "checkpoint" );
		
		// foreach ( var light in roomObject.GetAllObjects(true).Where( o => o.Tags.Contains( "light" ) ) )
		// {
		// 	var p = light.AddComponent<LightController>();
		// 	p._mapPos = new Vector3( float.Floor( position.x / _positionScaler), float.Floor(  position.y / _positionScaler), 0 );
		// 	light.GetComponent<Light>().Shadows = false;
		// 	// light.Destroy();
		// 	// light.GetComponent<LightController>()._mapPos = new Vector3( position.x, position.y, 0);
		// }

		// roomObject.GetComponent<ModelCollider>().Static = true;

		var comp = new ScpRoom(roomObject);
		comp.Name = name;
		comp.Shape = roomType;
		comp.MeshPath = prefab;
		comp.Zone = zone;
		roomObject.Components.Add(comp);

		SpawnObject(roomObject);
		
		return roomObject;
	}
	
	private GameObject CreateRoom( int zone, RoomType roomType, Vector2 position, string name )
	{
		try
		{
			GameObject roomObject;
			if ( name != null && name.Length > 0 )
			{
				return RoomObjectFromName( zone, roomType, position, name );
			}


			int temp = 0;
			foreach ( var (roomName, info) in RoomInformation )
			{
				for ( int i = 0; i <= 4; i++ )
				{
					if ( info.Zones[i] == zone )
					{
						if ( info.Shape == roomType )
						{
							temp += info.Commonness;
							break;
						}
					}
				}
			}

			int randomRoom = BlitzRng.BlitzRand( 1, 10 );
			temp = 0;
			foreach ( var (roomName, info) in RoomInformation )
			{
				for ( int i = 0; i <= 4; i++ )
				{
					if ( info.Zones[i] == zone && info.Shape == roomType )
					{
						temp += info.Commonness;
						if ( randomRoom > temp - info.Commonness && randomRoom <= temp )
						{
							return RoomObjectFromName( zone, roomType, position, roomName );
						}
					}
				}
			}

			_logger.LogError( $"{name} Zone: {zone} Type: {roomType}" );

			// They tell me this is unreachable
			throw new UnreachableException();
		}
		catch ( Exception e )
		{
			_logger.LogError( $"{e}" );
			var x = new GameObject();
			var r = new Renderer(x, new Model()
			{
				Meshes = new[] { new CubeMesh(new Vector3(1)) }
			});
			x.Position = new Vector3( position.X, position.Y, 0 );
			x.Scale = new Vector3( 9 );
			return x;
		}
	}

	private string GetRoomByName( string name )
	{
		var info = RoomInformation[name];
		return info.MeshPath;
	} 
	
	private void SetRoom( string roomName, RoomType roomType, int pos, int minPos, int maxPos )
	{
		var looped = false;
		var canPlace = true;
		while ( MapRoom[(int)roomType, pos] != null )
		{
			pos++;
			if ( pos > maxPos )
			{
				if ( !looped )
				{
					pos = minPos + 1;
					looped = true;
				}
				else
				{
					canPlace = false;
					break;
				}
			}
		}

		if ( canPlace )
		{
			MapRoom[(int)roomType, pos] = roomName;
		}
	}

	private GameObject? FindRoom( int x, int z )
	{
		
		return SavedRooms
			.FirstOrDefault( r => MathsUtility.AlmostEqual(r.Position.X / _positionScaler, x, 0.1f ) 
			                      && (MathsUtility.AlmostEqual(r.Position.Y / _positionScaler, z, 0.1f )));
	}

	private void CreateDoors()
	{
		int iZoneTransition0 = 13;
		int iZoneTransition1 = 7;
		int zone = 0, type = 0;
		var shouldSpawnDoor = false;
		for ( int y = MapHeight; y >= 0; y-- )
		{
			if ( y < iZoneTransition1 - 1 ) zone = 3;
			else if ( y >= iZoneTransition1 && y < iZoneTransition0 ) zone = 2;
			else zone = 1;

			for ( int x = MapWidth; x >= 0; x-- )
			{
				if ( MapTemp[x, y] > 0 )
				{
					if ( zone == 2 ) type = 2;
					else type = 0;
				}

				var room = FindRoom( x, y );
				if ( room != null)
				{
					var roomInfo = room.GetComponent<ScpRoom>();
					if (roomInfo != null) continue;
					var angle = room.Rotation.Yaw % 360;
					var tmp = angle;
					angle = Math.Abs( angle );
					shouldSpawnDoor = false;
					switch ( roomInfo.Shape )
					{
						case RoomType.Room1:
							if ( Math.Abs(angle - 90) < 1 ) shouldSpawnDoor = true;
							break;
						case RoomType.Room2:
							if ( Math.Abs(angle - 90) < 1  || Math.Abs( angle - 270 ) < 1) shouldSpawnDoor = true;
							break;
						case RoomType.Room2C:
							if ( Math.Abs(angle) < 1  || Math.Abs( angle - 90) < 1) shouldSpawnDoor = true;
							break;
						case RoomType.Room3:
							if ( Math.Abs(angle) < 1 || Math.Abs( angle - 90 ) < 1 || Math.Abs( angle - 180 ) < 1) shouldSpawnDoor = true;
							break;
						default:
							shouldSpawnDoor = true;
							break;
					}

					if ( shouldSpawnDoor )
					{
						if ( x < MapWidth )
						{
							if ( MapTemp[x + 1, y] > 0 )
							{
								var pos = new Vector3( x * _positionScaler + _positionScaler / 2f, y * _positionScaler, 0 );
								roomInfo.DoorTwo = NewDoor( Math.Max( BlitzRng.BlitzRand( -3, 1 ), 0 ) > 0, type, pos , 90f);
							}
						}
					}

					shouldSpawnDoor = false;
					switch ( roomInfo.Shape )
					{
						case RoomType.Room1:
							if ( Math.Abs(angle - 180) < 1 ) shouldSpawnDoor = true;
							break;
						case RoomType.Room2:
							if ( Math.Abs(angle) < 1  || Math.Abs( angle - 180) < 1) shouldSpawnDoor = true;
							break;
						case RoomType.Room2C:
							if ( Math.Abs(angle - 90) < 1  || Math.Abs( angle - 180) < 1) shouldSpawnDoor = true;
							break;
						case RoomType.Room3:
							if ( Math.Abs(angle - 90) < 1  || Math.Abs( angle - 180) < 1 || Math.Abs( angle - 270 ) < 1) shouldSpawnDoor = true;
							break;
						default:
							shouldSpawnDoor = true;
							break;
					}

					if ( shouldSpawnDoor )
					{
						if ( x < MapHeight )
						{
							if ( MapTemp[x, y + 1] > 0 )
							{
								var pos = new Vector3( x * _positionScaler, y * _positionScaler + _positionScaler /2.0f, 0 );
								roomInfo.DoorTwo = NewDoor( Math.Max( BlitzRng.BlitzRand( -3, 1 ), 0 ) > 0, type, pos , 0);
							}
						}
					}
				}
			}
		}
	}

	private GameObject NewDoor( bool open, int big, Vector3 pos, float angle)
	{

		var obj = new GameObject();
		var mod2 = new Model() { Meshes = [new CubeMesh(new Vector3(2))] };
		var rigidbody2 = new Rigidbody(obj) {Collider = new BoxCollider(obj) };
		var renderer2 = new Renderer(obj, mod2);
		obj.Components.Add(renderer2);
		
		// var door = obj.GetComponent<SlidingDoor>();
		// door._isOpen = open;
		// door.AutoClose = (open && big == 0 && BlitzRng.BlitzRand( 1, 8 ) == 1);
		obj.Position = pos;
		obj.Rotation = Rotation.From( 0, angle + 90f, 0 );
		SpawnObject(obj);
		return obj;
	}
}