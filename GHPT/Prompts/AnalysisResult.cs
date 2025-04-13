using System.Collections.Generic;

namespace GHPT.Prompts
{
    public class AnalysisResult
    {
        public string Type { get; set; } // "simple" or "complex"
        public SimpleComponent SimpleComponent { get; set; }
        public ComponentData Component { get; set; }
        public Analysis Analysis { get; set; }
        public Plan Plan { get; set; }
        public ComplexAnalysis ComplexAnalysis { get; set; }
        public string Error { get; set; }
    }

    public class SimpleComponent
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Inputs { get; set; }
        public List<string> Outputs { get; set; }
    }

    public class ComponentData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Parameters { get; set; }
        public List<object> Connections { get; set; }
    }

    public class Analysis
    {
        public string Description { get; set; }
        public List<string> RequiredComponents { get; set; }
        public List<string> RequiredConnections { get; set; }
    }

    public class Plan
    {
        public List<string> Steps { get; set; }
        public List<string> RequiredComponents { get; set; }
    }

    public class ComplexAnalysis
    {
        public string Description { get; set; }
        public List<string> RequiredComponents { get; set; }
        public List<string> RequiredConnections { get; set; }
    }
} 