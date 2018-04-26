using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Tools;
using System;
using System.Globalization;
using Button = FlaUI.Core.AutomationElements.Button;
using Label = FlaUI.Core.AutomationElements.Label;

namespace VcEngineAutomation.Panels
{
    public class SimulationPanel
    {
        private readonly Lazy<Button> playButton;
        private readonly Lazy<Label> elapsedSimulationTimeLabel;
        private readonly Lazy<Slider> speedFactorSlider;

        private SimulationPanel(VcEngine vcEngine)
        {
            Panel = vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByClassName("SimulationControlView"));
            elapsedSimulationTimeLabel = new Lazy<Label>(() => Panel.FindFirstDescendant(cf => cf.ByAutomationId("ElapsedPlayTime")).AsLabel());
            speedFactorSlider = new Lazy<Slider>(() => Panel.FindFirstDescendant(cf => cf.ByAutomationId("SimulationSpeed")).AsSlider());
            playButton = new Lazy<Button>(() => Panel.FindFirstDescendant(cf => cf.ByAutomationId("SimulationPlayButton")).AsButton());
        }

        /// <summary>
        /// Function to customize if simulation is running. The default check will take more than 1 second to finish.
        /// </summary>
        public Func<bool> IsSimulationRunningFunc;

        public AutomationElement Panel { get; }
        /// <summary>
        /// Waits until simulation ends or the specified timespan has run.
        /// Note that simulation will lag with 1 second as there is no simple way to get if the simulation is running or not
        /// </summary>
        /// <param name="timeSpan">the timespan to let the simulation run</param>
        public void RunFor(TimeSpan timeSpan)
        {
            RunUntil(ElapsedSimulationTime + timeSpan);
        }
        public void RunUntil(TimeSpan timeSpan)
        {
            Retry.While(() => ElapsedSimulationTime, t => t < timeSpan && IsSimulationRunning, TimeSpan.FromMinutes(30), TimeSpan.FromMilliseconds(100));
        }

        /// <summary>
        /// Returns if the simulation is running or not, this property can take up to 1 second to complete
        /// as there is no real way to check if simulation is currently running
        /// </summary>
        public bool IsSimulationRunning
        {
            get
            {
                if (IsSimulationRunningFunc != null)
                {
                    return IsSimulationRunningFunc.Invoke();
                }
                var previousSimulationTime = ElapsedSimulationTime;
                Retry.While(() => ElapsedSimulationTime == previousSimulationTime, 
                    TimeSpan.FromMilliseconds(1005), 
                    TimeSpan.FromMilliseconds(50));
                return ElapsedSimulationTime != previousSimulationTime;
            }
        }

        public double SpeedFactor
        {
            get { return speedFactorSlider.Value.Value; }
            set { speedFactorSlider.Value.Value = value; }
        }

        public void Reset()
        {
            Panel.FindFirstDescendant(cf => cf.ByAutomationId("RevertButton")).AsButton().Invoke();
        }
        public void Start()
        {
            if (!IsSimulationRunning)
            {
                playButton.Value.Invoke();
            }
        }
        public void Stop()
        {
            if (IsSimulationRunning)
            {
                Panel.FindFirstDescendant(cf => cf.ByAutomationId("SimulationPlayButton")).AsButton().Invoke();
            }
        }

        public TimeSpan ElapsedSimulationTime => TimeSpan.ParseExact(elapsedSimulationTimeLabel.Value.Text, "h\\:mm\\:ss", CultureInfo.InvariantCulture);

        public static SimulationPanel Attach(VcEngine vcEngine)
        {
            return new SimulationPanel(vcEngine);
        }
    }
}
