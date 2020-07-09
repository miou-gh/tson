////////////////////////////////////////////////////////////////////////////////////
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
using Xunit;
using Tson;
using Xunit.Sdk;
using System.Collections.Generic;
using Bogus;
using System.Buffers.Text;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace TsonUnitTests
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string MetaDescription { get; set; }
        public string Keywords { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public string AuthorId { get; set; }
        public uint AUint { get; set; }
        public long ALong { get; set; }
        public ulong AULong { get; set; }
        public byte[] AByteArray { get; set; }
        public byte AByte { get; set; }
        public float AFloat { get; set; }
        public double ADouble { get; set; }
        public char AChar { get; set; }
        public sbyte ASByte { get; set; }
        public short AShort { get; set; }
        public ushort AUShort { get; set; }

        public List<BlogPost> Children { get; set; }

        public BlogPost()
        {
            this.Children = new List<BlogPost>();
        }
    }

    public class SeriaizeTests
    {
        [Fact]
        public void SerializeNull()
        {
            var post = new BlogPost()
            {
                Id = 1234,
                Content = "This is some content.",
                Title = null
            };

            var serialized = TsonConvert.SerializeObject(post, Formatting.Indented, new SerializationOptions() { IncludeNullMembers = true });
            var deserialized = TsonConvert.DeserializeObject(serialized);
            var reserialized = TsonConvert.SerializeObject(deserialized, Formatting.Indented, new SerializationOptions() { IncludeNullMembers = true });

            Debug.WriteLine(serialized);
            Debug.WriteLine(reserialized);

            Assert.True(serialized == reserialized, "The re-serialized TSON deserialized content is not the same.");
        }

        [Fact]
        public void BogusBlog()
        {
            var parent = new Faker<BlogPost>()
               .RuleFor(bp => bp.Id, f => f.IndexFaker)
               .RuleFor(bp => bp.Content, f => f.Lorem.Paragraphs())
               .RuleFor(bp => bp.Created, f => f.Date.Past())
               .RuleFor(bp => bp.IsActive, f => f.PickRandomParam(new bool[] { true, true, false }))
               .RuleFor(bp => bp.MetaDescription, f => f.Lorem.Sentences(3))
               .RuleFor(bp => bp.Keywords, f => string.Join(", ", f.Lorem.Words()))
               .RuleFor(bp => bp.Title, f => f.Lorem.Sentence(10))
               .RuleFor(bp => bp.AUint, f => f.Random.UInt())
               .RuleFor(bp => bp.ALong, f => f.Random.Long())
               .RuleFor(bp => bp.AULong, f => f.Random.ULong())
               .RuleFor(bp => bp.AByteArray, f => f.Random.Bytes(100))
               .RuleFor(bp => bp.AByte, f => f.Random.Byte())
               .RuleFor(bp => bp.AFloat, f => f.Random.Float())
               .RuleFor(bp => bp.ADouble, f => 1.571d)
               .RuleFor(bp => bp.AChar, f => f.Random.Char())
               .RuleFor(bp => bp.ASByte, f => f.Random.SByte())
               .RuleFor(bp => bp.AShort, f => f.Random.Short())
               .RuleFor(bp => bp.AUShort, f => f.Random.UShort()).Generate();

            parent.Children.Add(new Faker<BlogPost>()
               .RuleFor(bp => bp.Id, f => f.IndexFaker)
               .RuleFor(bp => bp.Content, f => f.Lorem.Paragraphs())
               .RuleFor(bp => bp.Created, f => f.Date.Past())
               .RuleFor(bp => bp.IsActive, f => f.PickRandomParam(new bool[] { true, true, false }))
               .RuleFor(bp => bp.MetaDescription, f => f.Lorem.Sentences(3))
               .RuleFor(bp => bp.Keywords, f => string.Join(", ", f.Lorem.Words()))
               .RuleFor(bp => bp.Title, f => f.Lorem.Sentence(10))
               .RuleFor(bp => bp.AUint, f => f.Random.UInt())
               .RuleFor(bp => bp.ALong, f => f.Random.Long())
               .RuleFor(bp => bp.AULong, f => f.Random.ULong())
               .RuleFor(bp => bp.AByteArray, f => f.Random.Bytes(100))
               .RuleFor(bp => bp.AByte, f => f.Random.Byte())
               .RuleFor(bp => bp.AFloat, f => f.Random.Float())
               .RuleFor(bp => bp.AChar, f => f.Random.Char())
               .RuleFor(bp => bp.ASByte, f => f.Random.SByte())
               .RuleFor(bp => bp.AShort, f => f.Random.Short())
               .RuleFor(bp => bp.AUShort, f => f.Random.UShort()).Generate());

            var serialized = TsonConvert.SerializeObject(parent);
            var deserialized = TsonConvert.DeserializeObject(serialized);
            var reserialized = TsonConvert.SerializeObject(deserialized);

            Assert.True(serialized == reserialized, "The re-serialized TSON deserialized content is not the same.");
        }
    }

    public class DeserializeTests
    {
        [Fact]
        public void DeserializeToClass()
        {
            var parent = new Faker<BlogPost>()
               .RuleFor(bp => bp.Id, f => f.IndexFaker)
               .RuleFor(bp => bp.Content, f => f.Lorem.Paragraphs())
               .RuleFor(bp => bp.Created, f => f.Date.Past())
               .RuleFor(bp => bp.IsActive, f => f.PickRandomParam(new bool[] { true, true, false }))
               .RuleFor(bp => bp.MetaDescription, f => f.Lorem.Sentences(3))
               .RuleFor(bp => bp.Keywords, f => string.Join(", ", f.Lorem.Words()))
               .RuleFor(bp => bp.Title, f => f.Lorem.Sentence(10))
               .RuleFor(bp => bp.AUint, f => f.Random.UInt())
               .RuleFor(bp => bp.ALong, f => f.Random.Long())
               .RuleFor(bp => bp.AULong, f => f.Random.ULong())
               .RuleFor(bp => bp.AByteArray, f => f.Random.Bytes(100))
               .RuleFor(bp => bp.AByte, f => f.Random.Byte())
               .RuleFor(bp => bp.AFloat, f => f.Random.Float())
               .RuleFor(bp => bp.AChar, f => f.Random.Char())
               .RuleFor(bp => bp.ASByte, f => f.Random.SByte())
               .RuleFor(bp => bp.AShort, f => f.Random.Short())
               .RuleFor(bp => bp.AUShort, f => f.Random.UShort()).Generate();

            parent.Children.Add(new Faker<BlogPost>()
               .RuleFor(bp => bp.Id, f => f.IndexFaker)
               .RuleFor(bp => bp.Content, f => f.Lorem.Paragraphs())
               .RuleFor(bp => bp.Created, f => f.Date.Past())
               .RuleFor(bp => bp.IsActive, f => f.PickRandomParam(new bool[] { true, true, false }))
               .RuleFor(bp => bp.MetaDescription, f => f.Lorem.Sentences(3))
               .RuleFor(bp => bp.Keywords, f => string.Join(", ", f.Lorem.Words()))
               .RuleFor(bp => bp.Title, f => f.Lorem.Sentence(10))
               .RuleFor(bp => bp.AUint, f => f.Random.UInt())
               .RuleFor(bp => bp.ALong, f => f.Random.Long())
               .RuleFor(bp => bp.AULong, f => f.Random.ULong())
               .RuleFor(bp => bp.AByteArray, f => f.Random.Bytes(100))
               .RuleFor(bp => bp.AByte, f => f.Random.Byte())
               .RuleFor(bp => bp.AFloat, f => f.Random.Float())
               .RuleFor(bp => bp.AChar, f => f.Random.Char())
               .RuleFor(bp => bp.ASByte, f => f.Random.SByte())
               .RuleFor(bp => bp.AShort, f => f.Random.Short())
               .RuleFor(bp => bp.AUShort, f => f.Random.UShort()).Generate());

            var as_tson = TsonConvert.SerializeObject(parent, Formatting.Indented);
            var to_class = TsonConvert.DeserializeObject<BlogPost>(as_tson);
            var back_to_tson = TsonConvert.SerializeObject(to_class, Formatting.Indented);

            // TODO: The equality comparison doesn't work for some reason, so instead we'll compare TSON.
            //Assert.Equal(parent, to_class);
            Assert.True(as_tson == back_to_tson);
        }

        [Fact]
        public void String_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": string(\"hello world\") }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal("hello world", dictionary["a"]);
        }

        [Fact]
        public void Integer_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": int(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal(1000, dictionary["a"]);
        }

        [Fact]
        public void UInteger_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": uint(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal(1000u, dictionary["a"]);
        }

        [Fact]
        public void Short_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": short(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((short)1000, dictionary["a"]);
        }

        [Fact]
        public void UShort_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": ushort(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((ushort)1000, dictionary["a"]);
        }

        [Fact]
        public void Long_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": long(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((long)1000, dictionary["a"]);
        }

        [Fact]
        public void ULong_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": ulong(1000) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((ulong)1000, dictionary["a"]);
        }

        [Fact]
        public void Float_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": float(1.50) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((float)1.50, dictionary["a"]);
        }

        [Fact]
        public void Double_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": double(1.50) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((double)1.50, dictionary["a"]);
        }

        [Fact]
        public void Boolean_Property()
        {
            {
                if (!TsonParser.TryParse("{ \"a\": bool(false) }", out var value, out var error, out var _))
                    throw new XunitException(error);

                var dictionary = (value as Dictionary<string, object>);
                Assert.Equal((bool)false, dictionary["a"]);
            }

            {
                if (!TsonParser.TryParse("{ \"a\": bool(true) }", out var value, out var error, out var _))
                    throw new XunitException(error);

                var dictionary = (value as Dictionary<string, object>);
                Assert.Equal((bool)true, dictionary["a"]);
            }
        }

        [Fact]
        public void ByteArray_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": bytes(\"AAQABA==\") }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal(new byte[] { 0, 4, 0, 4 }, dictionary["a"]);
        }

        [Fact]
        public void DateTime_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": datetime(\"9999-12-31T23:59:59.9999999\") }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal(DateTime.MaxValue, dictionary["a"]);
        }

        [Fact]
        public void Char_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": char(\"F\") }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal('F', dictionary["a"]);
        }

        [Fact]
        public void Byte_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": byte(255) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((byte)255, dictionary["a"]);
        }

        [Fact]
        public void SByte_Property()
        {
            if (!TsonParser.TryParse("{ \"a\": sbyte(-5) }", out var value, out var error, out var _))
                throw new XunitException(error);

            var dictionary = (value as Dictionary<string, object>);
            Assert.Equal((sbyte)-5, dictionary["a"]);
        }
    }
}
