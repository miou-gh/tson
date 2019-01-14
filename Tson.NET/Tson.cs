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
using System.Linq.Expressions;
using System.Dynamic;

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
        /// <returns>
        /// A TSON string representation of the object.
        /// </returns>
        public static string SerializeObject(object input, Formatting format)
        {
            switch (format)
            {
                case Formatting.None:
                    return new TsonWriter().SerializeComplexTypeToTSON(input);

                case Formatting.Indented:
                    return TsonFormat.Format(new TsonWriter().SerializeComplexTypeToTSON(input));
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes the TSON to the specified .NET type.
        /// </summary>
        /// <typeparam name="T"> The type of the object to deserialize to. </typeparam>
        /// <param name="value"> The TSON to deserialize. </param>
        /// <returns> The deserialized object from the TSON string. </returns>
        public static T DeserializeObject<T>(string input) where T : class
        {
            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                return new TsonReader().Parse(input) as T;
            }

            var instance = new TsonReader().Parse(input);

            if (instance is IList)
            {
                var reference = ((List<object>)instance);

                if (typeof(T).IsArray)
                {
                    var array = Array.CreateInstance(typeof(T).GetElementType(), reference.Count);

                    for (var i = 0; i < reference.Count; i++)
                        array.SetValue(TsonMapper<object>.RetrieveObject((Dictionary<string, object>)reference[i], typeof(T).GetElementType()), i);

                    return (T)((object)array);
                }

                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                {
                    var type = typeof(T).GetGenericArguments().First();
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));

                    for (var i = 0; i < reference.Count; i++)
                        list.Add(TsonMapper<object>.RetrieveObject((Dictionary<string, object>)reference[i], type));

                    return (T)list;
                }
            }

            if (instance is IDictionary)
            {
                return TsonMapper<T>.FromDictionary((Dictionary<string, object>)instance);
            }

            return null;
        }

        /// <summary>
        /// Deserializes the TSON to a .NET object.
        /// </summary>
        /// <param name="value"> The TSON to deserialize. </param>
        /// <returns> The deserialized object from the TSON string. </returns>
        public static object DeserializeObject(string input)
        {
            //return (object)new TsonReader().Parse(input).Aggregate(new ExpandoObject() as IDictionary<string, object>,
            //                (a, p) => { a.Add(p.Key, p.Value); return a; });
            return null;
        }
    }

    [Serializable]
    public class TsonException : Exception
    {
        public TsonException() { }
        public TsonException(string message) : base(message) { }
        public TsonException(string message, Exception inner) : base(message, inner) { }
    }
}

/// <summary>
/// TsonReader / TsonWriter / TsonFormat
/// </summary>
namespace Tson.NET
{
    internal class TsonWriter
    {
        internal string SerializeComplexTypeToTSON(object complex)
        {
            void SerializeDictionary(object input, StringBuilder builder)
            {
                IEnumerable<KeyValuePair<string, object>> kvps;

                kvps = (input is IDictionary dict)
                    ? dict.Keys.Cast<object>().Select(k => new KeyValuePair<string, object>(k.ToString(), dict[k]))
                    : (IEnumerable<KeyValuePair<string, object>>)input;

                var kvpList = kvps.ToList();
                kvpList.Sort((e1, e2) => string.Compare(e1.Key, e2.Key, StringComparison.OrdinalIgnoreCase));

                foreach (var kvp in kvpList)
                {
                    builder.Append(@"""");
                    builder.Append(kvp.Key);
                    builder.Append(@""":");
                    builder.Append(this.SerializeObject(kvp.Value));
                    builder.Append(",");
                }

                if (builder.Length > 0 && builder[builder.Length - 1] == ',')
                    builder.Remove(builder.Length - 1, 1);
            }

            if (complex is IDictionary || complex is IEnumerable<KeyValuePair<string, object>>)
            {
                var sb = new StringBuilder("{");

                SerializeDictionary(complex, sb);

                sb.Append("}");

                return sb.ToString();
            }
            else if (complex is IList || complex is IEnumerable<object>)
            {
                var sb = new StringBuilder("[");
                var items = ((List<object>)ObjectToDictionary.Convert(complex, new List<object>()));

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    if (item is IDictionary)
                    {
                        sb.Append("{");
                        SerializeDictionary(item, sb);
                        sb.Append("}");
                    }
                    
                    if (item is IList)
                    {
                        sb.Append("[");
                        SerializeDictionary(item, sb);
                        sb.Append("]");
                    }

                    if (i != items.Count - 1)
                        sb.Append(",");
                }

                sb.Append("]");
                return sb.ToString();
            }

            return null;
        }
        
        private string SerializeObject(object input)
        {
            if (input == null)
                return "null()";

            string EscapeString(string src)
            {
                var sb = new StringBuilder();

                foreach (var c in src)
                {
                    if (c == '"' || c == '\\')
                    {
                        sb.Append('\\');
                        sb.Append(c);
                    }
                    else if (c < 0x20) // control character
                    {
                        var u = (int)c;

                        switch (u)
                        {
                            case '\b':
                                sb.Append("\\b");
                                break;
                            case '\f':
                                sb.Append("\\f");
                                break;
                            case '\n':
                                sb.Append("\\n");
                                break;
                            case '\r':
                                sb.Append("\\r");
                                break;
                            case '\t':
                                sb.Append("\\t");
                                break;
                            default:
                                sb.Append("\\u");
                                sb.Append(u.ToString("X4", NumberFormatInfo.InvariantInfo));
                                break;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                return sb.ToString();
            }

            if (input is IList && !(input is IEnumerable<KeyValuePair<string, object>>))
            {
                var list = (input as IList);
                var builder = new StringBuilder("[");

                if (list.Count > 0)
                {
                    builder.Append(string.Join(",", list.Cast<object>().Select(i => this.SerializeObject(i)).ToArray()));
                }

                builder.Append("]");
                return builder.ToString();
            }

            if (input is string)
            {
                return string.Format("string(\"{0}\")", EscapeString((string)input));
            }

            if (input is int)
            {
                return string.Format("int({0})", (int)input);
            }

            if (input is uint)
            {
                return string.Format("uint({0})", (uint)input);
            }

            if (input is long)
            {
                return string.Format("long({0})", (long)input);
            }

            if (input is float)
            {
                return string.Format("float({0})", (float)input);
            }

            if (input is double)
            {
                return string.Format("double({0})", (double)input);
            }

            if (input is bool)
            {
                return string.Format("bool({0})", (bool)input);
            }

            if (input is byte[])
            {
                return string.Format("bytes(\"{0}\")", Convert.ToBase64String((byte[])input));
            }

            if (input is DateTime)
            {
                return string.Format("datetime(\"{0}\")", ((DateTime)input).ToString("o"), CultureInfo.InvariantCulture);
            }

            if (input is Enum)
            {
                return string.Format("string(\"{0}\")", EscapeString(((Enum)input).ToString()));
            }

            return null;
        }
        
        private static class ObjectToDictionary
        {
            internal static object Convert(object input, List<object> stack)
            {
                var dictionary = new Dictionary<string, object>();
                
                if (input is IDictionary)
                {
                    foreach (KeyValuePair<object, object> kvp in (IDictionary)input)
                    {
                        dictionary.Add((string)kvp.Key, ObjectToDictionary.Convert(kvp.Value, stack));
                    }

                    return dictionary;
                }
                else if (input is IList || input.GetType().IsArray)
                {
                    var items = new List<object>();

                    foreach (var item in (IEnumerable)input)
                    {
                        items.Add(ObjectToDictionary.Convert(item, stack));
                    }

                    return items;
                }
                else
                {
                    if (!input.GetType().IsPrimitive && !(input is string) && !(input is DateTime))
                    {
                        var fields = input.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                        var properties = input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        void CheckForSelfReferencingLoop(MemberInfo member)
                        {
                            // check for self-referencing object
                            if (stack.Contains(input))
                            {
                                var message = "Self referencing loop detected";

                                if (member != null)
                                {
                                    message += string.Format(" for property '{0}'", member.Name);
                                }

                                message += string.Format(" with type '{0}'.", input.GetType());

                                throw new TsonException(message);
                            }
                        }

                        foreach (var member in new List<MemberInfo>(fields).Union(properties))
                        {
                            switch (member.MemberType)
                            {
                                case MemberTypes.Property:
                                    var property = ((PropertyInfo)member);

                                    CheckForSelfReferencingLoop(property);

                                    dictionary.Add(property.Name, ObjectToDictionary.Convert(property.GetValue(input), stack));
                                    break;
                                case MemberTypes.Field:
                                    var field = ((FieldInfo)member);

                                    CheckForSelfReferencingLoop(field);

                                    dictionary.Add(field.Name, ObjectToDictionary.Convert(field.GetValue(input), stack));
                                    break;
                            }

                        }
                    }
                    else
                    {
                        stack.Add(input);

                        return input;
                    }

                    return dictionary;
                }
            }
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

            var match = Regex.Match(keyword.ToString(), @"(\w+)\(([^)]+)\)");

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

                case "long":
                    if (long.TryParse(value, out var @long))
                        return @long;
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

/// <summary>
/// TsonMapper
/// </summary>
namespace Tson.NET
{
    internal static class TsonMapper<T>
    {
        public static T FromDictionary(Dictionary<string, object> input)
        {
            return (T)RetrieveObject(input, typeof(T));
        }

        public static object RetrieveObject(Dictionary<string, object> input, Type type)
        {
            var instance = Activator.CreateInstance(type);

            foreach (var kvp in input)
            {
                var prop = type.GetProperty(kvp.Key);

                if (prop == null)
                    continue;

                var value = kvp.Value;

                if (value is Dictionary<string, object>)
                    value = RetrieveObject((Dictionary<string, object>)value, prop.PropertyType);

                prop.ExpressionSetGet().Invoke(instance, value);
            }

            return instance;
        }
    }

    internal static class MemberExtensions
    {
        internal static Action<object, object> ExpressionSetGet(this PropertyInfo propertyInfo)
        {
            var _obj = typeof(object);

            var setMethodInfo = propertyInfo.GetSetMethod(true);
            var instance = Expression.Parameter(_obj, "instance");
            var value = Expression.Parameter(_obj, "value");
            var instanceCast = (!(propertyInfo.DeclaringType).GetTypeInfo().IsValueType) ? Expression.TypeAs(instance, propertyInfo.DeclaringType) : Expression.Convert(instance, propertyInfo.DeclaringType);
            var valueCast = (!(propertyInfo.PropertyType).GetTypeInfo().IsValueType) ? Expression.TypeAs(value, propertyInfo.PropertyType) : Expression.Convert(value, propertyInfo.PropertyType);

            return Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, setMethodInfo, valueCast), new ParameterExpression[] { instance, value }).Compile();
        }

        internal static Action<object, object> ExpressionSetGet(this FieldInfo fieldInfo)
        {
            var _obj = typeof(object);

            var instance = Expression.Parameter(_obj, "instance");
            var value = Expression.Parameter(_obj, "value");

            return Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Field(
                Expression.Convert(instance, fieldInfo.DeclaringType), fieldInfo),
                Expression.Convert(value, fieldInfo.FieldType)), instance, value).Compile();
        }
    }
}

