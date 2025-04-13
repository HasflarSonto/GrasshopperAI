using System;
using System.Threading.Tasks;
using System.Text.Json;
using GHPT.Utils;
using GHPT.IO;
using System.Collections.Generic;
using System.Linq;
using GHPT.Prompts;

namespace GHPT.Builders
{
    public class PromptCoordinator
    {
        private readonly ComplexityCheckBuilder _complexityChecker;
        private readonly ComponentAnalyzerAndGeneratorBuilder _simpleBuilder;
        private readonly ComponentAnalyzerBuilder _analyzer;
        private readonly ComponentGeneratorBuilder _generator;
        private readonly List<ChatMessage> _conversationHistory;
        private const int MAX_HISTORY_LENGTH = 10;

        public PromptCoordinator(GPTClient gptClient)
        {
            _complexityChecker = new ComplexityCheckBuilder(gptClient);
            _simpleBuilder = new ComponentAnalyzerAndGeneratorBuilder(gptClient);
            _analyzer = new ComponentAnalyzerBuilder(gptClient);
            _generator = new ComponentGeneratorBuilder(gptClient);
            _conversationHistory = new List<ChatMessage>();
        }

        public async Task<CoordinatorResult> ProcessRequest(string request)
        {
            try
            {
                // 1. Check complexity
                string complexity = await _complexityChecker.CheckComplexity(request);
                
                if (complexity == "simple")
                {
                    // For simple requests, use the combined analyzer and generator
                    var result = await _simpleBuilder.AnalyzeAndGenerate(request);
                    if (result.IsTooComplex)
                    {
                        return new CoordinatorResult
                        {
                            Success = false,
                            Error = "Request was determined to be too complex for simple processing",
                            Result = null
                        };
                    }

                    return new CoordinatorResult
                    {
                        Success = true,
                        Error = null,
                        Result = ConvertToPromptData(result, complexity, "Simple request - using direct component generation")
                    };
                }
                else
                {
                    // For complex requests, use the full pipeline
                    // 2. Analyze the request
                    var analysis = await _analyzer.AnalyzeRequest(request);
                    if (analysis.IsTooComplex)
                    {
                        return new CoordinatorResult
                        {
                            Success = false,
                            Error = "Request was determined to be too complex to process",
                            Result = null
                        };
                    }

                    // 3. Generate components based on analysis
                    var generationResult = await _generator.GenerateComponents(analysis);
                    
                    return new CoordinatorResult
                    {
                        Success = true,
                        Error = null,
                        Result = ConvertToPromptData(generationResult, complexity, string.Join(", ", analysis.Analysis.KeyComponents))
                    };
                }
            }
            catch (Exception ex)
            {
                return new CoordinatorResult
                {
                    Success = false,
                    Error = $"Error processing request: {ex.Message}",
                    Result = null
                };
            }
        }

        private PromptData ConvertToPromptData(ComponentAnalysisResult result, string complexity, string analysis)
        {
            return new PromptData
            {
                Advice = result.Advice,
                Complexity = complexity,
                Analysis = analysis,
                Additions = result.Additions.Select(a => new Addition
                {
                    Name = a.Name,
                    Id = a.Id,
                    Value = a.Value?.ToString(),
                    Tier = 0 // Default tier, can be adjusted based on component type
                }).ToList(),
                Connections = result.Connections.Select(c => new ConnectionPairing
                {
                    FromComponentId = c.From.Id,
                    FromParameter = c.From.ParameterName,
                    ToComponentId = c.To.Id,
                    ToParameter = c.To.ParameterName
                }).ToList()
            };
        }

        private PromptData ConvertToPromptData(GenerationResult result, string complexity, string analysis)
        {
            return new PromptData
            {
                Advice = result.Advice,
                Complexity = complexity,
                Analysis = analysis,
                Additions = result.Additions.Select(a => new Addition
                {
                    Name = a.Name,
                    Id = a.Id,
                    Value = a.Value?.ToString(),
                    Tier = 0 // Default tier, can be adjusted based on component type
                }).ToList(),
                Connections = result.Connections.Select(c => new ConnectionPairing
                {
                    FromComponentId = c.From.Id,
                    FromParameter = c.From.ParameterName,
                    ToComponentId = c.To.Id,
                    ToParameter = c.To.ParameterName
                }).ToList()
            };
        }

        public void AddMessage(string role, string content)
        {
            _conversationHistory.Add(new ChatMessage { Role = role, Content = content });
            if (_conversationHistory.Count > MAX_HISTORY_LENGTH)
            {
                _conversationHistory.RemoveAt(0);
            }
        }

        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }

        public IReadOnlyList<ChatMessage> GetHistory() => _conversationHistory.AsReadOnly();
    }

    public class CoordinatorResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public PromptData Result { get; set; }
    }
} 