using Grasshopper.Kernel;
using System.Drawing;
using Grasshopper;
using Rhino;

namespace GHPT.UI
{
    public class GHPTAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            RhinoApp.WriteLine("GHPT: Starting PriorityLoad");
            
            try 
            {
                // Register the GHPT tab with its icon
                Grasshopper.Instances.ComponentServer.AddCategoryIcon("GHPT", Resources.Icons.dark_logo_24x24);
                RhinoApp.WriteLine("GHPT: Added category icon");
                
                Grasshopper.Instances.ComponentServer.AddCategorySymbolName("GHPT", 'G');
                RhinoApp.WriteLine("GHPT: Added category symbol");
                
                // Register the Core tab under GHPT
                Grasshopper.Instances.ComponentServer.AddCategoryIcon("GHPT/Core", Resources.Icons.dark_logo_24x24);
                Grasshopper.Instances.ComponentServer.AddCategorySymbolName("GHPT/Core", 'C');
                RhinoApp.WriteLine("GHPT: Added Core category");
                
                // Register the Interface tab under GHPT
                Grasshopper.Instances.ComponentServer.AddCategoryIcon("GHPT/Interface", Resources.Icons.dark_logo_24x24);
                Grasshopper.Instances.ComponentServer.AddCategorySymbolName("GHPT/Interface", 'I');
                RhinoApp.WriteLine("GHPT: Added Interface category");
                
                return GH_LoadingInstruction.Proceed;
            }
            catch (System.Exception ex)
            {
                RhinoApp.WriteLine($"GHPT Error: {ex.Message}");
                RhinoApp.WriteLine($"GHPT Stack Trace: {ex.StackTrace}");
                return GH_LoadingInstruction.Abort;
            }
        }
    }
} 