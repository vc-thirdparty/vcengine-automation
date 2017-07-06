using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using System;

namespace VcEngineAutomation.Models
{
    public class Visual3DToolbar
    {
        private readonly Lazy<Menu> toolbarMenu;
        private readonly Lazy<AutomationElement> toolbarPanel;

        public Visual3DToolbar(VcEngine vcEngine)
        {
            toolbarPanel = new Lazy<AutomationElement>(() => vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("visual3DToolbar")));
            toolbarMenu = new Lazy<Menu>(() => toolbarPanel.Value.FindFirstChild().AsMenu());
        }

        public void FillView()
        {
            toolbarPanel.Value.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))[0].AsButton().Invoke();
        }

        public void FillOnSelected()
        {
            toolbarPanel.Value.FindAllDescendants(cf => cf.ByControlType(ControlType.Button))[1].AsButton().Invoke();
        }
    }
}
