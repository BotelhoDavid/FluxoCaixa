using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluxoCaixa.API.Converters
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        private readonly string _format = "dd-MM-yyyy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            
            if (string.IsNullOrEmpty(dateString))
                return default;

            if (DateTime.TryParseExact(dateString, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Fallback para o padrão ISO se falhar o dd-mm-yyyy
            return DateTime.Parse(dateString);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
        }
    }
}
