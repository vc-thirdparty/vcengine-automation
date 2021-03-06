﻿using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using System;
using System.Linq;

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

        public void WaitUntilUpdated()
        {
            WaitUntilUpdated(VcEngine.DefaultTimeout);
        }
        public void WaitUntilUpdated(TimeSpan timespan)
        {
            Retry.While(IsUpdating, timespan, VcEngine.DefaultRetryInternal);
        }

        public void WaitUntilPopulated(TimeSpan timeSpan)
        {
            var label = panel.Value.FindFirstDescendant(cf => cf.ByName("No Items."));
            Retry.While(() => !label.IsOffscreen, timeSpan, TimeSpan.FromSeconds(1));
        }

        public void Search(string text)
        {
            Search(text, TimeSpan.FromSeconds(30));
        }
        public void Search(string text, TimeSpan timespan)
        {
            SearchTextBox.Text = text;
            WaitUntilUpdated(timespan);
        }

        public void ClearSearch()
        {
            ClearSearch(VcEngine.DefaultTimeout);
        }
        public void ClearSearch(TimeSpan timespan)
        {
            Search(string.Empty, timespan);
        }

        public void WaitWhileSearching()
        {
            WaitWhileSearching(TimeSpan.FromSeconds(30));
        }
        public void WaitWhileSearching(TimeSpan timespan)
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
