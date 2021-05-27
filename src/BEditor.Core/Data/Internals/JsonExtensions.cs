﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BEditor.Data.Internals
{
    internal static class JsonExtensions
    {
        public static void Write(this Utf8JsonWriter writer, EditingProperty property, object value)
        {
            if (property.Serializer is not null)
            {
                writer.WritePropertyName(property.Name.Split(',')[0]);

                property.Serializer.Write(writer, value);
            }
        }

        public static void Write<T>(this Utf8JsonWriter writer, string name, T value, Action<Utf8JsonWriter, T> write)
        {
            writer.WritePropertyName(name.Split(',')[0]);
            write(writer, value);
        }

        public static object? Read(this JsonElement element, EditingProperty property)
        {
            foreach (var item in property.Name.Split(','))
            {
                if (element.TryGetProperty(item, out var value))
                {
                    return property.Serializer!.Read(value);
                }
            }

            if (property.Initializer is not null)
            {
                return property.Initializer.Create();
            }

            return null;
        }

        public static T Read<T>(this JsonElement element, string name, Func<JsonElement, T> read, Func<T> getdefault)
        {
            foreach (var item in name.Split(','))
            {
                if (element.TryGetProperty(item, out var value))
                {
                    return read(value);
                }
            }

            return getdefault();
        }
    }
}