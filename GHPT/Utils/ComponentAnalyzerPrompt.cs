using System;

namespace GHPT.Utils
{
    public static class ComponentAnalyzerPrompt
    {
        public static string GetPrompt(string userRequest, string examples, string bestPractices)
        {
            return $@"You are a Grasshopper Expert specializing in analyzing complex user requests and creating detailed modeling outlines.
Your task is to analyze the user's request and create a comprehensive outline for how to model it parametrically.

Complex Examples:
{examples}

Best Practices:
{bestPractices}

User Request: {userRequest}

Please analyze this request and provide a response in the following JSON format:
{{
    ""Analysis"": {{
        ""KeyComponents"": [""list of main components needed""],
        ""SimilarExample"": ""name of most similar example"",
        ""RequiredParameters"": [""list of key parameters needed""]
    }},
    ""Outline"": {{
        ""Steps"": [
            {{
                ""Step"": 1,
                ""Description"": ""Description of what to do in this step"",
                ""Components"": [""list of components needed for this step""],
                ""Parameters"": [""list of parameters to set in this step""]
            }},
            // ... more steps
        ],
        ""Connections"": [
            {{
                ""From"": ""source component"",
                ""To"": ""target component"",
                ""Parameter"": ""parameter name""
            }},
            // ... more connections
        ]
    }},
    ""BestPractices"": [
        ""list of relevant best practices to follow""
    ]
}}

Remember:
1. Focus on creating a clear, step-by-step outline
2. Reference the most similar complex example from the provided examples
3. Include all relevant best practices from the provided list
4. Be specific about components and parameters needed
5. If the request is too complex, respond with {{TOO_COMPLEX}}
6. Use the complex examples as a reference for similar patterns and structures";
        }
    }
} 