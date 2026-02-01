using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KonSanad.Core.Converters
{
    /// <summary>
    /// Serializes DateTime? as "yyyy-MM-dd" and reads the same format (falls back to parse).
    /// Applied to DTO StartDate so the JSON response will contain date-only string.
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateTime?>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s)) return null;

            if (DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            // fallback to general parse for backward compatibility
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                return dt;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Use invariant culture and format as yyyy-MM-dd
                writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
