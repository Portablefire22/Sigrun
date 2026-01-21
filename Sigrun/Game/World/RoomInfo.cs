using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sigrun.Game.World;

    
public class RoomInfoTemp
{
	[JsonPropertyName("descr")]
	public string Description{get;set;}
	[JsonPropertyName("mesh path")]
	public string MeshPath{get;set;}
	[JsonPropertyName("shape")]
	[JsonConverter(typeof(RoomTypeConverter))]
	public RoomType Shape{get;set;}
	[JsonPropertyName("commonness")]
	public string Commonness{get;set;}
	[JsonPropertyName("zone1")]
	public string? Zone1{get;set;}
	[JsonPropertyName("zone2")]
	public string? Zone2{get;set;}
	[JsonPropertyName("zone3")]
	public string? Zone3{get;set;}
	[JsonPropertyName("zone4")]
	public string? Zone4{get;set;}
	[JsonPropertyName("zone5")]
	public string? Zone5{get;set;}
	[JsonPropertyName("disabledecals")]
	public bool DisableDecals{get;set;}

	public RoomInfo ToInfo()
	{
		var zones = new List<int>();
		zones.Add(Zone1 != null? int.Parse(Zone1): 0);
		zones.Add(Zone2 != null? int.Parse(Zone2): 0);
		zones.Add(Zone3 != null? int.Parse(Zone3): 0);
		zones.Add(Zone4 != null? int.Parse(Zone4): 0);
		zones.Add(Zone5 != null? int.Parse(Zone5): 0);
		return new RoomInfo()
		{
			Description = this.Description,
			MeshPath = this.MeshPath,
			Shape = this.Shape,
			Commonness = int.Parse(this.Commonness),
			Zones = zones.ToArray(),
			DisableDecals = this.DisableDecals,
		};
	}
}

public class RoomInfo
{
	[JsonPropertyName("descr")]
	public string Description{get;set;}
	[JsonPropertyName("mesh path")]
	public string MeshPath{get;set;}
	[JsonPropertyName("shape")]
	[JsonConverter(typeof(RoomTypeConverter))]
	public RoomType Shape{get;set;}
	[JsonPropertyName("commonness")]
	public int Commonness{get;set;}
	[JsonPropertyName("zone1")]
	public int[] Zones{get;set;}
	[JsonPropertyName("disabledecals")]
	public bool DisableDecals{get;set;}
}

public class RoomTypeConverter : JsonConverter<RoomType>
{
	public override RoomType Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		var str = reader.GetString().ToLower();
		switch (str)
		{
			case "1":
				return RoomType.Room1;
			case "2":
				return RoomType.Room2;
			case "2c":
				return RoomType.Room2C;
			case "3":
				return RoomType.Room3;
			case "4":
				return RoomType.Room4;
			case "255":
				return RoomType.ZoneTransition;
		}

		throw new JsonException( $"invalid room type {str}" );
	}

	public override void Write( Utf8JsonWriter writer, RoomType value, JsonSerializerOptions options )
	{
		throw new NotImplementedException();
	}

}