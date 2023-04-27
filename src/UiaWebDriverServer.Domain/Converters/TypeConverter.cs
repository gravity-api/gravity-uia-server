/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESOURCES
 */
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UiaWebDriverServer.Domain.Converters
{
    /// <summary>
    /// Allows safe serialization and deserialization of <see cref="Type"/>.
    /// </summary>
    public class TypeConverter : JsonConverter<Type>
    {
        /// <summary>
        /// Reads and converts the JSON to type T.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(Type), value.FullName);
            writer.WriteEndObject();
        }
    }
}
