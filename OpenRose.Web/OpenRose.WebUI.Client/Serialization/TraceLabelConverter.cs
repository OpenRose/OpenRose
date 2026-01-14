// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedConstants;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRose.WebUI.Client.Serialization
{

	/// <summary>
	/// A custom JSON converter for <see cref="string"/> values used on the client side
	/// when sending data to the API layer.
	/// 
	/// This converter enforces sentinel logic only during serialization (outbound):
	/// <list type="bullet">
	///   <item>
	///     <description>Property omitted entirely → treated as sentinel, so the field is not sent and existing repository value is preserved.</description>
	///   </item>
	///   <item>
	///     <description>Property explicitly set to null → serialized as <c>null</c>, indicating the client intends to clear the value.</description>
	///   </item>
	///   <item>
	///     <description>Property set to a non-null string → serialized as that string, indicating the client intends to update the value.</description>
	///   </item>
	/// </list>
	/// 
	/// On deserialization (inbound), this converter does not alter System.Text.Json’s default behavior:
	/// missing properties and explicit nulls both become <c>null</c>.
	/// </summary>
	public class TraceLabelConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Use default string handling
			return reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
		}


		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{

			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value);
		}
	}
}
