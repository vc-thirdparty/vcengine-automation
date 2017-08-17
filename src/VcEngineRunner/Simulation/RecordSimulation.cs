using Fldt.Runner.Utils;
using System;
using System.IO;
using VcEngineAutomation;
using VcEngineAutomation.Actions;
using VcEngineAutomation.Panels;

namespace VcEngineRunner.Simulation
{
    public class RecordSimulation
    {
        private readonly RecordSimulationOptions options;
        private VcEngine vcEngine;

        public RecordSimulation(RecordSimulationOptions options)
        {
            this.options = options;
        }

        public int Run()
        {
            vcEngine = VcEngine.AttachOrLaunch(options.InstallationPath);
            var simulationPanel = SimulationPanel.Attach(vcEngine);
            var duration = TimeSpan.Parse(options.Duration);
            simulationPanel.SpeedFactor = options.SpeedFactor;

            vcEngine.LoadLayout(options.LayoutFile);

            var videoFile = FileUtils.RootFilename(options.OutputFile);
            if (File.Exists(videoFile))
            {
                File.Delete(videoFile);
            }

            switch (options.OutputFileType)
            {
                default:
                    new RecordVideoAction(vcEngine).RecordFor(videoFile, duration);
                    break;
                case RecordType.Experience:
                    new RecordExperienceAction(vcEngine).RecordFor(videoFile, duration);
                    break;
            }

            return 0;
        }
    }
}
