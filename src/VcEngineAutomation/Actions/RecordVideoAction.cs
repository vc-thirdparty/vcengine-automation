using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using VcEngineAutomation.Panels;
using VcEngineAutomation.Windows;

namespace VcEngineAutomation.Actions
{
    public class RecordVideoAction : IDisposable
    {
        private readonly VcEngine vcEngine;
        private CommandPanel recordPanel;

        public RecordVideoAction(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
        }

        public void RecordFor(string filename, TimeSpan duration)
        {
            StartRecording(filename);
            var simulationPanel = SimulationPanel.Attach(vcEngine);
            simulationPanel.RunFor(duration);
            StopRecording();
        }

        public void StartRecording(string filename)
        {
            var simulationPanel = SimulationPanel.Attach(vcEngine);
            var videoButton = simulationPanel.Panel.FindFirstDescendant("VideoRecordingButton");
            if (videoButton == null) throw new InvalidOperationException("Video recording is not activated. Please activate it in the exe.config.");
            videoButton.AsButton().Invoke();
            recordPanel = vcEngine.GetCommandPanel();
            Button[] buttons = recordPanel.CustomPane.FindAllDescendants(cf => cf.ByControlType(ControlType.Button)).Select(ae => ae.AsButton()).ToArray();
            buttons[0].Invoke();
            FileDialog.Attach(vcEngine).Save(filename, true, false);
        }

        public void StopRecording()
        {
            if (recordPanel != null)
            {
                Button[] buttons = recordPanel.CustomPane.FindAllDescendants(cf => cf.ByControlType(ControlType.Button)).Select(ae => ae.AsButton()).ToArray();
                buttons[1].Invoke();
                recordPanel.Close();
            }
            recordPanel = null;
        }

        public void Dispose()
        {
            if (recordPanel != null)
            {
                StopRecording();
            }
        }
    }
}
