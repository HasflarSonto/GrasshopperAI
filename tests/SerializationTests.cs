using GHPT.Prompts;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.Json;

namespace UnitTests
{
    public class SerializationTests
    {

        private static PromptData GetTestPrompt()
        {
            IEnumerable<Addition> additions = new List<Addition>
                {
                    new Addition() {
                        Name = "Number Slider",
                        Id = 1,
                        Value = "0..50..100"
                    },
                    new Addition() {
                        Name = "Number Slider",
                        Id = 2,
                        Value = "0..90..360"
                    },
                    new Addition()
                    {
                        Name = "Addition",
                        Id = 3,
                    }
                };

            IEnumerable<ConnectionPairing> connections = new List<ConnectionPairing>
                {
                    new ConnectionPairing()
                    {
                        FromComponentId = 3,
                        FromParameter = "A",
                        ToComponentId = 1,
                        ToParameter = "number"
                    },
                    new ConnectionPairing()
                    {
                        FromComponentId = 3,
                        FromParameter = "B",
                        ToComponentId = 2,
                        ToParameter = "number"
                    }
                };

            return new PromptData()
            {
                Additions = additions,
                Connections = connections,
                Advice = "Test prompt"
            };
        }

        [Test]
        public void Test1()
        {
            PromptData promptData = GetTestPrompt();
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(promptData, options);

            Assert.Pass();
        }
    }
}