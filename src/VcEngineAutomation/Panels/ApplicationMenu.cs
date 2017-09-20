using System;
using System.Linq;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Input;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Panels
{
    public class ApplicationMenu
    {
        private readonly VcEngine vcEngine;

        public ApplicationMenu(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
        }
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


        public void ClickCancel(TimeSpan? waitTimeSpan=null)
        {
            vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("cancel")).AsButton().Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        public void ClickOk(TimeSpan? waitTimeSpan = null)
        {
            vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ok")).AsButton().Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
    }
}
