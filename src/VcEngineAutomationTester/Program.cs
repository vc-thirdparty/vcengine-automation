using System.Linq;
using VcEngineAutomation;
using VcEngineAutomation.Models;
using VcEngineAutomation.Panels;

namespace VcEngineAutomationTester
{
    /// <summary>
    /// Dummy program to quickly test automation's
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("test-launch"))
            {
                Launch();
            }
            else if (args.Contains("test-camera"))
            {
                TestCamera();
            }
            else if (args.Contains("test-compproperties"))
            {
                TestComponentProperties();
            }
        }

        private static void TestComponentProperties()
        {
            // Select a component in the engine
            var eng = VcEngine.Attach();
            var pos = eng.PropertiesPanel.Position;
            pos.X += 100;
            eng.PropertiesPanel.Position = pos;

            var category = eng.PropertiesPanel.GetProperty("Category");
            eng.PropertiesPanel.SetProperty("Category", $"{category}-New");
        }

        private static void TestCamera()
        {
            var eng = VcEngine.Attach();
            eng.Camera.FillView();
        }

        private static void Launch()
        {
            var vcEngine = VcEngine.AttachOrLaunch(@"C:\Program Files\Visual Components\Visual Components Professional 4.0.4");
            vcEngine.Ribbon.DrawingTab.Select();
            vcEngine.Application.Close();
        }
    }
}
