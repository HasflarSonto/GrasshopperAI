using System;
using System.Text.Json;
using GHPT.Prompts;

namespace GHPT.Utils
{
    public static class ComponentGeneratorPrompt
    {
        public static string GetPrompt(AnalysisResult analysis, string documentation, string bestPractices)
        {
            // Convert the analysis to a natural language string
            string naturalLanguageOutline = ConvertAnalysisToNaturalLanguage(analysis);

            return $@"You are a Grasshopper Expert specializing in generating valid component configurations.
Your task is to convert the natural language outline into a precise Grasshopper component configuration.

Grasshopper Documentation:
{documentation}

Component Hierarchy and Prerequisites:
1. Base Components (No Inputs Required):
   - Plane: Used as reference for other components
   - Point: Used for positioning and reference

2. Primary Shape Components (Require Base Components):
   - Circle CNR: Requires [Plane, Radius]
   - Rectangle: Requires [Plane, Width, Height]
   - Line: Requires [Point A, Point B]
   - Arc: Requires [Plane, Start Angle, End Angle]

3. Complex Shape Components (Require Primary Shapes):
   - Loft: Requires [Multiple Curves]
   - Extrude: Requires [Curve, Height]
   - Sweep: Requires [Rail Curve, Profile Curve]

4. Transform Components (Require Shapes):
   - Move: Requires [Geometry, Vector]
   - Rotate: Requires [Geometry, Angle]
   - Scale: Requires [Geometry, Factor]

Component Prerequisites:
- Before using Circle CNR:
  1. Must have a Plane component
  2. Must have a Number Slider for radius
  3. Connect Plane to ""P"" input
  4. Connect Number Slider to ""R"" input

- Before using Rectangle:
  1. Must have a Plane component
  2. Must have Number Sliders for width and height
  3. Connect Plane to ""P"" input
  4. Connect Number Sliders to ""W"" and ""H"" inputs

- Before using Loft:
  1. Must have at least 2 curves
  2. Curves must be properly connected to ""C"" input
  3. Ensure curves are in correct order

Best Practices:
{bestPractices}

Natural Language Outline:
{naturalLanguageOutline}

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
6. Include all necessary components from the natural language outline
7. Set appropriate default values where needed
8. Follow Grasshopper best practices for component layout
9. Always check component prerequisites before adding new components
10. Ensure base components (Plane, Point) are created before dependent components
11. Verify all required inputs are connected before using a component

For each step in the natural language outline:
1. Identify the required Grasshopper components
2. Determine the necessary connections between components
3. Set appropriate positions for each component
4. Include any required default values or parameters";
        }

        private static string ConvertAnalysisToNaturalLanguage(AnalysisResult analysis)
        {
            if (analysis == null)
                return "No analysis provided";

            var outline = new System.Text.StringBuilder();

            // Add description from analysis
            if (!string.IsNullOrEmpty(analysis.Analysis?.Description))
            {
                outline.AppendLine(analysis.Analysis.Description);
                outline.AppendLine();
            }

            // Add steps from plan
            if (analysis.Plan?.Steps != null && analysis.Plan.Steps.Count > 0)
            {
                outline.AppendLine("Steps to create the model:");
                foreach (var step in analysis.Plan.Steps)
                {
                    outline.AppendLine($"- {step}");
                }
            }

            // Add required components
            if (analysis.Analysis?.RequiredComponents != null && analysis.Analysis.RequiredComponents.Count > 0)
            {
                outline.AppendLine();
                outline.AppendLine("Required components:");
                foreach (var component in analysis.Analysis.RequiredComponents)
                {
                    outline.AppendLine($"- {component}");
                }
            }

            // Add required connections
            if (analysis.Analysis?.RequiredConnections != null && analysis.Analysis.RequiredConnections.Count > 0)
            {
                outline.AppendLine();
                outline.AppendLine("Required connections:");
                foreach (var connection in analysis.Analysis.RequiredConnections)
                {
                    outline.AppendLine($"- {connection}");
                }
            }

            return outline.ToString();
        }
    }
} 