using System;
using System.Collections.Generic;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using VcEngineAutomation.Extensions;

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

        public RibbonTab HomeTab => GetTab("HOME", "VcTabHome");
        public RibbonTab DrawingTab => GetTab("Drawing", "VcTabDrawing");
        public RibbonTab ModelingTab => GetTab("Modeling", "VcTabAuthor");
        public RibbonTab ProgramTab => GetTab("Program", "VcTabTeach");
        public RibbonTab HelpTab => GetTab("Help", "VcTabHelp");
        public RibbonTab ConnectivityTab => GetTab("Connectivity", "VcTabConnections");

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

        public RibbonTab GetTab(string automationId, string automationIdR7)
        {
            automationId = vcEngine.IsR7 ? automationIdR7 : automationId;
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

        public void ClickImageInAboutDialog()
        {
            RibbonTab helpTab = HelpTab;
            helpTab.ClickButton("About", "About");
            mainWindow.WaitWhileBusy();
            Window helpWindow = mainWindow.ModalWindows[0];
            helpWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Image)).LeftClick();
            Helpers.WaitUntilInputIsProcessed();
            helpWindow.Close();
        }
    }
}
