using GHPT.Prompts;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace GHPT.Builders
{
    public class PromptCoordinator
    {
        private readonly ComponentAnalyzerBuilder _analyzerBuilder;
        private readonly ComponentGeneratorBuilder _generatorBuilder;
        private readonly List<ChatMessage> _conversationHistory;
        private const int MAX_HISTORY_LENGTH = 10;

        public PromptCoordinator()
        {
            _analyzerBuilder = new ComponentAnalyzerBuilder();
            _generatorBuilder = new ComponentGeneratorBuilder();
            _conversationHistory = new List<ChatMessage>();
        }

        public async Task<CoordinatorResult> ProcessRequest(string userRequest)
        {
            try
            {
                // 1. Analyze the request using ComponentAnalyzerBuilder
                var analysisResult = await _analyzerBuilder.AnalyzeRequest(userRequest);

                // 2. Based on complexity, either:
                if (analysisResult.Type == "simple")
                {
                    // For simple requests, convert SimpleComponent to PromptData
                    var simpleComponent = analysisResult.SimpleComponent;
                    var promptData = new PromptData
                    {
                        Advice = "Created a simple component based on your request",
                        Additions = new List<Addition>
                        {
                            new Addition
                            {
                                Name = simpleComponent.Name,
                                Id = 1,
                                Value = "",
                                Tier = 0
                            }
                        },
                        Connections = new List<ConnectionPairing>()
                    };

                    return new CoordinatorResult
                    {
                        Success = true,
                        Error = null,
                        Result = promptData
                    };
                }
                else if (analysisResult.Type == "complex")
                {
                    // For complex requests, pass the analysis to the generator
                    var generationResult = await _generatorBuilder.GenerateComponents(analysisResult);
                    
                    // Convert GenerationResult to PromptData
                    var promptData = new PromptData
                    {
                        Advice = generationResult.Advice,
                        Additions = generationResult.Additions.Select(a => new Addition
                        {
                            Name = a.Name,
                            Id = a.Id,
                            Value = a.Value,
                            Tier = 0
                        }).ToList(),
                        Connections = generationResult.Connections.Select(c => new ConnectionPairing
                        {
                            FromComponentId = c.From.Id,
                            FromParameter = c.From.ParameterName,
                            ToComponentId = c.To.Id,
                            ToParameter = c.To.ParameterName
                        }).ToList()
                    };
                    
                    return new CoordinatorResult
                    {
                        Success = true,
                        Error = null,
                        Result = promptData
                    };
                }
                else
                {
                    throw new InvalidOperationException($"Invalid analysis result type: {analysisResult.Type}");
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