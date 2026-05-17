using System;
using System.Globalization;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

public static class GameJsonSettings
{
    static readonly JsonConverter[] MathematicsConverters =
    {
        new Int2JsonConverter(),
        new Int3JsonConverter(),
        new Int4JsonConverter()
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyRuntimeDefaultSettings()
    {
        ApplyDefaultSettings();
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void ApplyEditorDefaultSettings()
    {
        ApplyDefaultSettings();
    }
#endif

    public static void ApplyDefaultSettings()
    {
        JsonConvert.DefaultSettings = CreateDefaultSettings;
    }

    public static JsonSerializerSettings CreateDefaultSettings()
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };
        AddGameConverters(settings);
        return settings;
    }

    public static void AddGameConverters(JsonSerializerSettings settings)
    {
        if (settings == null)
            return;

        foreach (var converter in MathematicsConverters)
        {
            if (!HasConverter(settings, converter.GetType()))
                settings.Converters.Add(converter);
        }
    }

    static bool HasConverter(JsonSerializerSettings settings, Type converterType)
    {
        for (int i = 0; i < settings.Converters.Count; i++)
        {
            if (settings.Converters[i].GetType() == converterType)
                return true;
        }

        return false;
    }

    static int[] ReadIntValues(JsonReader reader, int count)
    {
        if (reader.TokenType == JsonToken.String)
            return ParseIntString((string)reader.Value, count);

        if (reader.TokenType == JsonToken.StartArray)
        {
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                if (!reader.Read())
                    throw new JsonSerializationException("Unexpected end while reading Unity.Mathematics vector array.");

                values[i] = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
            }

            while (reader.TokenType != JsonToken.EndArray && reader.Read())
            {
            }

            return values;
        }

        if (reader.TokenType != JsonToken.StartObject)
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} while reading Unity.Mathematics vector.");

        var result = new int[count];
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                break;

            if (reader.TokenType != JsonToken.PropertyName)
                continue;

            string propertyName = (string)reader.Value;
            if (!reader.Read())
                break;

            int index = ComponentIndex(propertyName);
            if (index >= 0 && index < count)
                result[index] = Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture);
            else
                reader.Skip();
        }

        return result;
    }

    static int ComponentIndex(string propertyName)
    {
        switch (propertyName)
        {
            case "x":
                return 0;
            case "y":
                return 1;
            case "z":
                return 2;
            case "w":
                return 3;
            default:
                return -1;
        }
    }

    static int[] ParseIntString(string value, int count)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new int[count];

        int start = value.IndexOf('(');
        int end = value.LastIndexOf(')');
        if (start >= 0 && end > start)
            value = value.Substring(start + 1, end - start - 1);

        string[] parts = value.Split(',');
        if (parts.Length < count)
            throw new JsonSerializationException($"Cannot parse Unity.Mathematics vector from '{value}'.");

        var result = new int[count];
        for (int i = 0; i < count; i++)
            result[i] = int.Parse(parts[i].Trim(), CultureInfo.InvariantCulture);

        return result;
    }

    static void WriteComponents(JsonWriter writer, params int[] values)
    {
        writer.WriteStartObject();
        WriteComponent(writer, "x", values[0]);
        WriteComponent(writer, "y", values[1]);
        if (values.Length > 2)
            WriteComponent(writer, "z", values[2]);
        if (values.Length > 3)
            WriteComponent(writer, "w", values[3]);
        writer.WriteEndObject();
    }

    static void WriteComponent(JsonWriter writer, string name, int value)
    {
        writer.WritePropertyName(name);
        writer.WriteValue(value);
    }

    sealed class Int2JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(int2);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (int2)value;
            WriteComponents(writer, v.x, v.y);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int[] values = ReadIntValues(reader, 2);
            return new int2(values[0], values[1]);
        }
    }

    sealed class Int3JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(int3);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (int3)value;
            WriteComponents(writer, v.x, v.y, v.z);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int[] values = ReadIntValues(reader, 3);
            return new int3(values[0], values[1], values[2]);
        }
    }

    sealed class Int4JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(int4);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (int4)value;
            WriteComponents(writer, v.x, v.y, v.z, v.w);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int[] values = ReadIntValues(reader, 4);
            return new int4(values[0], values[1], values[2], values[3]);
        }
    }
}
