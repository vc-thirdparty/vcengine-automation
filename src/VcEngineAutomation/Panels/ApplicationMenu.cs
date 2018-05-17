using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using System;
using System.Linq;
using VcEngineAutomation.Extensions;
using Menu = FlaUI.Core.AutomationElements.Menu;

namespace VcEngineAutomation.Panels
{
    public class ApplicationMenu
    {
        private readonly VcEngine vcEngine;

        public ApplicationMenu(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
        }
        [Obsolete("Use FindMenuByAutomationId instead")]
        public AutomationElement GetMenu(string header, string subHeader = null)
        {
            vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("MainRibbon")).FindFirstDescendant(cf => cf.ByClassName("ApplicationMenu2010FileTab")).AsButton().Invoke();

            var tabHeaders = vcEngine.MainWindow.FindAllDescendants(cf => cf.ByAutomationId("BackstageNavigationMenuTabHeader"));
            string tabHeaderNames = string.Join(", ", tabHeaders.Select(t => t.Properties.Name.Value));
            var tabHeader = tabHeaders.FirstOrDefault(i => header.Equals(i.Properties.Name, StringComparison.OrdinalIgnoreCase));
            if (tabHeader == null) throw new InvalidOperationException($"expected header '{header}' not found among '{tabHeaderNames}");

            var tabItem = tabHeader.GetParent();
            tabItem.LeftClick();
            if (!string.IsNullOrEmpty(subHeader))
            {
                Mouse.MoveBy(200,0);
                var listItems = tabItem.FindAllDescendants(cf => cf.ByName("VisualComponents.UX.Shared.HeaderModel")).
                    Select(i => Tuple.Create(i, i.FindFirstChild().Properties.Name.Value)).ToArray();
                var listItem = listItems.FirstOrDefault(t => t.Item2.Equals(subHeader, StringComparison.OrdinalIgnoreCase));
                if (listItem == null) throw new InvalidOperationException($"expected sub header '{subHeader}' not found among '{string.Join("', '", listItems.Select(t => t.Item2))}'");
                listItem.Item1.AsTabItem().Select();
            }
            return tabItem;
        }

        public Menu Menu
        {
            get
            {
                var appMenu = vcEngine.MainWindow.FindFirstChild(cf => cf.ByAutomationId("ApplicationMenu"));
                if (appMenu == null)
                {
                    vcEngine.MainWindow.FindFirstChild(cf => cf.ByAutomationId("MainRibbon")).FindFirstChild(cf => cf.ByClassName("ApplicationMenu2010FileTab")).AsButton().Invoke();
                    appMenu = vcEngine.MainWindow.FindFirstChild(cf => cf.ByAutomationId("ApplicationMenu"));
                }
                return appMenu.AsMenu();
            }
        }


        public void Expand()
        {
            var appMenu = Menu;
            if (appMenu.IsOffscreen)
            {
                appMenu.AsMenu().Patterns.ExpandCollapse.Pattern.Expand();
            }
        }

        public void Collapse()
        {
            var appMenu = vcEngine.MainWindow.FindFirstChild(cf => cf.ByAutomationId("ApplicationMenu"));
            if (appMenu != null && !appMenu.IsOffscreen)
            {
                appMenu.AsMenu().Patterns.ExpandCollapse.Pattern.Collapse();
            }
        }

        public AutomationElement FindMenuByAutomationId(string menuAutomationId)
        {
            var menu = Menu.FindFirstChild(cf => cf.ByAutomationId(menuAutomationId));
            if (menu == null) throw new InvalidOperationException($"No menu with automationId='{menuAutomationId}'");
            return menu;
        }

        public AutomationElement FindMenuItem(string menuAutomationId, string menuItemText)
        {
            var menu = FindMenuByAutomationId(menuAutomationId);
            menu.Click();
            Mouse.MoveBy(200, 0);

            var listItem = menu.FindFirstDescendant(cf => cf.ByControlType(ControlType.List)).FindAllChildren()
                .FirstOrDefault(i => i.FindFirstChild().AsLabel().Text == menuItemText)?.AsTabItem();
            if (listItem == null) throw new InvalidOperationException($"No menu item with text='{menuItemText}'");
            listItem.Select();
            return listItem.Parent.Parent;
        }


        public void ClickCancel()
        {
            ClickCancel(null);
        }
        public void ClickCancel(TimeSpan? waitTimeSpan)
        {
            vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("cancel")).AsButton().Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        public void ClickOk()
        {
            ClickOk(null);
        }
        public void ClickOk(TimeSpan? waitTimeSpan)
        {
            vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ok")).AsButton().Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
    }
}
