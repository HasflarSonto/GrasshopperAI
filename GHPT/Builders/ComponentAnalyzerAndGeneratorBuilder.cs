using System;
using System.Threading.Tasks;
using System.Text.Json;
using GHPT.Utils;
using GHPT.IO;
using GHPT.Models;
using GHPT.Prompts;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GHPT.Builders
{
    public class ComponentAnalyzerAndGeneratorBuilder
    {
        private readonly GPTClient _gptClient;
        private readonly string _documentation;
        private readonly string _simpleExamples;

        public ComponentAnalyzerAndGeneratorBuilder(GPTClient gptClient)
        {
            _gptClient = gptClient;
            _documentation = ComponentDocumentation.GetDocumentation();
            _simpleExamples = ComponentExamples.GetSimpleExamples();
        }

        public async Task<ComponentAnalysisResult> AnalyzeAndGenerate(string userRequest)
        {
            try
            {
                string prompt = ComponentAnalyzerAndGeneratorPrompt.GetPrompt(userRequest, _documentation, _simpleExamples);
                string response = await _gptClient.GetCompletion(prompt);

                // Check if the response indicates the request is too complex
                if (response.Contains("{TOO_COMPLEX}"))
                {
                    return new ComponentAnalysisResult
                    {
                        IsTooComplex = true,
                        Error = "The request is too complex to process automatically."
                    };
                }

                // Extract JSON from markdown code block if present
                var jsonMatch = Regex.Match(response, @"```json\s*(\{[\s\S]*?\})\s*```");
                string jsonContent = jsonMatch.Success ? jsonMatch.Groups[1].Value : response;

                if (string.IsNullOrEmpty(jsonContent))
                {
                    throw new Exception("Invalid response format: No JSON content found");
                }

                // Parse the JSON content with proper options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                // First deserialize into a dynamic object to validate structure
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var root = jsonDocument.RootElement;

                // Create the result object
                var result = new ComponentAnalysisResult
                {
                    Advice = root.GetProperty("Advice").GetString(),
                    Additions = root.GetProperty("Additions").Deserialize<Component[]>(options),
                    Connections = root.GetProperty("Connections").Deserialize<ConnectionPairing[]>(options),
                    IsTooComplex = false,
                    Error = null
                };

                if (result == null)
                {
                    throw new Exception("Failed to parse the AI response");
                }

                // Validate the result
                if (result.Additions != null && result.Connections != null)
                {
                    foreach (var connection in result.Connections)
                    {
                        if (!connection.IsValid())
                        {
                            throw new Exception("Invalid connection: From or To is invalid");
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error analyzing and generating components: {ex.Message}", ex);
            }
        }
    }

    public class ResponsePayload
    {
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }

    public class ComponentAnalysisResult
    {
        public string Advice { get; set; }
        public Component[] Additions { get; set; }
        public ConnectionPairing[] Connections { get; set; }
        public bool IsTooComplex { get; set; }
        public string Error { get; set; }
    }
} 