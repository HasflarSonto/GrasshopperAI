using GHPT.Prompts;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GHPT.Utils;
using GHPT.IO;
using System.Collections.Generic;

namespace GHPT.Builders
{
    public class ComponentAnalyzerBuilder
    {
        private readonly string _componentDocumentation;
        private readonly string _simpleExamples;
        private readonly string _complexExamples;
        private readonly string _bestPractices;

        public ComponentAnalyzerBuilder()
        {
            _componentDocumentation = ResourceLoader.LoadComponentDocumentation();
            _simpleExamples = ResourceLoader.LoadSimpleExamples();
            _complexExamples = ResourceLoader.LoadComplexExamples();
            _bestPractices = ResourceLoader.LoadBestPractices();
        }

        public async Task<AnalysisResult> AnalyzeRequest(string userRequest)
        {
            try
            {
                LoggingUtil.LogInfo($"Starting analysis of request: {userRequest}");

                // 1. Load and format the prompt template
                var promptTemplate = ResourceLoader.LoadPromptTemplate("component_analyzer.txt")
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
                
                // Add user message
                payload.AddUserMessage(userRequest);

                // 3. Send request to API
                var configs = ConfigUtil.Configs;
                if (!configs.Any())
                {
                    LoggingUtil.LogError("No valid GPT configuration found");
                    return new AnalysisResult { Type = "error", Error = "No valid GPT configuration found" };
                }

                var config = configs.First();
                var response = await ClientUtil.Ask(config, payload);
                if (response == null || response.Choices == null || !response.Choices.Any())
                {
                    LoggingUtil.LogError("No response received from API");
                    return new AnalysisResult { Type = "error", Error = "No response received from API" };
                }

                // 4. Parse the response
                var content = response.Choices.First().Message.Content;
                try
                {
                    var result = JsonConvert.DeserializeObject<AnalysisResult>(content);
                    if (result == null)
                    {
                        LoggingUtil.LogError("Failed to parse API response");
                        return new AnalysisResult { Type = "error", Error = "Failed to parse API response" };
                    }
                    return result;
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    LoggingUtil.LogError($"Error parsing response: {ex.Message}");
                    return new AnalysisResult { Type = "error", Error = $"Error parsing response: {ex.Message}" };
                }
            }
            catch (Exception ex)
            {
                LoggingUtil.LogError($"Error in AnalyzeRequest: {ex.Message}");
                return new AnalysisResult { Type = "error", Error = ex.Message };
            }
        }
    }

    public class ParsedRequest
    {
        public string RawInput { get; set; }
        public List<string> Components { get; set; }
        public List<Parameter> Parameters { get; set; }
        public List<Connection> Connections { get; set; }
        public bool HasConditionalLogic { get; set; }
        public bool HasStateDependencies { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsComplex { get; set; }
    }

    public class Connection
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Parameter { get; set; }
    }

    public enum ComplexityType
    {
        Simple,
        Complex
    }

    public class AnalysisResult
    {
        public string Type { get; set; } // "simple" or "complex"
        public SimpleComponent SimpleComponent { get; set; }
        public Analysis Analysis { get; set; }
        public Plan Plan { get; set; }
        public string Error { get; set; }
    }

    public class SimpleComponent
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public List<string> Connections { get; set; }
    }

    public class Analysis
    {
        public string CurrentState { get; set; }
        public List<string> RequiredChanges { get; set; }
        public List<string> Dependencies { get; set; }
    }

    public class Plan
    {
        public List<Step> Steps { get; set; }
        public string ExpectedOutcome { get; set; }
    }

    public class Step
    {
        public int StepNumber { get; set; }
        public string Action { get; set; }
        public List<string> Components { get; set; }
        public List<string> Connections { get; set; }
        public List<string> Parameters { get; set; }
    }
} 