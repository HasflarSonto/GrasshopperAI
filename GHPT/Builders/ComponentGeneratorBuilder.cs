using GHPT.Prompts;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GHPT.Utils;
using GHPT.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GHPT.Builders
{
    public class ComponentGeneratorBuilder
    {
        private readonly string _componentDocumentation;
        private readonly string _simpleExamples;
        private readonly string _complexExamples;
        private readonly string _bestPractices;

        public ComponentGeneratorBuilder()
        {
            _componentDocumentation = ResourceLoader.LoadComponentDocumentation();
            _simpleExamples = ResourceLoader.LoadSimpleExamples();
            _complexExamples = ResourceLoader.LoadComplexExamples();
            _bestPractices = ResourceLoader.LoadBestPractices();
        }

        public async Task<PromptData> GenerateComponents(AnalysisResult analysis)
        {
            try
            {
                LoggingUtil.LogInfo($"Starting component generation for analysis type: {analysis.Type}");

                // 1. Load and format the prompt template
                var promptTemplate = ResourceLoader.LoadPromptTemplate("component_generator.txt")
                    .Replace("{ComponentDocumentation}", _componentDocumentation)
                    .Replace("{SimpleExamples}", _simpleExamples)
                    .Replace("{ComplexExamples}", _complexExamples)
                    .Replace("{BestPractices}", _bestPractices);

                // 2. Create the payload with proper message formatting
                var payload = new AskPayload();
                payload.Model = "gpt-4";
                payload.Temperature = 0.7;
                
                // Add system message with documentation and examples
                payload.AddSystemMessage(promptTemplate);
                
                // Add user message with analysis
                payload.AddUserMessage(JsonConvert.SerializeObject(analysis));

                // 3. Send request to API
                var configs = ConfigUtil.Configs;
                if (!configs.Any())
                {
                    LoggingUtil.LogError("No valid GPT configuration found");
                    throw new InvalidOperationException("No valid GPT configuration found");
                }

                var config = configs.First();
                var response = await ClientUtil.Ask(config, payload);
                var content = response.Choices.FirstOrDefault()?.Message?.Content;

                if (string.IsNullOrEmpty(content))
                {
                    LoggingUtil.LogError("No response received from LLM");
                    throw new InvalidOperationException("No response received from LLM");
                }

                // 4. Parse the response into PromptData
                var result = JsonConvert.DeserializeObject<PromptData>(content);
                
                // 5. Validate the response using shared validation
                if (!ComponentValidationUtils.ValidateComponentJson(result))
                {
                    LoggingUtil.LogError("Invalid component JSON in response");
                    throw new InvalidOperationException("Invalid component JSON in response");
                }

                LoggingUtil.LogInfo($"Successfully generated components. Count: {result.Additions?.Count() ?? 0}");
                return result;
            }
            catch (Exception ex)
            {
                LoggingUtil.LogError("Error generating components", ex);
                throw;
            }
        }
    }

    public class GenerationResult
    {
        public string Advice { get; set; }
        public List<ComponentAddition> Additions { get; set; }
        public List<ComponentConnection> Connections { get; set; }
    }

    public class ComponentAddition
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
        public PointF Position { get; set; }
    }

    public class ComponentConnection
    {
        public ConnectionPoint From { get; set; }
        public ConnectionPoint To { get; set; }
    }

    public class ConnectionPoint
    {
        public int Id { get; set; }
        public string ParameterName { get; set; }
    }
} 