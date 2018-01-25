using System;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace VcEngineAutomation.Ribbons
{
    public class Ribbon
    {
        private readonly VcEngine vcEngine;
        private readonly Window mainWindow;

        private readonly Dictionary<string, RibbonTab> tabs = new Dictionary<string, RibbonTab>();

        public Ribbon(VcEngine vcEngine, Window mainWindow, Tab tab)
        {
            this.vcEngine = vcEngine;
            this.mainWindow = mainWindow;
            MainTab = tab;
        }

        public RibbonTab HomeTab => GetTab("VcTabHome");
        public RibbonTab DrawingTab => GetTab("VcTabDrawing");
        public RibbonTab ModelingTab => GetTab("VcTabAuthor");
        public RibbonTab ProgramTab => GetTab("VcTabTeach");
        public RibbonTab HelpTab => GetTab("VcTabHelp");
        public RibbonTab ConnectivityTab => GetTab("VcTabConnections");

        public void ExpandState(ExpandCollapseState state)
        {
            if (!MainTab.Patterns.ExpandCollapse.IsSupported) throw new InvalidOperationException("Ribbon tab does not support expand/collapse pattern");
            var currentState = MainTab.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value;
            if (state == ExpandCollapseState.Expanded && currentState != ExpandCollapseState.Expanded)
            {
                MainTab.Patterns.ExpandCollapse.Pattern.Expand();
            }
            else if (state == ExpandCollapseState.Collapsed && currentState != ExpandCollapseState.Collapsed)
            {
                MainTab.Patterns.ExpandCollapse.Pattern.Collapse();
            }
        }

        public RibbonTab GetTab(string automationId)
        {
            ExpandState(ExpandCollapseState.Expanded);
            RibbonTab tab;
            if (!tabs.TryGetValue(automationId, out tab))
            {
                var tabPage = GetTabPage(automationId);
                if (tabPage == null) throw new InvalidOperationException($"Could not find ribbon tab with automationid ='{automationId}'");
                tab = new RibbonTab(vcEngine, tabPage) { AutomationId = automationId };
                tabs[automationId] = tab;
            }
            return tab;
        }

        public Tab MainTab { get; set; }

        public TabItem GetTabPage(string automationId)
        {
            return MainTab.FindFirstChild(cf => cf.ByAutomationId(automationId))?.AsTabItem();
        }
    }
}
