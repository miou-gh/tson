using System;
using System.Collections.Generic;
using Mapster;
using Newtonsoft.Json;
using Tson;

namespace TsonBenchmarks
{
    public class Person
    {
        public string Name { get; set; }
        public Pet Pet { get; set; }
    }

    public class Pet
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //var pet = new Dictionary<string, object> { { "Name", "Fluffy" }, { "Type", "Cat" },
            //    { "Data", new Dictionary<string, Datum>() { { "test", new Datum() { Anomaly = "AnomalyValue", Value = "ValueWoo" } } } } };

            //var dictionary = new Dictionary<string, object>
            //{
            //    {"Name", "Alice"},
            //    {"Pet", pet},
            //    {"Description", "hello world" }
            //};

            //var person = dictionary.Adapt<Person>();
            //;

            //var serialized = TsonConvert.SerializeObject(new Person() { Name = "Alice", Pet = new Pet() { Name = "Fluffy", Type = "Cat" } });
            var serialized = "{\"Name\":string(\"Alice\"),\"Pet\":{\"Name\":string(\"Fluffy\"),\"Type\":string(\"Cat\")}}";
            var deserialized = TsonConvert.DeserializeObject<Person>(serialized);

            new TsonVsNewtonsoftJson();
        }
    }

    public class Temperatures
    {
        [JsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, Datum> Data { get; set; }
    }

    public class Datum
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("anomaly")]
        public string Anomaly { get; set; }
    }

    public class Description
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("units")]
        public string Units { get; set; }

        [JsonProperty("base_period")]
        public string BasePeriod { get; set; }
    }

    public class TsonVsNewtonsoftJson
    {
        private string json_string;
        private string tson_string;

        public TsonVsNewtonsoftJson()
        {
            json_string = "{ \"description\": { \"title\": \"Contiguous U.S., Average Temperature\", \"units\": \"Degrees Fahrenheit\", \"base_period\": \"1901-2000\" }, \"data\": { \"189512\": { \"value\": \"50.34\", \"anomaly\": \"-1.68\" }, \"189612\": { \"value\": \"51.99\", \"anomaly\": \"-0.03\" }, \"189712\": { \"value\": \"51.56\", \"anomaly\": \"-0.46\" } } }";
            var json_des = JsonConvert.DeserializeObject<Temperatures>(json_string);
            tson_string = TsonConvert.SerializeObject(json_des);
            //System.IO.File.WriteAllText(@"C:\Users\atilla\Desktop\Projects\data.txt", TsonConvert.SerializeObject(json_des, Tson.Formatting.Indented));

            var des = TsonConvert.DeserializeObject<Temperatures>(tson_string);

            DeserializeFromTSON();
            ;
        }

        public void DeserializeFromJSON() => JsonConvert.DeserializeObject<Temperatures>(json_string);
        public void DeserializeFromTSON() => TsonConvert.DeserializeObject<Temperatures>(tson_string);
    }
}
