using System.Text.Json.Serialization;
using System.Text.Json;

namespace SteamPlaytimeTracker.Utility.Converter;
internal sealed class StringToIntJsonConverter : JsonConverter<int?>
{
	public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if(reader.TokenType == JsonTokenType.String)
		{
			var stringValue = reader.GetString();
			if (int.TryParse(stringValue, out int intValue))
			{
				return intValue;
			}
			return null;
		}
		else if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.GetInt32();
		}
		return null;
	}

	public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
