using System;
using System.Threading.Tasks;
using System.Text.Json;
using GHPT.Utils;
using GHPT.IO;
using System.IO;

namespace GHPT.Builders
{
    public class ComponentAnalyzerBuilder
    {
        private readonly GPTClient _gptClient;
        private readonly string _examples;
        private readonly string _bestPractices;

        public ComponentAnalyzerBuilder(GPTClient gptClient)
        {
            _gptClient = gptClient;
            _examples = ComponentExamples.GetComplexExamples();
            _bestPractices = ComponentBestPractices.GetBestPractices();
        }

        public async Task<AnalysisResult> AnalyzeRequest(string userRequest)
        {
            try
            {
                string prompt = ComponentAnalyzerPrompt.GetPrompt(userRequest, _examples, _bestPractices);
                string response = await _gptClient.GetCompletion(prompt);

                // Check if the response indicates the request is too complex
                if (response.Contains("{TOO_COMPLEX}"))
                {
                    return new AnalysisResult
                    {
                        IsTooComplex = true,
                        Error = "The request is too complex to process automatically."
                    };
                }

                // Parse the JSON response
                var result = JsonSerializer.Deserialize<AnalysisResult>(response);
                if (result == null)
                {
                    throw new Exception("Failed to parse the AI response");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error analyzing request: {ex.Message}", ex);
            }
        }
    }

    public class AnalysisResult
    {
        public Analysis Analysis { get; set; }
        public Outline Outline { get; set; }
        public string[] BestPractices { get; set; }
        public bool IsTooComplex { get; set; }
        public string Error { get; set; }
    }

    public class Analysis
    {
        public string[] KeyComponents { get; set; }
        public string SimilarExample { get; set; }
        public string[] RequiredParameters { get; set; }
    }

    public class Outline
    {
        public Step[] Steps { get; set; }
        public Connection[] Connections { get; set; }
    }

    public class Step
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public string[] Components { get; set; }
        public string[] Parameters { get; set; }
    }

    public class Connection
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Parameter { get; set; }
    }
} 