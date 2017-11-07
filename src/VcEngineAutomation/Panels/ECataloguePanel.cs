using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using System;
using FlaUI.Core.Tools;

namespace VcEngineAutomation.Panels
{
    public class ECataloguePanel
    {
        private readonly VcEngine vcEngine;
        private readonly Lazy<AutomationElement> panel;

        public ECataloguePanel(VcEngine vcEngine, Func<AutomationElement> paneRetriever)
        {
            this.vcEngine = vcEngine;
            panel = new Lazy<AutomationElement>(() => paneRetriever().FindFirstDescendant(cf => cf.ByClassName("ECatalogueView")));
        }

        public TextBox SearchTextBox => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("SearchTextBox")).AsTextBox();
        public AutomationElement[] DisplayedItems => panel.Value.FindAllDescendants(cf => cf.ByClassName("LargeItem"));
        public AutomationElement CollectionsPanel => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("CollectionsPanel"));
        public AutomationElement ItemPanel => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("ItemPanel"));

        public void WaitUntilPopulated(TimeSpan timeSpan)
        {
            var label = panel.Value.FindFirstDescendant(cf => cf.ByName("No Items."));
            Retry.While(() => !label.IsOffscreen, timeSpan, TimeSpan.FromSeconds(1));
        }
    }
}
