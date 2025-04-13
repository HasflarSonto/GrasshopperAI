using System;

namespace GHPT.Utils
{
    public static class ComponentAnalyzerAndGeneratorPrompt
    {
        public static string GetPrompt(string userRequest, string documentation, string simpleExamples)
        {
            return $@"You are a Grasshopper Expert specializing in analyzing user requests and creating Grasshopper Definitions.
Your task is to analyze the user's request and generate a complete Grasshopper component configuration.

Grasshopper Documentation:
{documentation}

Simple Examples:
{simpleExamples}

User Request: {userRequest}

Please analyze this request and provide a response in the following JSON format:
{{
    ""Advice"": ""Brief advice or tips for the user"",
    ""Additions"": [
        {{
            ""Name"": ""Component Name"",
            ""Id"": unique_number,
            ""Value"": ""optional_value""
        }},
        // ... more components
    ],
    ""Connections"": [
        {{
            ""From"": {{
                ""Id"": source_component_id,
                ""ParameterName"": ""source_parameter""
            }},
            ""To"": {{
                ""Id"": target_component_id,
                ""ParameterName"": ""target_parameter""
            }}
        }},
        // ... more connections
    ]
}}

Remember:
1. Each component must have a unique Id
2. Component names must exactly match Grasshopper documentation
3. Parameter names must exactly match Grasshopper documentation
4. Always prefer the simplest solution that achieves the goal
5. Use specialized components when available (e.g., Box 2Pt for cubes, Circle for circles)
6. Only use complex component chains when necessary
7. Provide clear advice for the user
8. Reference the simple examples for similar patterns
9. If the request is too complex, respond with {{TOO_COMPLEX}}

For simple shapes:
- Use Box 2Pt for cubes/boxes
- Use Circle for circles
- Use Line for straight lines
- Use Point for single points
- Use Number Slider for numeric inputs

For complex shapes:
- Break down into simpler components only when necessary
- Use appropriate transformation components
- Consider using specialized components first";
        }
    }
} 