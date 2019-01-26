using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tson.NET;

namespace Tson.UnitTest
{
    [TestClass]
    public class TrySerializeValues
    {
        public class StringModel { public string FieldName = "hello world\n"; }
        public class CharModel { public char FieldName = 'a'; }
        public class BoolModel { public bool FieldName = true; }
        public class IntModel { public int FieldName = int.MaxValue; }
        public class ByteModel { public byte FieldName = byte.MaxValue; }
        public class SByteModel { public sbyte FieldName = sbyte.MaxValue; }
        public class ShortModel { public short FieldName = short.MaxValue; }
        public class UShortModel { public ushort FieldName = ushort.MaxValue; }
        public class UIntModel { public uint FieldName = uint.MaxValue; }
        public class LongModel { public long FieldName = long.MaxValue; }
        public class ULongModel { public ulong FieldName = ulong.MaxValue; }
        public class FloatModel { public float FieldName = float.MaxValue; }
        public class DoubleModel { public double FieldName = double.MaxValue; }
        public class DateTimeModel { public DateTime FieldName = DateTime.MaxValue; }

        [TestMethod]
        public void TrySerializeString() => Assert.AreEqual("{\"FieldName\":string(\"hello world\\n\")}", TsonConvert.SerializeObject(new StringModel()));

        [TestMethod]
        public void TrySerializeChar() => Assert.AreEqual("{\"FieldName\":char(\"a\")}", TsonConvert.SerializeObject(new CharModel()));

        [TestMethod]
        public void TrySerializeBool() => Assert.AreEqual("{\"FieldName\":bool(true)}", TsonConvert.SerializeObject(new BoolModel()));

        [TestMethod]
        public void TrySerializeInt() => Assert.AreEqual("{\"FieldName\":int(2147483647)}", TsonConvert.SerializeObject(new IntModel()));

        [TestMethod]
        public void TrySerializeByte() => Assert.AreEqual("{\"FieldName\":byte(255)}", TsonConvert.SerializeObject(new ByteModel()));

        [TestMethod]
        public void TrySerializeSByte() => Assert.AreEqual("{\"FieldName\":sbyte(127)}", TsonConvert.SerializeObject(new SByteModel()));

        [TestMethod]
        public void TrySerializeShort() => Assert.AreEqual("{\"FieldName\":short(32767)}", TsonConvert.SerializeObject(new ShortModel()));

        [TestMethod]
        public void TrySerializeUShort() => Assert.AreEqual("{\"FieldName\":ushort(65535)}", TsonConvert.SerializeObject(new UShortModel()));

        [TestMethod]
        public void TrySerializeUInt() => Assert.AreEqual("{\"FieldName\":uint(4294967295)}", TsonConvert.SerializeObject(new UIntModel()));

        [TestMethod]
        public void TrySerializeLong() => Assert.AreEqual("{\"FieldName\":long(9223372036854775807)}", TsonConvert.SerializeObject(new LongModel()));

        [TestMethod]
        public void TrySerializeULong() => Assert.AreEqual("{\"FieldName\":ulong(18446744073709551615)}", TsonConvert.SerializeObject(new ULongModel()));

        [TestMethod]
        public void TrySerializeFloat() => Assert.AreEqual("{\"FieldName\":float(3.402823E+38)}", TsonConvert.SerializeObject(new FloatModel()));

        [TestMethod]
        public void TrySerializeDouble() => Assert.AreEqual("{\"FieldName\":double(1.79769313486232E+308)}", TsonConvert.SerializeObject(new DoubleModel()));

        [TestMethod]
        public void TrySerializeDateTime() => Assert.AreEqual("{\"FieldName\":datetime(\"9999-12-31T23:59:59.9999999\")}", TsonConvert.SerializeObject(new DateTimeModel()));
    }
}
