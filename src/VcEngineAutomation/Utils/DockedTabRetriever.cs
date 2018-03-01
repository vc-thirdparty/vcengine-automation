using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Shapes;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Utils
{
    /// <summary>
    /// Class that works with the docked tab parts
    /// </summary>
    public class DockedTabRetriever
    {
        private readonly Window mainWindow;
        private readonly Lazy<AutomationElement> lazyDockManager;

        public DockedTabRetriever(Window mainWindow)
        {
            this.mainWindow = mainWindow;
            lazyDockManager = new Lazy<AutomationElement>(() => mainWindow.FindFirstChild(cf => cf.ByAutomationId("dockManager")));
        }

        public AutomationElement GetPane(string automationId)
        {
            var dockManager = lazyDockManager.Value;
            var tabItem = dockManager.FindFirstDescendant(cf => cf.ByAutomationId($"{automationId}TabItem")).AsTabItem();
            if (tabItem == null)
            {
                string automationIds = string.Join("', '", dockManager.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem)).Select(ae => ae.Properties.AutomationId));
                throw new InvalidOperationException($"Could not find the tab with automationid '{automationId}TabItem' among '{automationIds}'");
            }
            var pane = tabItem.FindFirstChild(cf => cf.ByControlType(ControlType.Custom));
            if (pane == null)
            {
                tabItem.LeftClick();
                Wait.UntilResponsive(tabItem);
                if (!tabItem.IsSelected)
                {
                    tabItem.Select();
                    Wait.UntilResponsive(tabItem);
                }
                pane = tabItem.FindFirstChild(cf => cf.ByControlType(ControlType.Custom));
                if (pane == null) throw new InvalidOperationException($"Could not find the underlying custom pane for tab item '{automationId}TabItem'");
            }
            return pane;
        }

        public AutomationElement GetPane(string paneTitleStart, string customPaneAutomationId)
        {
            var dockManager = lazyDockManager.Value;
            var tabPages = dockManager.
                FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem)).ToArray();
            if (tabPages.Length == 0) throw new InvalidOperationException("Could not find any tab items under the dock manager");
            string tagPageNames = string.Join("', '", tabPages.Select(t => t.Properties.Name.Value));
            var tabPage = tabPages.FirstOrDefault(t => t.Properties.Name.Value.StartsWith(paneTitleStart, StringComparison.OrdinalIgnoreCase))?.AsTabItem();
            if (tabPage == null)
            {
                tabPages = dockManager.
                    FindAllDescendants(cf => cf.ByControlType(ControlType.Custom)).ToArray();
                tabPage = tabPages.FirstOrDefault(t => t.Properties.Name.Value.StartsWith(paneTitleStart, StringComparison.OrdinalIgnoreCase))?.AsTabItem();
                tagPageNames = string.Join("', '", tabPages.Select(t => t.Properties.Name.Value));
            }
            if (tabPage == null) throw new InvalidOperationException($"could not find the tab with '{paneTitleStart}' among '{tagPageNames}'");
            // Left click to force a load of the panel, otherwise the tab item will not have any children
            tabPage.LeftClick();
            if (!tabPage.IsSelected)
            {
                tabPage.Select();
            }
            if (tabPage.Properties.AutomationId.ValueOrDefault == customPaneAutomationId)
            {
                Wait.UntilResponsive(tabPage);
                return tabPage;
            }

            var pane = tabPage.FindFirstDescendant(cf => cf.ByAutomationId(customPaneAutomationId));
            if (pane == null) throw new InvalidOperationException($"Could not find pane with automation id '{customPaneAutomationId}'");
            Wait.UntilResponsive(pane);
            return pane;
            /*AutomationElement[] descendants = mainWindow.FindAllDescendants(cf => cf.ByAutomationId(customPaneAutomationId));
            descendants.Should().NotBeEmpty("no docked panes could be found");
            AutomationElement descendant = descendants.FirstOrDefault(ae => ae.Current.Name.StartsWith(paneTitleStart, StringComparison.OrdinalIgnoreCase)).AsTabItem();
            descendant.Should().NotBeNull($"could not find the tab with '{paneTitleStart}'");
            return descendant;*/
        }

        public void DockPane(string paneTItle)
        {
            Window window = mainWindow.FindAllChildren(cf => cf.ByControlType(ControlType.Window)).FirstOrDefault(w => w.AsWindow().Title == paneTItle)?.AsWindow();
            if (window != null)
            {                
                Point point = window.Properties.BoundingRectangle.Value.North;
                Mouse.MoveTo(point);
                Mouse.MoveBy(50, 10);
                Mouse.LeftDoubleClick();
            }
        }
    }
}