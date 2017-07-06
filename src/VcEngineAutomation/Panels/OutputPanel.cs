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

        public OutputPanel(VcEngine vcEngine, Func<AutomationElement> paneRetriever)
        {
            Pane = paneRetriever().FindFirstDescendant(cf => cf.ByAutomationId("ConsoletextBlock")).AsTextBox();
            this.vcEngine = vcEngine;
        }

        public string Text => Pane.Text;
        public TextBox Pane { get; }

        public void Clear()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Pane.RightClick(true);
                vcEngine.MainWindow.Popup.ContextMenu.MenuItems.First(m => m.Properties.Name == "Clear").Invoke();
                vcEngine.MainWindow.WaitWhileBusy();
            }
        }
    }
}
