using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mapster;
using Newtonsoft.Json;
using Tson;

namespace TsonBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TsonVsNewtonsoftJson>();
        }
    }

    public class Temperatures
    {
        [JsonProperty("description")]
        [TsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("data")]
        [TsonProperty("data")]
        public Dictionary<string, Datum> Data { get; set; }
    }

    public class Datum
    {
        [JsonProperty("value")]
        [TsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("anomaly")]
        [TsonProperty("anomaly")]
        public string Anomaly { get; set; }
    }

    public class Description
    {
        [JsonProperty("title")]
        [TsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("units")]
        [TsonProperty("units")]
        public string Units { get; set; }

        [JsonProperty("base_period")]
        [TsonProperty("base_period")]
        public string BasePeriod { get; set; }
    }

    public class TsonVsNewtonsoftJson
    {
        private string json_string;
        private string tson_string;
        private Temperatures object_to_serialize;
        private readonly int iteration_count = 50;

        [GlobalSetup]
        public void Setup()
        {
            json_string = "{ \"description\": { \"title\": \"Contiguous U.S., Average Temperature\", \"units\": \"Degrees Fahrenheit\", \"base_period\": \"1901-2000\" }, \"data\": { \"189512\": { \"value\": \"50.34\", \"anomaly\": \"-1.68\" }, \"189612\": { \"value\": \"51.99\", \"anomaly\": \"-0.03\" }, \"189712\": { \"value\": \"51.56\", \"anomaly\": \"-0.46\" } } }";
            tson_string = "{\"description\":{\"title\":string(\"Contiguous U.S., Average Temperature\"),\"units\":string(\"Degrees Fahrenheit\"),\"base_period\":string(\"1901-2000\")},\"data\":{\"189512\":{\"value\":string(\"50.34\"),\"anomaly\":string(\"-1.68\")},\"189612\":{\"value\":string(\"51.99\"),\"anomaly\":string(\"-0.03\")},\"189712\":{\"value\":string(\"51.56\"),\"anomaly\":string(\"-0.46\")}}}";
            object_to_serialize = JsonConvert.DeserializeObject<Temperatures>(json_string);
        }

        [Benchmark]
        public void DeserializeFromJSON()
        {
            for (var i = 0; i < iteration_count; i++)
                JsonConvert.DeserializeObject<Temperatures>(json_string);
        }

        [Benchmark]
        public void DeserializeFromTSON()
        {
            for (var i = 0; i < iteration_count; i++)
                TsonConvert.DeserializeObject<Temperatures>(tson_string);
        }

        [Benchmark]
        public void SerializeToJSON()
        {
            for (var i = 0; i < iteration_count; i++)
                JsonConvert.SerializeObject(object_to_serialize, Newtonsoft.Json.Formatting.None);
        }

        [Benchmark]
        public void SerializeToTSON()
        {
            for (var i = 0; i < iteration_count; i++)
                TsonConvert.SerializeObject(object_to_serialize, Tson.Formatting.None);
        }
    }
}
