using GHPT.Prompts;
using System;
using System.Linq;

namespace GHPT.Utils
{
    public static class ComponentValidationUtils
    {
        public static bool ValidateComponentJson(PromptData data)
        {
            if (data == null)
                return false;

            // Validate Additions
            if (data.Additions != null)
            {
                foreach (var addition in data.Additions)
                {
                    if (string.IsNullOrEmpty(addition.Name))
                        return false;

                    if (addition.Id <= 0)
                        return false;
                }
            }

            // Validate Connections
            if (data.Connections != null)
            {
                foreach (var connection in data.Connections)
                {
                    if (!connection.IsValid())
                        return false;

                    // Verify that both components exist in Additions
                    if (data.Additions != null)
                    {
                        bool fromExists = data.Additions.Any(a => a.Id == connection.FromComponentId);
                        bool toExists = data.Additions.Any(a => a.Id == connection.ToComponentId);

                        if (!fromExists || !toExists)
                            return false;
                    }
                }
            }

            return true;
        }
    }
} 