using System;
using VcEngineAutomation.Panels;

namespace VcEngineAutomation.Actions
{
    public class RunSimulationAction
    {
        private readonly SimulationPanel simulationPanel;

        public RunSimulationAction(VcEngine vcEngine)
        {
            simulationPanel = SimulationPanel.Attach(vcEngine);
        }

        public void RunFor(TimeSpan duration, double speedFactor)
        {
            simulationPanel.SpeedFactor = speedFactor;
            RunFor(duration);
        }

        public void RunFor(TimeSpan duration)
        {
            simulationPanel.Reset();
            simulationPanel.Start();
            simulationPanel.RunUntil(duration);
            simulationPanel.Stop();
        }

        public void ContinueFor(TimeSpan duration, double speedFactor = 50)
        {
            simulationPanel.SpeedFactor = speedFactor;
            simulationPanel.Start();
            simulationPanel.RunFor(duration);
            simulationPanel.Stop();
        }
    }
}
