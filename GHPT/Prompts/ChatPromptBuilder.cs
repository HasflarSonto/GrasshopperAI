using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace GHPT.Prompts
{
    public class ChatPromptBuilder
    {
        private readonly string _examplesPath;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly Dictionary<int, ComponentState> _componentStates;
        private const int MAX_HISTORY_LENGTH = 10; // Keep last 10 messages

        public ChatPromptBuilder(string examplesPath)
        {
            _examplesPath = examplesPath;
            _conversationHistory = new List<ChatMessage>();
            _componentStates = new Dictionary<int, ComponentState>();
        }

        public string BuildPrompt(string userMessage, GrasshopperState currentState)
        {
            var prompt = new StringBuilder();
            
            // System prompt
            prompt.AppendLine("You are a Grasshopper expert assistant. You will help users create and modify Grasshopper definitions through natural conversation.");
            prompt.AppendLine("\nImportant rules:");
            prompt.AppendLine("1. Remember the entire conversation history");
            prompt.AppendLine("2. Track all components created and their relationships");
            prompt.AppendLine("3. Understand references to previous components and actions");
            prompt.AppendLine("4. Maintain context about the user's design intent");
            prompt.AppendLine("5. Be concise but helpful in your responses");
            prompt.AppendLine("6. ALWAYS format your response in the following JSON structure:");
            prompt.AppendLine("   {\n     \"Advice\": \"Your advice to the user\",\n     \"Additions\": [\n       {\n         \"Name\": \"ComponentName\",\n         \"Id\": uniqueNumber,\n         \"value\": \"optionalValue\"\n       }\n     ],\n     \"Connections\": [\n       {\n         \"To\": {\n           \"Id\": componentId,\n           \"ParameterName\": \"parameterName\"\n         },\n         \"From\": {\n           \"Id\": componentId,\n           \"ParameterName\": \"parameterName\"\n         }\n       }\n     ]\n   }");
            prompt.AppendLine("7. NEVER deviate from this JSON format");
            prompt.AppendLine("8. ALWAYS include an Advice field with helpful instructions");
            prompt.AppendLine("9. ALWAYS include Additions for any new components needed");
            prompt.AppendLine("10. ALWAYS include Connections for any component relationships");
            
            // Current state
            prompt.AppendLine("\nCurrent Grasshopper Definition State:");
            prompt.AppendLine(FormatGrasshopperState(currentState));
            
            // Conversation history
            prompt.AppendLine("\nConversation History:");
            prompt.AppendLine(FormatConversationHistory());
            
            // Current request
            prompt.AppendLine("\nUser's Current Request:");
            prompt.AppendLine(userMessage);
            
            // Task instructions
            prompt.AppendLine("\nYour Task:");
            prompt.AppendLine("1. Understand the request in context of previous conversation");
            prompt.AppendLine("2. Reference any relevant previous components or actions");
            prompt.AppendLine("3. Plan the necessary Grasshopper operations");
            prompt.AppendLine("4. Provide a clear, concise response in the exact JSON format specified above");
            prompt.AppendLine("5. Update your internal state for future reference");
            
            // Examples
            prompt.AppendLine("\nHere are some examples of how to structure your responses:");
            prompt.AppendLine(File.ReadAllText(_examplesPath));
            
            return prompt.ToString();
        }

        private string FormatGrasshopperState(GrasshopperState state)
        {
            var sb = new StringBuilder();
            
            foreach (var component in state.Components)
            {
                sb.AppendLine($"Component {component.Id} ({component.Type}):");
                sb.AppendLine($"  Position: ({component.Position.X}, {component.Position.Y})");
                
                if (component.Parameters.Any())
                {
                    sb.AppendLine("  Parameters:");
                    foreach (var param in component.Parameters)
                    {
                        sb.AppendLine($"    {param.Key}: {param.Value}");
                    }
                }
                
                if (component.Connections.Any())
                {
                    sb.AppendLine("  Connections:");
                    foreach (var conn in component.Connections)
                    {
                        sb.AppendLine($"    {conn.FromComponentId}.{conn.FromParameter} -> {conn.ToComponentId}.{conn.ToParameter}");
                    }
                }
                
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        private string FormatConversationHistory()
        {
            var sb = new StringBuilder();
            
            var lastMessages = _conversationHistory.Count > MAX_HISTORY_LENGTH
                ? _conversationHistory.Skip(_conversationHistory.Count - MAX_HISTORY_LENGTH)
                : _conversationHistory;
            
            foreach (var message in lastMessages)
            {
                sb.AppendLine($"{message.Timestamp:HH:mm:ss} {message.Role}: {message.Content}");
                if (message.Actions != null && message.Actions.Any())
                {
                    sb.AppendLine("  Actions taken:");
                    foreach (var action in message.Actions)
                    {
                        sb.AppendLine($"    - {action}");
                    }
                }
                sb.AppendLine();
            }
            
            return sb.ToString();
        }

        public void AddMessage(string role, string content, List<string> actions = null)
        {
            _conversationHistory.Add(new ChatMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.Now,
                Actions = actions
            });
            
            // Trim history if needed
            if (_conversationHistory.Count > MAX_HISTORY_LENGTH)
            {
                _conversationHistory.RemoveAt(0);
            }
        }

        public void UpdateComponentState(int componentId, ComponentState state)
        {
            _componentStates[componentId] = state;
        }

        public void RemoveComponentState(int componentId)
        {
            _componentStates.Remove(componentId);
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> Actions { get; set; } // Optional list of actions taken
    }

    public class ComponentState
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public PointF Position { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public List<ConnectionPairing> Connections { get; set; }
    }

    public class GrasshopperState
    {
        public List<ComponentState> Components { get; set; }
    }
} 