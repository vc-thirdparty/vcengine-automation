using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using System;
using System.Linq;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using VcEngineAutomation.Models;

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
        public EcatComponent[] DisplayedComponents => DisplayedItems.Select(ae => new EcatComponent(ae)).ToArray();
        public AutomationElement CollectionsPanel => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("CollectionsPanel"));
        public AutomationElement ItemPanel => panel.Value.FindFirstDescendant(cf => cf.ByAutomationId("ItemPanel"));

        public bool IsUpdating()
        {
            var isOffscreenValueOrDefault = !ItemPanel.FindFirstDescendant(cf => cf.ByAutomationId("progressBar")).Properties.IsOffscreen.ValueOrDefault;
            return isOffscreenValueOrDefault;
        }

        public void WaitUntilUpdated(TimeSpan? timespan = null)
        {
            Retry.While(IsUpdating, timespan ?? TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
        }

        public void WaitUntilPopulated(TimeSpan timeSpan)
        {
            var label = panel.Value.FindFirstDescendant(cf => cf.ByName("No Items."));
            Retry.While(() => !label.IsOffscreen, timeSpan, TimeSpan.FromSeconds(1));
        }

        public void Search(string text, TimeSpan? timespan = null)
        {
            SearchTextBox.Text = text;
            WaitUntilUpdated(timespan);
        }
        public void ClearSearch(TimeSpan? timespan = null)
        {
            Search(string.Empty, timespan);
        }
        public void WaitWhileSearching(TimeSpan? timespan = null)
        {
            WaitUntilUpdated(timespan);
        }
        public EcatComponent[] GetComponents()
        {
            return DisplayedItems.Select(ae => new EcatComponent(ae)).ToArray();
        }
    }
    public class EcatComponent
    {
        public AutomationElement AutomationElement { get; }

        public EcatComponent(AutomationElement automationElement)
        {
            this.AutomationElement = automationElement;
        }

        public void Load()
        {
            AutomationElement.DoubleClick(true);
        }

        public string Name
        {
            get { return AutomationElement.FindFirstChild(cf => cf.ByControlType(ControlType.Text)).AsLabel().Text; }
        }
    }
}
