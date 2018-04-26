using System;
using System.Diagnostics;
using System.Linq;
using VcEngineAutomation;
using VcEngineAutomation.Panels;
using VcEngineAutomation.Windows;

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
            else if (args.Contains("test-ribbon-performance"))
            {
                TestRibbonPerformance();
            }
            else if (args.Contains("test-compproperties"))
            {
                TestComponentProperties();
            }
            else if (args.Contains("test-loadcomponent"))
            {
                TestLoadComponent();
            }
            else if (args.Contains("test-output"))
            {
                TestOutput();
            }
            else if (args.Contains("test-misc"))
            {
                TestMisc();
            }
            else if (args.Contains("test-dialogs"))
            {
                TestVariousDialogs();
            }
            else if (args.Contains("test-simulation"))
            {
                TestSimulation();
            }
            else if (args.Contains("test-ecat"))
            {
                var eng = VcEngine.Attach();
                eng.ECataloguePanel.Search("Conveyor");
                eng.ECataloguePanel.DisplayedComponents[0].Load();
                eng.ECataloguePanel.ClearSearch();
            }
            else if (args.Contains("test-all"))
            {
                var eng = VcEngine.Attach();
                eng.World.Clear();
                eng.World.LoadComponentByVcid("01d7764c-45a2-4c15-bc87-3a5853be14e1");
                eng.PropertiesPanel.FindProperty("Name").AsTextBox().Text = "OtherName";
                eng.PropertiesPanel.Position = new Position(100,200,300);
                eng.PropertiesPanel.SetProperty("Name", "new name");
                eng.MoveFocusTo3DViewPort();
                eng.Camera.FillView();
                var output = eng.OutputPanel.Text;
                eng.World.SelectAll();
                eng.World.CopyAndPasteSelectedComponents();
                eng.World.DeleteSelectedComponent();
                eng.World.Clear();
                eng.ECataloguePanel.Search("Conveyor");
                eng.ECataloguePanel.DisplayedComponents[0].Load();
                eng.ECataloguePanel.ClearSearch();
            }
        }

        private static void TestSimulation()
        {
            var eng = VcEngine.Attach();
            var simulationPanel = SimulationPanel.Attach(eng);
            simulationPanel.Start();
            simulationPanel.RunFor(TimeSpan.FromSeconds(10));
            simulationPanel.Stop();
            simulationPanel.Start();
            simulationPanel.RunUntil(TimeSpan.FromSeconds(15));
            simulationPanel.Stop();
        }

        private static void TestVariousDialogs()
        {
            var eng = VcEngine.Attach();
            var fileDialog = FileDialog.Attach(eng);
            //var vcMsgBox = VcMessageBox.Attach(eng);
            //var msgBox = MessageBox.Attach(eng);
        }

        private static void TestRibbonPerformance()
        {
            var eng = VcEngine.Attach();
            var watch = new Stopwatch();
            watch.Start();
            var count = 200;
            for (int i = 0; i < count; i++)
            {
                eng.Ribbon.HomeTab.FindButtonByName("Clipboard", "Copy");
                eng.Ribbon.HelpTab.FindButtonByName("Social Media", "Youtube");
            }
            watch.Stop();
            var otherWatch = new Stopwatch();
            otherWatch.Start();
            for (int i = 0; i < count; i++)
            {
                eng.Ribbon.HomeTab.FindButtonByAutomationId("VcRibbonClipboard", "Delete");
                eng.Ribbon.HelpTab.FindButtonByAutomationId("VcRibbonSocialMedia", "OpenYoutubePage");
            }
            otherWatch.Stop();
            System.Console.WriteLine($"Old={watch.Elapsed} ({watch.Elapsed.TotalSeconds / count:F2}), New={otherWatch.Elapsed} ({otherWatch.Elapsed.TotalSeconds / count:F2})");
        }

        private static void TestMisc()
        {
            var eng = VcEngine.Attach();
            eng.World.RenameSelectedComponent("RenameSelectedComponent");
        }

        private static void TestOutput()
        {
            var eng = VcEngine.Attach();
            var text = eng.OutputPanel.Text;
        }

        private static void TestLoadComponent()
        {
            var eng = VcEngine.Attach();
            eng.World.LoadComponentByVcid("01d7764c-45a2-4c15-bc87-3a5853be14e1");
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
            var vcEngine = VcEngine.AttachOrLaunch(@"C:\Program Files\Visual Components\Visual Components Professional");
            vcEngine.Ribbon.DrawingTab.Select();
            vcEngine.Application.Close();
        }
    }
}
