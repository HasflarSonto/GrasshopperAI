using System;
using System.IO;
using System.Reflection;
using System.Text;
using GHPT.Utils;
using Grasshopper;

namespace GHPT.Utils
{
    public static class ResourceLoader
    {
        private static readonly string LibrariesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Grasshopper",
            "Libraries"
        );

        private static string GetDocumentPath()
        {
            try
            {
                if (Grasshopper.Instances.ActiveCanvas?.Document?.FilePath != null)
                {
                    return Path.GetDirectoryName(Grasshopper.Instances.ActiveCanvas.Document.FilePath);
                }
            }
            catch (Exception ex)
            {
                LoggingUtil.LogError("Error getting document path", ex);
            }
            return null;
        }

        private static string[] GetPossibleBasePaths()
        {
            var paths = new System.Collections.Generic.List<string>();
            
            // 1. First try the assembly directory (where the GHA is loaded from)
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(assemblyDir))
            {
                paths.Add(assemblyDir);
                paths.Add(Path.Combine(assemblyDir, "Chat_Prompts"));
            }

            // 2. Then try the Libraries directory
            if (!string.IsNullOrEmpty(LibrariesPath))
            {
                paths.Add(LibrariesPath);
                paths.Add(Path.Combine(LibrariesPath, "Chat_Prompts"));
            }

            // 3. Finally try the document path if available
            var docPath = GetDocumentPath();
            if (!string.IsNullOrEmpty(docPath))
            {
                paths.Add(docPath);
                paths.Add(Path.Combine(docPath, "Chat_Prompts"));
            }

            return paths.ToArray();
        }

        private static string TryFindFile(string relativePath)
        {
            // First try in the assembly directory
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(assemblyDir))
            {
                var assemblyPath = Path.Combine(assemblyDir, relativePath);
                if (File.Exists(assemblyPath))
                {
                    LoggingUtil.LogInfo($"Found file in assembly directory: {assemblyPath}");
                    return assemblyPath;
                }
            }

            // Then try other paths
            foreach (var basePath in GetPossibleBasePaths())
            {
                if (string.IsNullOrEmpty(basePath)) continue;
                
                var fullPath = Path.Combine(basePath, relativePath);
                LoggingUtil.LogInfo($"Checking for file at: {fullPath}");
                if (File.Exists(fullPath))
                {
                    LoggingUtil.LogInfo($"Found file at: {fullPath}");
                    return fullPath;
                }
            }
            return null;
        }

        private static string TryLoadEmbeddedResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Try both with and without the GHPT prefix
                var resourcePaths = new[] {
                    $"GHPT.{resourceName.Replace('\\', '.')}",
                    resourceName.Replace('\\', '.')
                };

                foreach (var resourcePath in resourcePaths)
                {
                    LoggingUtil.LogInfo($"Attempting to load embedded resource: {resourcePath}");
                    using (var stream = assembly.GetManifestResourceStream(resourcePath))
                    {
                        if (stream != null)
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                var content = reader.ReadToEnd();
                                LoggingUtil.LogInfo($"Successfully loaded embedded resource: {resourcePath}");
                                return content;
                            }
                        }
                    }
                }
                LoggingUtil.LogInfo($"Could not find embedded resource with paths: {string.Join(", ", resourcePaths)}");
            }
            catch (Exception ex)
            {
                LoggingUtil.LogError($"Error loading embedded resource {resourceName}", ex);
            }
            return null;
        }

        public static string LoadResource(string relativePath, bool required = false)
        {
            try
            {
                // First try loading as embedded resource
                var content = TryLoadEmbeddedResource(relativePath);
                if (content != null)
                {
                    LoggingUtil.LogInfo($"Successfully loaded {relativePath} from embedded resources");
                    return content;
                }

                // If not found in embedded resources, try to find the file in the filesystem
                var filePath = TryFindFile(relativePath);
                if (filePath != null)
                {
                    LoggingUtil.LogInfo($"Successfully loaded {relativePath} from filesystem at {filePath}");
                    return File.ReadAllText(filePath, Encoding.UTF8);
                }

                if (required)
                {
                    var errorMessage = $"Required resource file not found: {relativePath}\n" +
                                     $"Searched in:\n{string.Join("\n", GetPossibleBasePaths())}";
                    throw new FileNotFoundException(errorMessage);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                if (required)
                {
                    throw;
                }
                LoggingUtil.LogError($"Error loading resource {relativePath}", ex);
                return string.Empty;
            }
        }

        public static string LoadComponentDocumentation()
        {
            return ComponentDocumentation.Documentation;
        }

        public static string LoadSimpleExamples()
        {
            return ComponentExamples.SimpleExamples;
        }

        public static string LoadComplexExamples()
        {
            return ComponentExamples.ComplexExamples;
        }

        public static string LoadBestPractices()
        {
            return ComponentBestPractices.BestPractices;
        }

        public static string LoadPromptTemplate(string templateName)
        {
            return templateName switch
            {
                "component_generator.txt" => ComponentGeneratorPrompt.Prompt,
                "component_analyzer.txt" => ComponentAnalyzerPrompt.Prompt,
                _ => LoadResource(Path.Combine("Chat_Prompts", templateName), true)
            };
        }
    }
} 