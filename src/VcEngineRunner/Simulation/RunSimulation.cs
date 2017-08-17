using System;
using System.IO;
using VcEngineAutomation;
using VcEngineAutomation.Actions;
using VcEngineAutomation.Panels;

namespace VcEngineRunner.Simulation
{
    class RunSimulation
    {
        private readonly RunSimulationOptions options;
        private VcEngine vcEngine;

        public RunSimulation(RunSimulationOptions options)
        {
            this.options = options;
        }

        public int Run()
        {
            vcEngine = VcEngine.AttachOrLaunch(options.InstallationPath);
            var outputPanel = vcEngine.OutputPanel;
            var simulationPanel = SimulationPanel.Attach(vcEngine);
            var duration = TimeSpan.Parse(options.Duration);

            var fileNameWithoutExtension = Path.Combine(Path.GetDirectoryName(options.LayoutFile) ?? string.Empty, Path.GetFileNameWithoutExtension(options.LayoutFile));
            vcEngine.LoadLayout(options.LayoutFile);

            outputPanel.Clear();


            new RunSimulationAction(vcEngine).RunFor(duration, 100);

            if (simulationPanel.ElapsedSimulationTime >= duration)
            {
                File.WriteAllText($"{fileNameWithoutExtension}.txt", "Success");
                return 0;
            }

            File.WriteAllText($"{fileNameWithoutExtension}.txt", outputPanel.Text);
            simulationPanel.Reset();
            simulationPanel.SpeedFactor = 50;
            if (File.Exists($"{fileNameWithoutExtension}.avi"))
            {
                File.Delete($"{fileNameWithoutExtension}.avi");
            }
            new RecordVideoAction(vcEngine).RecordFor($"{fileNameWithoutExtension}.avi", duration);

            return 0;
        }
    }
}
