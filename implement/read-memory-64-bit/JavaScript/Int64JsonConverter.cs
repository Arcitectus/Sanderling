using System;

namespace read_memory_64_bit.JavaScript;


public class Int64JsonConverter : System.Text.Json.Serialization.JsonConverter<long>
{
    public override long Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            long.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        long integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}
