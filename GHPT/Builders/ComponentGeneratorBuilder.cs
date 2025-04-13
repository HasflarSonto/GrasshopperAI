using System;
using System.Threading.Tasks;
using System.Text.Json;
using GHPT.Utils;
using GHPT.IO;
using GHPT.Prompts;
using System.IO;
using System.Drawing;
using System.Linq;
using GHPT.Models;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GHPT.Builders
{
    public class ComponentGeneratorBuilder
    {
        private readonly GPTClient _gptClient;
        private readonly string _documentation;
        private readonly string _examples;

        public ComponentGeneratorBuilder(GPTClient gptClient)
        {
            _gptClient = gptClient;
            _documentation = ComponentDocumentation.GetDocumentation();
            _examples = ComponentExamples.GetComplexExamples();
        }

        public async Task<GenerationResult> GenerateComponents(GHPT.Builders.AnalysisResult analysis)
        {
            try
            {
                var promptAnalysis = ConvertToPromptAnalysis(analysis);
                string prompt = ComponentGeneratorPrompt.GetPrompt(promptAnalysis, _documentation, _examples);
                string response = await _gptClient.GetCompletion(prompt);

                // Extract JSON from markdown code block if present
                var jsonMatch = Regex.Match(response, @"```json\s*(\{[\s\S]*?\})\s*```");
                string jsonContent = jsonMatch.Success ? jsonMatch.Groups[1].Value : response;

                // Parse the JSON response with proper options
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                // First deserialize into a dynamic object to validate structure
                var jsonDocument = JsonDocument.Parse(jsonContent);
                var root = jsonDocument.RootElement;

                // Create the result object
                var result = new GenerationResult
                {
                    Advice = root.GetProperty("Advice").GetString(),
                    Additions = root.GetProperty("Additions").Deserialize<Component[]>(options),
                    Connections = root.GetProperty("Connections").Deserialize<ConnectionPairing[]>(options)
                };

                if (result == null)
                {
                    throw new Exception("Failed to parse the AI response");
                }

                // Validate the generated configuration
                ValidateConfiguration(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating components: {ex.Message}", ex);
            }
        }

        private GHPT.Prompts.AnalysisResult ConvertToPromptAnalysis(GHPT.Builders.AnalysisResult analysis)
        {
            var result = new GHPT.Prompts.AnalysisResult
            {
                Type = analysis.IsTooComplex ? "complex" : "simple",
                Error = analysis.Error
            };

            // Create Analysis object
            result.Analysis = new GHPT.Prompts.Analysis
            {
                Description = string.Join(", ", analysis.Analysis.KeyComponents),
                RequiredComponents = analysis.Analysis.KeyComponents.ToList(),
                RequiredConnections = analysis.Outline.Connections.Select(c => $"{c.From} -> {c.To} ({c.Parameter})").ToList()
            };

            // Create Plan object
            result.Plan = new GHPT.Prompts.Plan
            {
                Steps = analysis.Outline.Steps.Select(s => s.Description).ToList(),
                RequiredComponents = analysis.Outline.Steps.SelectMany(s => s.Components).ToList()
            };

            // Create ComplexAnalysis object
            result.ComplexAnalysis = new GHPT.Prompts.ComplexAnalysis
            {
                Description = string.Join(", ", analysis.Analysis.KeyComponents),
                RequiredComponents = analysis.Analysis.KeyComponents.ToList(),
                RequiredConnections = analysis.Outline.Connections.Select(c => $"{c.From} -> {c.To} ({c.Parameter})").ToList()
            };

            return result;
        }

        private void ValidateConfiguration(GenerationResult result)
        {
            if (result.Additions == null || result.Connections == null)
            {
                throw new Exception("Invalid configuration: Additions or Connections are null");
            }

            // Validate component IDs are unique
            var componentIds = result.Additions.Select(c => c.Id).ToList();
            if (componentIds.Distinct().Count() != componentIds.Count)
            {
                throw new Exception("Duplicate component IDs found");
            }

            // Validate all connections reference existing components
            foreach (var connection in result.Connections)
            {
                if (!componentIds.Contains(connection.FromComponentId))
                {
                    throw new Exception($"Connection references non-existent source component ID: {connection.FromComponentId}");
                }
                if (!componentIds.Contains(connection.ToComponentId))
                {
                    throw new Exception($"Connection references non-existent target component ID: {connection.ToComponentId}");
                }
            }

            // Validate component positions are reasonable
            foreach (var component in result.Additions)
            {
                if (component.Position.X < 0 || component.Position.Y < 0)
                {
                    throw new Exception($"Invalid position for component {component.Id}: ({component.Position.X}, {component.Position.Y})");
                }
            }
        }
    }

    public class GenerationResult
    {
        public string Advice { get; set; }
        public Component[] Additions { get; set; }
        public ConnectionPairing[] Connections { get; set; }
    }
} 