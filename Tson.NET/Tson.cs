////////////////////////////////////////////////////////////////////////////////////
//    MIT License
//
//    Copyright (c) 2019 Atilla Lonny (https://github.com/atillabyte/tson)
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Tson.NET
{
    public enum Formatting
    {
        None,
        Indented
    }

    public static class TsonConvert
    {
        /// <summary>
        /// Serializes the specified object to a TSON string using formatting.
        /// </summary>
        /// <param name="value"> The object to serialize. </param>
        /// <param name="formatting"> Indicates how the output should be formatted. </param>
        /// <param name="includePrivate"> Indicates whether to include private properties and members in serialization. </param>
        /// <returns>
        /// A TSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object input, Formatting format = Formatting.None, bool includePrivate = false)
        {
            var writer = new TsonWriter(includePrivate);
                writer.SerializeValue(input);

            switch (format)
            {
                case Formatting.None:
                    return writer.GetTson();
                case Formatting.Indented:
                    return TsonFormat.Format(writer.GetTson());
                default:
                    throw new TsonException("The formatting specified is invalid.");
            }
        }

        /// <summary>
        /// Deserializes the TSON string into a dictionary.
        /// </summary>
        /// <param name="input"> The TSON to deserialize. </param>
        /// <returns> A dictionary containing the deserialized items. </returns>
        public static Dictionary<string, object> DeserializeObject(string input) => new TsonReader().Parse(input) as Dictionary<string, object>;
    }

    [Serializable]
    public class TsonException : Exception
    {
        internal TsonException() { }
        internal TsonException(string message) : base(message) { }
        internal TsonException(string message, Exception inner) : base(message, inner) { }
    }
}

/// <summary>
/// TsonReader / TsonWriter / TsonFormat
/// </summary>
namespace Tson.NET
{
    internal class TsonWriter
    {
        private StringBuilder m_builder;
        private BindingFlags m_memberFlags;
        private bool m_includePrivateMembers;

        internal string GetTson() => m_builder.ToString();

        internal TsonWriter(bool includePrivateMembers = false)
        {
            m_builder = new StringBuilder();
            m_includePrivateMembers = includePrivateMembers;
            m_memberFlags =
                BindingFlags.Instance |
                BindingFlags.Public |
                (m_includePrivateMembers ? BindingFlags.NonPublic : 0);
        }

        private static bool IsGenericList(Type type) => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>));
        private static bool IsGenericDictionary(Type type) => type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>));

        internal void SerializeValue(object input)
        {
            if (input == null)
            {
                m_builder.Append("null()");
                return;
            }

            var type = input.GetType();

            if (type == typeof(byte[]))
            {
                m_builder.Append("bytes").Append("(\"").Append(Convert.ToBase64String((byte[])input)).Append("\")");
            }
            else if (type.IsArray)
            {
                this.SerializeArray(input);
            }
            else if (IsGenericList(type))
            {
                var elementType = type.GetGenericArguments()[0];
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(new System.Type[] { elementType });
                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(new System.Type[] { elementType });
                var castedObjectEnum = castMethod.Invoke(null, new object[] { input });
                var castedObject = toArrayMethod.Invoke(null, new object[] { castedObjectEnum });

                this.SerializeArray(castedObject);
            }
            else if (typeof(IEnumerable<object>).IsAssignableFrom(type))
            {
                var elementType = typeof(object);
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(new System.Type[] { elementType });
                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(new System.Type[] { elementType });
                var castedObjectEnum = castMethod.Invoke(null, new object[] { input });
                var castedObject = toArrayMethod.Invoke(null, new object[] { castedObjectEnum });

                this.SerializeArray(castedObject);
            }
            else if (type.IsEnum)
                m_builder.Append("string").Append("(").Append(this.SerializeString(input.ToString())).Append(")");
            else if (type == typeof(string))
                m_builder.Append("string").Append("(").Append(this.SerializeString(input.ToString())).Append(")");
            else if (type == typeof(char))
                m_builder.Append("char").Append("(\"").Append(input).Append("\")");
            else if (type == typeof(bool))
                m_builder.Append("bool").Append("(").Append(((bool)input) ? "true" : "false").Append(")");
            else if (type == typeof(int))
                m_builder.Append("int").Append("(").Append(input).Append(")");
            else if (type == typeof(byte))
                m_builder.Append("byte").Append("(").Append(input).Append(")");
            else if (type == typeof(sbyte))
                m_builder.Append("sbyte").Append("(").Append(input).Append(")");
            else if (type == typeof(short))
                m_builder.Append("short").Append("(").Append(input).Append(")");
            else if (type == typeof(ushort))
                m_builder.Append("ushort").Append("(").Append(input).Append(")");
            else if (type == typeof(uint))
                m_builder.Append("uint").Append("(").Append(input).Append(")");
            else if (type == typeof(long))
                m_builder.Append("long").Append("(").Append(input).Append(")");
            else if (type == typeof(ulong))
                m_builder.Append("ulong").Append("(").Append(input).Append(")");
            else if (type == typeof(float))
                m_builder.Append("float").Append("(").Append(input).Append(")");
            else if (type == typeof(double))
                m_builder.Append("double").Append("(").Append(input).Append(")");
            else if (type == typeof(DateTime))
                m_builder.Append("datetime").Append("(\"").Append(((DateTime)input).ToString("o")).Append("\")");
            else if (type.IsValueType)
                this.SerializeObject(input);
            else if (type.IsClass)
                this.SerializeObject(input);
            else
                throw new InvalidOperationException($"Unable to serialize value to TSON - unsupported type '{ type.Name }'.");
        }

        private void SerializeArray(object input)
        {
            m_builder.Append("[");
            var array = input as Array;
            var first = true;

            foreach (var element in array)
            {
                if (!first)
                    m_builder.Append(",");

                this.SerializeValue(element);
                first = false;
            }
            m_builder.Append("]");
        }

        private string SerializeString(string input)
        {
            m_builder.Append('\"');

            foreach (var c in input.ToCharArray())
            {
                switch (c)
                {
                    case '"':
                        m_builder.Append("\\\"");
                        break;
                    case '\\':
                        m_builder.Append("\\\\");
                        break;
                    case '\b':
                        m_builder.Append("\\b");
                        break;
                    case '\f':
                        m_builder.Append("\\f");
                        break;
                    case '\n':
                        m_builder.Append("\\n");
                        break;
                    case '\r':
                        m_builder.Append("\\r");
                        break;
                    case '\t':
                        m_builder.Append("\\t");
                        break;
                    default:
                        var codepoint = Convert.ToInt32(c);
                        if ((codepoint >= 32) && (codepoint <= 126))
                        {
                            m_builder.Append(c);
                        }
                        else
                        {
                            m_builder.Append("\\u");
                            m_builder.Append(codepoint.ToString("x4"));
                        }
                        break;
                }
            }

            m_builder.Append('\"');
            return "";
        }

        private void SerializeDictionary(IDictionary<string, object> obj)
        {
            var first = true;
            foreach (var key in obj.Keys)
            {
                if (!first)
                    m_builder.Append(',');

                this.SerializeString(key.ToString());
                m_builder.Append(':');

                this.SerializeValue(obj[key]);

                first = false;
            }
        }

        private void SerializeDictionary(IDictionary obj)
        {
            var first = true;
            foreach (var key in obj.Keys)
            {
                if (!first)
                    m_builder.Append(',');

                this.SerializeString(key.ToString());
                m_builder.Append(':');

                this.SerializeValue(obj[key]);

                first = false;
            }
        }

        private void SerializeObject(object input)
        {
            m_builder.Append("{");

            var first = true;
            if (input is IDictionary<string, object> string_object_dict)
            {
                this.SerializeDictionary(string_object_dict);
            }
            else if (input is IDictionary non_generic_dict)
            {
                this.SerializeDictionary(non_generic_dict);
            }
            else
            {
                var fields = input.GetType().GetFields(m_memberFlags);
                var properties = input.GetType().GetProperties(m_memberFlags);

                // TODO: A safety check to avoid infinite loops in self-referenced properties or fields.

                foreach (var info in fields)
                {
                    if (info.IsStatic)
                    {
                        continue;
                    }

                    var fieldValue = info.GetValue(input);

                    if (fieldValue != null)
                    {
                        if (!first)
                            m_builder.Append(",");

                        this.SerializeString(info.Name);
                        m_builder.Append(":");
                        this.SerializeValue(fieldValue);
                        first = false;
                    }
                }

                foreach (var property in properties)
                {
                    var propertyValue = property.GetValue(input);

                    if (propertyValue != null)
                    {
                        if (!first)
                            m_builder.Append(",");

                        this.SerializeString(property.Name);
                        m_builder.Append(":");
                        this.SerializeValue(propertyValue);
                        first = false;
                    }
                }
            }

            m_builder.Append("}");
        }
    }

    internal class TsonReader
    {
        private enum Token
        {
            None,
            OpenBrace,
            CloseBrace,
            OpenBracket,
            CloseBracket,
            Colon,
            Comma,
            String,
            ValueExpression
        }

        private StringReader StringReader;

        public object Parse(string input)
        {
            this.StringReader = new StringReader(Regex.Replace(input, @"(""(?:[^""\\]|\\.)*"")|\s+", "$1")); // remove whitespace

            if (this.StringReader.Peek() == '{')
                return this.DecodeObject();

            if (this.StringReader.Peek() == '[')
                return this.DecodeArray();

            return null;
        }

        private Dictionary<string, object> DecodeObject()
        {
            this.StringReader.Read(); // skip opening brace

            var dictionary = new Dictionary<string, object>();

            while (true)
            {
                switch (this.NextToken())
                {
                    case Token.None:
                        return null;

                    case Token.Comma:
                        continue;

                    case Token.CloseBrace:
                        return dictionary;

                    default:
                        var key = this.DecodeString();

                        if (string.IsNullOrEmpty(key))
                            return null;

                        if (this.NextToken() != Token.Colon)
                            return null;

                        this.StringReader.Read();

                        var value = this.DecodeFromToken(this.NextToken());

                        dictionary.Add(key, value);
                        break;
                }
            }
        }

        private List<object> DecodeArray()
        {
            var list = new List<object>();

            // skip opening bracket.
            this.StringReader.Read();

            // [
            var parsing = true;

            while (parsing)
            {
                switch (this.NextToken())
                {
                    case Token.None:
                        return null;

                    case Token.Comma:
                        continue;

                    case Token.CloseBracket:
                        parsing = false;
                        break;

                    default:
                        list.Add(this.DecodeFromToken(this.NextToken()));
                        break;
                }
            }

            return list;
        }

        private object DecodeFromToken(Token token)
        {
            switch (token)
            {
                case Token.String:
                    return this.DecodeString();

                case Token.OpenBrace:
                    return this.DecodeObject();

                case Token.OpenBracket:
                    return this.DecodeArray();

                case Token.ValueExpression:
                    return this.DecodeValueExpression();
            }

            return null;
        }

        private object DecodeValueExpression()
        {
            var keyword = new StringBuilder();

            var inside_string = false;
            while ("{}[],:".IndexOf((char)this.StringReader.Peek()) == -1 || inside_string)
            {
                var n = (char)this.StringReader.Read();
                keyword.Append(n);

                if (n == '"')
                    inside_string = !inside_string;

                if (this.StringReader.Peek() == -1)
                    break;
            }

            var match = Regex.Match(keyword.ToString(), @"(\w+)\(([^()]+|[^(]+\([^)]*\)[^()]*)\)");

            var type = match.Groups[1].Value;
            var value = match.Groups[2].Value;

            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                return null;

            switch (type)
            {
                case "string":
                    return value.Remove(value.Length - 1, 1).Remove(0, 1);

                case "int":
                    if (int.TryParse(value, out var @int))
                        return @int;
                    break;

                case "uint":
                    if (uint.TryParse(value, out var @uint))
                        return @uint;
                    break;

                case "byte":
                    if (byte.TryParse(value, out var @byte))
                        return @byte;
                    break;

                case "sbyte":
                    if (sbyte.TryParse(value, out var @sbyte))
                        return @sbyte;
                    break;

                case "short":
                    if (sbyte.TryParse(value, out var @short))
                        return @short;
                    break;

                case "ushort":
                    if (ushort.TryParse(value, out var @ushort))
                        return @ushort;
                    break;

                case "long":
                    if (long.TryParse(value, out var @long))
                        return @long;
                    break;

                case "ulong":
                    if (ulong.TryParse(value, out var @ulong))
                        return @ulong;
                    break;

                case "float":
                    if (float.TryParse(value, out var @float))
                        return @float;
                    break;

                case "double":
                    if (double.TryParse(value, out var @double))
                        return @double;
                    break;

                case "bool":
                    if (bool.TryParse(value, out var @bool))
                        return @bool;
                    break;

                case "char":
                    if (string.IsNullOrEmpty(value) || value.Length <= 2 || !(value[0] == '"' && (value[value.Length - 1] == '"')))
                        return null;

                    return value.Remove(value.Length - 1, 1).Remove(0, 1).First();

                case "bytes":
                    if (string.IsNullOrEmpty(value) || value.Length <= 2 || !(value[0] == '"' && (value[value.Length - 1] == '"')))
                        return null;

                    return Convert.FromBase64String(value.Remove(value.Length - 1, 1).Remove(0, 1));

                case "datetime":
                    if (string.IsNullOrEmpty(value) || value.Length <= 2 || !(value[0] == '"' && (value[value.Length - 1] == '"')))
                        return null;

                    return DateTime.Parse(value.Remove(value.Length - 1, 1).Remove(0, 1), null, DateTimeStyles.RoundtripKind);

                case "null":
                    return null;
            }

            return null;
        }

        private string DecodeString()
        {
            var stringBuilder = new StringBuilder();

            // skip opening quote
            this.StringReader.Read();

            var parsing = true;
            while (parsing)
            {
                if (this.StringReader.Peek() == -1)
                {
                    parsing = false;
                    break;
                }

                var c = (char)this.StringReader.Read();
                switch (c)
                {
                    case '"':
                        parsing = false;
                        break;

                    case '\\':
                        if (this.StringReader.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = (char)this.StringReader.Read();

                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                stringBuilder.Append(c);
                                break;

                            case 'b':
                                stringBuilder.Append('\b');
                                break;

                            case 'f':
                                stringBuilder.Append('\f');
                                break;

                            case 'n':
                                stringBuilder.Append('\n');
                                break;

                            case 'r':
                                stringBuilder.Append('\r');
                                break;

                            case 't':
                                stringBuilder.Append('\t');
                                break;

                            case 'u':
                                var hex = new StringBuilder();

                                for (var i = 0; i < 4; i++)
                                {
                                    hex.Append(this.StringReader.Read());
                                }

                                stringBuilder.Append((char)Convert.ToInt32(hex.ToString(), 16));
                                break;
                        }

                        break;

                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private Token NextToken()
        {
            if (this.StringReader.Peek() == -1)
                return Token.None;

            switch (this.StringReader.Peek())
            {
                case '{':
                    return Token.OpenBrace;

                case '}':
                    this.StringReader.Read();
                    return Token.CloseBrace;

                case '[':
                    return Token.OpenBracket;

                case ']':
                    this.StringReader.Read();
                    return Token.CloseBracket;

                case ',':
                    this.StringReader.Read();
                    return Token.Comma;

                case '"':
                    return Token.String;

                case ':':
                    return Token.Colon;
            }

            return Token.ValueExpression;
        }
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
            {
                action(i);
            }
        }
    }
}