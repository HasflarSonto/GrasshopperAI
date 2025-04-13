using System;
using System.Threading.Tasks;
using GHPT.Utils;

namespace GHPT.IO
{
    public class GPTClient
    {
        private readonly GPTConfig _config;

        public GPTClient(GPTConfig config)
        {
            _config = config;
        }

        public async Task<string> GetCompletion(string prompt)
        {
            try
            {
                var response = await ClientUtil.Ask(_config, prompt);
                return response.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting completion: {ex.Message}", ex);
            }
        }
    }
} 