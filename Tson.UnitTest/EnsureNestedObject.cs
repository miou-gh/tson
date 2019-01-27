using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Tson.NET;

namespace Tson.UnitTest
{
    [TestClass]
    public class EnsureNestedObject
    {
        public Person PersonModel => new Person()
        {
            FirstName = "John",
            LastName = "Smith",
            Age = 25,
            Address = new Address()
            {
                StreetAddress = "21 2nd Street",
                City = "New York",
                State = "NY",
                PostalCode = "10021"
            },
            PhoneNumber = new List<PhoneNumber>()
            {
                new PhoneNumber()
                {
                    Type = "home",
                    Number = "212 555-1234",
                },

                new PhoneNumber()
                {
                    Type = "fax",
                    Number = "646 555-4567",
                }
            },
            Gender = new Gender()
            {
                Type = "male"
            }
        };

        public class Address
        {
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
        }

        public class PhoneNumber
        {
            public string Type { get; set; }
            public string Number { get; set; }
        }

        public class Gender
        {
            public string Type { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
            public Address Address { get; set; }
            public List<PhoneNumber> PhoneNumber { get; set; }
            public Gender Gender { get; set; }
        }

        [TestMethod]
        public void CheckSerializationResultForNestedObject()
        {
            var expected = "{\"FirstName\":string(\"John\"),\"LastName\":string(\"Smith\"),\"Age\":int(25),\"Address\":{\"StreetAddress\":string(\"21 2nd Street\")" +
                             ",\"City\":string(\"New York\"),\"State\":string(\"NY\"),\"PostalCode\":string(\"10021\")},\"PhoneNumber\":[{\"Type\":string(\"home\")" +
                             ",\"Number\":string(\"212 555-1234\")},{\"Type\":string(\"fax\"),\"Number\":string(\"646 555-4567\")}],\"Gender\":{\"Type\":string(\"male\")}}";

            var actual = TsonConvert.SerializeObject(this.PersonModel);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CheckReserializationResultForNestedObject()
        {
            var actual = TsonConvert.SerializeObject(this.PersonModel);
            var deserialize = TsonConvert.DeserializeObject(actual);
            var reserialize = TsonConvert.SerializeObject(deserialize);

            Assert.AreEqual(actual, reserialize);
        }
    }
}
