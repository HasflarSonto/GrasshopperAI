using FuzzySharp;
using FuzzySharp.Extractor;
using GHPT.Prompts;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GHPT.Utils
{
    public static class GraphUtil
    {
        private static readonly string LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GHPT_Debug.log");

        private static void LogToFile(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private static readonly Dictionary<string, string> fuzzyPairs = new()
        {
            { "Extrusion", "Extrude" },
            { "Text Panel", "Panel" }
        };

        private static readonly Dictionary<int, IGH_DocumentObject> CreatedComponents = new();

        public static void InstantiateComponent(GH_Document doc, Addition addition, System.Drawing.PointF pivot)
        {
            try
            {
                LogToFile($"Attempting to create component: {addition.Name} (ID: {addition.Id}) at position ({pivot.X}, {pivot.Y})");
                
                string name = addition.Name;
                IGH_ObjectProxy myProxy = GetObject(name);
                if (myProxy is null)
                {
                    LogToFile($"Failed to find component proxy for: {name}");
                    return;
                }

                Guid myId = myProxy.Guid;
                LogToFile($"Found component proxy with GUID: {myId}");

                if (CreatedComponents.ContainsKey(addition.Id))
                {
                    LogToFile($"Removing existing component with ID: {addition.Id}");
                    CreatedComponents.Remove(addition.Id);
                }

                var emit = Instances.ComponentServer.EmitObject(myId);
                CreatedComponents.Add(addition.Id, emit);
                LogToFile($"Created component instance with ID: {addition.Id}");

                doc.AddObject(emit, false);
                emit.Attributes.Pivot = pivot;
                SetValue(addition, emit);
                LogToFile($"Successfully added component to document and set value");
            }
            catch (Exception ex)
            {
                LogToFile($"Error in InstantiateComponent: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void ConnectComponent(GH_Document doc, ConnectionPairing pairing)
        {
            try
            {
                LogToFile($"Attempting to connect components: {pairing.From.Id} -> {pairing.To.Id}");
                LogToFile($"Connection details - From: {pairing.From.ParameterName}, To: {pairing.To.ParameterName}");
                
                CreatedComponents.TryGetValue(pairing.From.Id, out IGH_DocumentObject componentFrom);
                CreatedComponents.TryGetValue(pairing.To.Id, out IGH_DocumentObject componentTo);

                if (componentFrom == null || componentTo == null)
                {
                    LogToFile($"Failed to find components: From={componentFrom == null}, To={componentTo == null}");
                    return;
                }

                LogToFile($"Found components: From={componentFrom.GetType().Name}, To={componentTo.GetType().Name}");

                IGH_Param fromParam = GetParam(componentFrom, pairing.From, false);
                IGH_Param toParam = GetParam(componentTo, pairing.To, true);

                if (fromParam is null || toParam is null)
                {
                    LogToFile($"Failed to find parameters: From={fromParam == null}, To={toParam == null}");
                    return;
                }

                LogToFile($"Found parameters: From={fromParam.Name}, To={toParam.Name}");

                try
                {
                    toParam.AddSource(fromParam);
                    LogToFile("Added source parameter");

                    toParam.CollectData();
                    LogToFile("Collected data");

                    toParam.ComputeData();
                    LogToFile("Computed data");
                }
                catch (Exception ex)
                {
                    LogToFile($"Error during connection: {ex.Message}\n{ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Error in ConnectComponent: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static IGH_Param GetParam(IGH_DocumentObject docObj, Connection connection, bool isInput)
        {
            var resultParam = docObj switch
            {
                IGH_Param param => param,
                IGH_Component component => GetComponentParam(component, connection, isInput),
                _ => null
            };

            return resultParam;
        }

        private static IGH_Param GetComponentParam(IGH_Component component, Connection connection, bool isInput)
        {
            IList<IGH_Param> _params = (isInput ? component.Params.Input : component.Params.Output)?.ToArray();

            if (_params is null || _params.Count == 0)
            {
                LogToFile($"No parameters found for component {component.GetType().Name} (Input={isInput})");
                return null;
            }

            LogToFile($"Available parameters for {component.GetType().Name}: {string.Join(", ", _params.Select(p => p.Name))}");
            LogToFile($"Looking for parameter: {connection.ParameterName}");

            if (_params.Count() <= 1)
            {
                var param = _params.First();
                LogToFile($"Only one parameter available, using: {param.Name}");
                return param;
            }

            // First try exact match (case insensitive)
            foreach (var _param in _params)
            {
                if (_param.Name.ToLowerInvariant() == connection.ParameterName.ToLowerInvariant())
                {
                    LogToFile($"Found exact parameter match: {_param.Name}");
                    return _param;
                }
            }

            // Then try partial match
            foreach (var _param in _params)
            {
                if (_param.Name.ToLowerInvariant().Contains(connection.ParameterName.ToLowerInvariant()) ||
                    connection.ParameterName.ToLowerInvariant().Contains(_param.Name.ToLowerInvariant()))
                {
                    LogToFile($"Found partial parameter match: {_param.Name}");
                    return _param;
                }
            }

            // Try common parameter name mappings
            var commonMappings = new Dictionary<string, string>
            {
                { "geometry", "geometry" },
                { "curve", "curve" },
                { "curves", "curve" },
                { "point", "point" },
                { "points", "point" },
                { "number", "number" },
                { "value", "number" },
                { "input", "input" },
                { "output", "output" }
            };

            foreach (var _param in _params)
            {
                var paramName = _param.Name.ToLowerInvariant();
                var targetName = connection.ParameterName.ToLowerInvariant();

                if (commonMappings.TryGetValue(targetName, out string mappedName) && 
                    paramName.Contains(mappedName))
                {
                    LogToFile($"Found mapped parameter match: {_param.Name}");
                    return _param;
                }
            }

            LogToFile($"No parameter match found for: {connection.ParameterName}");
            return null;
        }

        private static IGH_ObjectProxy GetObject(string name)
        {
            IGH_ObjectProxy[] results = Array.Empty<IGH_ObjectProxy>();
            double[] resultWeights = new double[] { 0 };
            Instances.ComponentServer.FindObjects(new string[] { name }, 10, ref results, ref resultWeights);

            var myProxies = results.Where(ghpo => ghpo.Kind == GH_ObjectType.CompiledObject);

            var _components = myProxies.OfType<IGH_Component>();
            var _params = myProxies.OfType<IGH_Param>();

            // Prefer Components to Params
            var myProxy = myProxies.First();
            if (_components is not null)
                myProxy = _components.FirstOrDefault() as IGH_ObjectProxy;
            else if (myProxy is not null)
                myProxy = _params.FirstOrDefault() as IGH_ObjectProxy;

            // Sort weird names
            if (fuzzyPairs.ContainsKey(name))
            {
                name = fuzzyPairs[name];
            }

            myProxy = Instances.ComponentServer.FindObjectByName(name, true, true);

            return myProxy;
        }

        private static void SetValue(Addition addition, IGH_DocumentObject ghProxy)
        {
            string lowerCaseName = addition.Name.ToLowerInvariant();

            bool result = ghProxy switch
            {
                GH_NumberSlider slider => SetNumberSliderData(addition, slider),
                GH_Panel panel => SetPanelData(addition, panel),
                Param_Point point => SetPointData(addition, point),
                _ => false
            };

        }

        private static bool SetPointData(Addition addition, Param_Point point)
        {
            try
            {
                if (string.IsNullOrEmpty(addition.Value))
                    return false;

                string[] pointValues = addition.Value.Replace("{", "").Replace("}", "").Split(',');
                double[] pointDoubles = pointValues.Select(p => double.Parse(p)).ToArray();

                point.SetPersistentData(new Rhino.Geometry.Point3d(pointDoubles[0], pointDoubles[1], pointDoubles[2]));
            }
            catch
            {
                point.SetPersistentData(new Rhino.Geometry.Point3d(0, 0, 0));
            }
            finally
            {
                point.CollectData();
                point.ComputeData();
            }

            return true;
        }

        private static bool SetPanelData(Addition addition, GH_Panel panel)
        {
            panel.SetUserText(addition.Value);
            return true;
        }

        private static bool SetNumberSliderData(Addition addition, GH_NumberSlider slider)
        {
            string value = addition.Value;
            if (string.IsNullOrEmpty(value)) return false;
            slider.SetInitCode(value);

            return true;
        }

    }
}
