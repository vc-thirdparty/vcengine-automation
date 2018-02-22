using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Panels
{
    public class OutputPanel
    {
        private readonly VcEngine vcEngine;
        private readonly Lazy<AutomationElement> panel;
        private readonly Lazy<TextBox> textBox;

        public OutputPanel(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
            panel = new Lazy<AutomationElement>(() => vcEngine.DockManager.FindFirstDescendant(cf => cf.ByAutomationId("VcOutputPanel")));
            textBox = new Lazy<TextBox>(() => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("ConsoletextBlock")).AsTextBox());
        }

        public string Text => TextBox.Text;
        public TextBox TextBox => textBox.Value;
        public AutomationElement Panel => panel.Value;

        public void Clear()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                TextBox.RightClick(true);
                vcEngine.MainWindow.Popup.ContextMenu.Items.First(m => m.Properties.Name == "Clear").Invoke();
                vcEngine.MainWindow.WaitWhileBusy();
            }
        }
    }
}
