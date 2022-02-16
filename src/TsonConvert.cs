﻿////////////////////////////////////////////////////////////////////////////////////
//    MIT License
//
//    Copyright (c) 2020 Atilla Lonny (https://github.com/atillabyte/tson)
//
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mapster;

namespace Tson
{
    public static class TsonConvert
    {
        /// <summary>
        /// Serializes the specified object to a TSON string using the formatting specified.
        /// </summary>
        /// <param name="value"> The object to serialize. </param>
        /// <param name="formatting"> Indicates how the output should be formatted. </param>
        /// <param name="options"> An optional configuration object for serialization. </param>
        /// <returns>
        /// A TSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object input, Formatting format = Formatting.None, SerializationOptions options = null)
        {
            var writer = new TsonWriter(options ?? new SerializationOptions());

            switch (format)
            {
                case Formatting.None:
                    return writer.Serialize(input);

                case Formatting.Indented:
                    return TsonFormat.Format(writer.Serialize(input));

                default:
                    throw new TsonException("The formatting specified is invalid.");
            }
        }

        /// <summary>
        /// Deserializes the TSON to a dictionary.
        /// </summary>
        /// <param name="input"> The TSON to deserialize. </param>
        /// <returns> A dictionary containing the deserialized items. </returns>
        public static Dictionary<string, object> DeserializeObject(string input)
        {
            if (!TsonParser.TryParse(input, out var value, out var error, out var position))
                throw new TsonException("Unable to deserialize object. " + error + " at line " + position.Line + ", column: " + position.Column);

            return (Dictionary<string, object>)value;
        }

        /// <summary>
        /// Deserializes the TSON to the specified .NET type.
        /// <param name="input"> The TSON to deserialize. </param>
        /// <param name="options"> An optional configuration object for deserialization. </param>
        /// </summary>
        public static T DeserializeObject<T>(string input, DeserializationOptions options = null) where T : class
        {
            if (!TsonParser.TryParse(input, out var value, out var error, out var position))
                throw new TsonException("Unable to deserialize object. " + error + " at line " + position.Line + ", column: " + position.Column);

            if (options == null)
                options = new DeserializationOptions();

            var config = new TypeAdapterConfig();

            config.ForType<IDictionary<string, object>, T>()
                .IgnoreAttribute(typeof(TsonIgnoreAttribute))  // ignore members which have the ignore attribute specified
                .IgnoreNullValues(true); // inherit values from initialized automatic properties

            if (options.IncludeNonPublicMembers)
            {
                // include members which are private
                config.ForType<IDictionary<string, object>, T>()
                    .IncludeMember((member, side) =>
                        side == MemberSide.Destination && member.Info is PropertyInfo && member.AccessModifier == AccessModifier.Private);
            }

            return (value as IDictionary<string, object>).Adapt<T>(config);
        }
    }

    /// <summary>
    /// Specifies formatting options for TSON serialization.
    /// </summary>
    public enum Formatting
    {
        None,
        Indented
    }

    /// <summary>
    /// Specifies the options for TSON serialization.
    /// </summary>
    public class SerializationOptions
    {
        public SerializationOptions()
        {
        }

        /// <summary>
        /// Indicates whether to include non-public members during serialization.
        /// </summary>
        public bool IncludeNonPublicMembers { get; set; }

        /// <summary>
        /// Indicates whether to include members whose values are <see langword="null"/> during serialization.
        /// </summary>
        public bool IncludeNullMembers { get; set; }
    }

    /// <summary>
    /// Specifies the options for TSON deserialization.
    /// </summary>
    public class DeserializationOptions
    {
        public DeserializationOptions()
        {
        }

        /// <summary>
        /// Indicates whether to include non-public members during deserialization.
        /// </summary>
        public bool IncludeNonPublicMembers { get; set; }
    }

    [Serializable]
    public class TsonException : Exception
    {
        internal TsonException()
        {
        }

        internal TsonException(string message) : base(message)
        {
        }

        internal TsonException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    internal static class TsonValueMap
    {
        internal static Dictionary<Type, string> Dictionary = new Dictionary<Type, string>()
        {
            { typeof(string), "string" },   { typeof(byte[]), "bytes"  },   { typeof(char), "char"         },
            { typeof(bool),   "bool"   },   { typeof(int),    "int"    },   { typeof(byte), "byte"         },
            { typeof(sbyte),  "sbyte"  },   { typeof(short),  "short"  },   { typeof(ushort), "ushort"     },
            { typeof(uint),   "uint"   },   { typeof(long),   "long"   },   { typeof(ulong), "ulong"       },
            { typeof(float),  "float"  },   { typeof(double), "double" },   { typeof(DateTime), "datetime" },
            { typeof(Uri),    "uri" }
        };

        internal static string TypeToName(Type type) => Dictionary[type];

        internal static Type NameToType(string name) => Dictionary.First(n => n.Value == name).Key;
    }

    internal class TsonWriter
    {
        private SerializationOptions Options { get; }
        private BindingFlags MemberFlags { get; }

        internal TsonWriter(SerializationOptions options)
        {
            this.Options = options;
            this.MemberFlags = BindingFlags.Instance | BindingFlags.Public | (options.IncludeNonPublicMembers ? BindingFlags.NonPublic : 0);
        }

        internal string Serialize(object input) => this.SerializeValue(input);

        private string SerializeValue(object value)
        {
            if (value is null && this.Options.IncludeNullMembers)
                return "null()";

            var value_type = value.GetType();

            if (TsonValueMap.Dictionary.ContainsKey(value_type))
            {
                return new StringBuilder().Append(TsonValueMap.TypeToName(value_type)).Append("(").Append(
                    (value is string) ? this.EscapeString((string)value) :
                    (value is byte[]) ? this.EscapeString(Convert.ToBase64String((byte[])value)) :
                    (value is bool) ? ((bool)value ? "true" : "false") :
                    (value is char) ? this.EscapeString("" + value) :
                    (value is byte) ? (byte)value :
                    (value is sbyte) ? (sbyte)value :
                    (value is double) ? Math.Round((double)value, 10) :
                    (value is Uri) ? this.EscapeString((value as Uri).ToString()) :
                    (value is DateTime) ? this.EscapeString(((DateTime)value).ToString("o")) : value).Append(")")
                    .ToString();
            }

            if (this.CheckTypeIsArray(value_type))
                return this.SerializeArray(value as IEnumerable<object>);

            if (value_type.IsEnum)
                return string.Format("string({0})", this.EscapeString(Convert.ToString(value)));

            if (value_type.IsValueType || value_type.IsClass)
                return this.SerializeObject(value);

            return null;
        }

        private string SerializeObject(object input)
        {
            var builder = new StringBuilder().Append("{");

            if (input is IDictionary<string, object> || input is IDictionary<object, object>)
            {
                builder.Append(this.SerializeDictionary((IDictionary<string, object>)input));
            }
            else
            {
                var input_type = input.GetType();

                if (input_type.IsGenericType && input_type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = input_type.GetGenericArguments()[0];
                    var valueType = input_type.GetGenericArguments()[1];

                    if (keyType == typeof(string))
                    {
                        var dictionary = new Dictionary<string, object>();

                        var keys = (input_type.GetFieldOrProperty("Keys").GetMemberValue(input) as IEnumerable<string>).ToArray();
                        var values = (input_type.GetFieldOrProperty("Values").GetMemberValue(input) as IEnumerable<object>).ToArray();

                        for (var i = 0; i < keys.Length; i++)
                            dictionary.Add(keys[i], values[i]);

                        builder.Append(this.SerializeDictionary(dictionary));
                    }
                }
                else
                {
                    var members = input_type
                        .GetProperties(this.MemberFlags).Where(p => p.GetIndexParameters().Length == 0)
                        .Select(pi => (pi.Name, Value: pi.GetValue(input, null), MemberAttributes: pi.GetCustomAttributes()))
                        .Union(input_type.GetFields(this.MemberFlags).Select(fi => (fi.Name, Value: fi.GetValue(input), MemberAttributes: fi.GetCustomAttributes())));

                    var first_value = true;
                    foreach (var kvp in members)
                    {
                        var (name, value, attributes) = kvp;

                        var propertyNameAttribute = attributes.FirstOrDefault(att => att is TsonPropertyAttribute);
                        var propertyIgnoreAttribute = attributes.FirstOrDefault(att => att is TsonIgnoreAttribute);

                        if (propertyIgnoreAttribute != null)
                            continue;

                        if (propertyNameAttribute != null)
                            name = (propertyNameAttribute as TsonPropertyAttribute).PropertyName;

                        if (value != null)
                        {
                            if (!first_value)
                                builder.Append(",");

                            builder.Append(this.EscapeString(name));
                            builder.Append(":");
                            builder.Append(this.SerializeValue(value));
                            first_value = false;
                        }
                    }
                }
            }

            return builder.Append("}").ToString();
        }

        private string SerializeDictionary(IDictionary<string, object> input)
        {
            var builder = new StringBuilder();

            var first_value = true;
            foreach (var key in input.Keys)
            {
                if (!first_value)
                    builder.Append(',');

                builder.Append(this.EscapeString(key.ToString()));
                builder.Append(':');
                builder.Append(this.SerializeValue(input[key]));

                first_value = false;
            }

            return builder.ToString();
        }

        private string EscapeString(string input)
        {
            var builder = new StringBuilder().Append('\"');

            foreach (var c in input)
            {
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        var codepoint = Convert.ToInt32(c);

                        builder.Append((codepoint >= 32) && (codepoint <= 126) ? "" + c : "\\u" + codepoint.ToString("x4"));
                        break;
                }
            }

            return builder.Append('\"').ToString();
        }

        private string SerializeArray(IEnumerable<object> collection) => "[" + collection.Cast<object>().Aggregate(new StringBuilder(),
             (sb, v) => sb.Append(this.SerializeValue(v)).Append(","), sb => { if (0 < sb.Length) sb.Length--; return sb.ToString(); }) + "]";

        private bool CheckTypeIsArray(Type type) =>
            type.IsArray ||
            (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>))) ||
            typeof(IEnumerable<object>).IsAssignableFrom(type);
    }

    internal static class TsonFormat
    {
        internal const string INDENT_STRING = "    ";

        internal static string Format(string input)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();

            for (var i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;

                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;

                    case '"':
                        sb.Append(ch);
                        var escaped = false;
                        var index = i;
                        while (index > 0 && input[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;

                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;

                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        internal static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
                action(i);
        }
    }

    public static class TypeExtension
    {
        public static MemberInfo GetFieldOrProperty(this Type type, string name)
            => type.GetInheritedMember(name) ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type) => type.GetTypeInfo().DeclaredMembers;

        private static IEnumerable<MemberInfo> GetAllMembers(this Type type) =>
            type.GetTypeInheritance().Concat(type.GetInterfaces()).SelectMany(i => i.GetDeclaredMembers());

        public static IEnumerable<Type> GetTypeInheritance(this Type type)
        {
            yield return type;

            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        public static MemberInfo GetInheritedMember(this Type type, string name) => type.GetAllMembers().FirstOrDefault(mi => mi.Name == name);

        public static object GetMemberValue(this MemberInfo propertyOrField, object target)
        {
            if (propertyOrField is PropertyInfo property)
            {
                return property.GetValue(target, null);
            }
            if (propertyOrField is FieldInfo field)
            {
                return field.GetValue(target);
            }
            throw new ArgumentOutOfRangeException(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);
        }
    }
}