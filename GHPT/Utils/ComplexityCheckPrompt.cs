using System;

namespace GHPT.Utils
{
    public static class ComplexityCheckPrompt
    {
        public static string GetPrompt(string userRequest)
        {
            return $@"You are a Grasshopper expert tasked with determining if a modeling request is simple or complex.

A simple request is one that:
- Can be solved with 1-3 components
- Has straightforward inputs and outputs
- Doesn't require complex geometric operations
- Can be solved with basic components like Number Slider, Addition, etc.

A complex request is one that:
- Requires 4 or more components
- Involves complex geometric operations
- Requires multiple steps or transformations
- Needs specialized components or custom logic

User Request: {userRequest}

Respond with exactly one word: 'simple' or 'complex'";
        }
    }
} 