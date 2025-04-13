using System;
using System.Text.Json;
using GHPT.Prompts;

namespace GHPT.Utils
{
    public static class ComponentGeneratorPrompt
    {
        public static string GetPrompt(GHPT.Prompts.AnalysisResult analysis, string documentation, string examples)
        {
            return $@"You are a Grasshopper Expert specializing in generating valid component configurations.
Your task is to convert the analysis outline into a precise Grasshopper component configuration.

Grasshopper Documentation:
{documentation}

Complex Examples:
{examples}

Analysis Outline:
{JsonSerializer.Serialize(analysis)}

Please generate a response in the following JSON format:
{{
    ""Advice"": ""Brief advice or tips for the user"",
    ""Additions"": [
        {{
            ""Name"": ""Component Name"",
            ""Id"": unique_number,
            ""Value"": ""optional_value"",
            ""Position"": {{
                ""X"": x_coordinate,
                ""Y"": y_coordinate
            }}
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
4. Position components logically on the canvas (X increases right, Y increases down)
5. Ensure all connections are valid according to Grasshopper standards
6. Include all necessary components from the analysis outline
7. Set appropriate default values where needed
8. Follow Grasshopper best practices for component layout
9. Reference the complex examples for similar patterns and structures";
        }
    }
} 