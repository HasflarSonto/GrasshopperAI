using System;
using System.Threading.Tasks;
using GHPT.Utils;
using GHPT.IO;

namespace GHPT.Builders
{
    public class ComplexityCheckBuilder
    {
        private readonly GPTClient _gptClient;

        public ComplexityCheckBuilder(GPTClient gptClient)
        {
            _gptClient = gptClient;
        }

        public async Task<string> CheckComplexity(string userRequest)
        {
            try
            {
                string prompt = ComplexityCheckPrompt.GetPrompt(userRequest);
                string response = await _gptClient.GetCompletion(prompt);

                // Clean and validate the response
                string cleanedResponse = response.Trim().ToLower();
                if (cleanedResponse != "simple" && cleanedResponse != "complex")
                {
                    throw new Exception($"Invalid complexity response: {response}");
                }

                return cleanedResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking complexity: {ex.Message}", ex);
            }
        }
    }
} 