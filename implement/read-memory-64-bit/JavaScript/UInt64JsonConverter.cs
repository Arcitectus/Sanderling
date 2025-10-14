using System;

namespace read_memory_64_bit.JavaScript;

public class UInt64JsonConverter : System.Text.Json.Serialization.JsonConverter<ulong>
{
    public override ulong Read(
        ref System.Text.Json.Utf8JsonReader reader,
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options) =>
            ulong.Parse(reader.GetString()!);

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer,
        ulong integer,
        System.Text.Json.JsonSerializerOptions options) =>
            writer.WriteStringValue(integer.ToString());
}
