using System;
using System.Globalization;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Tools;

namespace VcEngineAutomation.Panels
{
    public class SimulationPanel
    {
        private readonly Lazy<Label> elapsedSimulationTimeLabel;
        private readonly Lazy<Slider> speedFactorSlider;

        private SimulationPanel(VcEngine vcEngine)
        {
            Panel = vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByClassName("SimulationControlView"));
            elapsedSimulationTimeLabel = new Lazy<Label>(() => Panel.FindFirstDescendant(cf => cf.ByAutomationId("ElapsedPlayTime")).AsLabel());
            speedFactorSlider = new Lazy<Slider>(() => Panel.FindFirstDescendant(cf => cf.ByAutomationId("SimulationSpeed")).AsSlider());
        }

        public AutomationElement Panel { get; }

        public void WaitFor(TimeSpan timeSpan)
        {
            Retry.While(() => ElapsedSimulationTime, t => t < timeSpan && IsSimulationRunning, TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(1));
        }

        public bool IsSimulationRunning
        {
            get
            {
                var startElapsedTime = ElapsedSimulationTime;
                Thread.Sleep(1001);
                return ElapsedSimulationTime > startElapsedTime;
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
                Panel.FindFirstDescendant(cf => cf.ByAutomationId("SimulationPlayButton")).AsButton().Invoke();
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
