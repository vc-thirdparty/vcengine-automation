using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using System;
using System.Linq;
using VcEngineAutomation.Extensions;
using VcEngineAutomation.Panels;
using Button = FlaUI.Core.AutomationElements.Button;
using CheckBox = FlaUI.Core.AutomationElements.CheckBox;
using ComboBox = FlaUI.Core.AutomationElements.ComboBox;
using Menu = FlaUI.Core.AutomationElements.Menu;
using MenuItem = FlaUI.Core.AutomationElements.MenuItem;
using TextBox = FlaUI.Core.AutomationElements.TextBox;

namespace VcEngineAutomation.Ribbons
{
    public class RibbonTab
    {
        private readonly VcEngine vcEngine;
        private AutomationElement[] cachedRibbonGroups;

        public RibbonTab(VcEngine vcEngine, TabItem tabItem)
        {
            this.vcEngine = vcEngine;
            TabPage = tabItem;
        }

        public TabItem TabPage { get; }

        public string AutomationId { get; set; }

        public bool IsSelected => TabPage.IsSelected;
        public void Select()
        {
            /*if (vcEngine.IsR7)
            {
                if (!TabPage.IsSelected)
                {
                    Retry.WhileException(() => TabPage.Select(), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100));
                    Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                    if (!TabPage.IsSelected) throw new InvalidOperationException($"Ribbon tab ({AutomationId}) was not selected");
                }
            }
            else*/
            {
                Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                //Mouse.MoveTo(TabPage.GetCenter());
                if (!TabPage.IsSelected)
                {
                    Mouse.LeftClick(TabPage.GetCenter());
                    Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                    if (!TabPage.IsSelected)
                    {
                        if (this != vcEngine.Ribbon.HomeTab)
                        {
                            vcEngine.Ribbon.HomeTab.Select();
                            Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                        }
                        Mouse.LeftClick(TabPage.GetCenter());
                        Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                        if (!TabPage.IsSelected) throw new InvalidOperationException($"Ribbon tab ({AutomationId}) was not selected");
                    }
                }
            }
        }
        
        public AutomationElement[] Groups()
        {
            Select();
            if (cachedRibbonGroups == null)
            {
                cachedRibbonGroups = TabPage.FindAllChildren(cf => cf.ByClassName("RibbonGroup"));
                if (!cachedRibbonGroups.Any()) throw new InvalidOperationException($"Ribbon tab ({AutomationId}) does not contain ribbon group '{TabPage.Properties.AutomationId.Value}'");
            }
            return cachedRibbonGroups;
        }
        //[Obsolete("Use FindAutomationElement instead, as it will replace this method in future")]
        public AutomationElement Group(int groupIndex)
        {
            AutomationElement[] groups = Groups();
            if (groups.Length <= groupIndex) throw new InvalidOperationException("No ribbon ribbonGroup at specified index");
            return groups.ElementAt(groupIndex);
        }
        //[Obsolete("Use FindAutomationElement instead, as it will replace this method in future")]
        public AutomationElement Group(string name)
        {
            AutomationElement[] groups = Groups();
            string groupNames = string.Join("', '", groups.Select(i => i.Properties.Name.Value));
            AutomationElement ribbonGroup = groups.FirstOrDefault(g => string.Equals(g.Properties.Name, name, StringComparison.OrdinalIgnoreCase));
            if (ribbonGroup == null) throw new InvalidOperationException($"No ribbon group found named '{name}', available choices are '{groupNames}'");
            return ribbonGroup;
        }
        //[Obsolete("Use FindAutomationElement instead, as it will replace this method in future")]
        private AutomationElement[] GetGroupItems<T>(AutomationElement ribbonGroup) where T : AutomationElement
        {
            if (typeof(T) == typeof(Button))
            {
                return ribbonGroup.FindAllChildren(cf => cf.ByControlType(ControlType.Button));
            }
            if (typeof(T) == typeof(CheckBox))
            {
                return ribbonGroup.FindAllChildren(cf => cf.ByControlType(ControlType.CheckBox));
            }
            throw new InvalidOperationException($"Unknown type '{typeof(T)}'");
        }
        //[Obsolete("Use InvokeButtonByAutomationId, as it will replace this method in future")]
        public void ClickButton(string groupName, string buttonName, TimeSpan? waitTimeSpan=null)
        {
            var button = FindButtonByName(groupName, buttonName);
            if (!button.IsEnabled) throw new InvalidOperationException($"Ribbon button '{buttonName}' is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
        //[Obsolete("Use FindButtonByAutomationId, as it will replace this method in future")]
        public Button FindButtonByName(string groupName, string buttonName)
        {
            vcEngine.CheckForCrash();
            AutomationElement[] buttons = GetGroupItems<Button>(Group(groupName));
            string itemNames = string.Join("', '", buttons.Select(i => i.Properties.Name.Value));
            Button button = buttons.FirstOrDefault(b => string.Equals(b.Properties.Name, buttonName, StringComparison.OrdinalIgnoreCase))?.AsButton();
            if (button == null) throw new InvalidOperationException($"No ribbon button found named '{buttonName}', available choices are '{itemNames}'");
            return button;
        }

        private void Invoke(AutomationElement button)
        {
            var className = button.Properties.ClassName.Value;
            if (className == "ToggleButtonTool")
            {
                Wait.UntilResponsive(button, TimeSpan.FromSeconds(5));
                if (button.Patterns.Toggle.Pattern.ToggleState.Value != ToggleState.On)
                {
                    button.Click(true);
                    Wait.UntilResponsive(button, TimeSpan.FromSeconds(5));
                }
            }
            else if (className == "ButtonTool")
            {
                button.Patterns.Invoke.Pattern.Invoke();
            }
            else
            {
                throw new InvalidOperationException($"Neither Toggle or Invoke is supported for button ({button.ToDebugString()})");
            }
        }

        //[Obsolete("Use InvokeButtonByAutomationId, as it will replace this method in future")]
        public void ClickButton(int groupIndex, int buttonIndex, TimeSpan? waitTimeSpan = null)
        {
            vcEngine.CheckForCrash();
            AutomationElement[] buttons = GetGroupItems<Button>(Group(groupIndex));
            if (buttons.Length <= buttonIndex) throw new InvalidOperationException($"No ribbon button found at index {buttonIndex}, number of buttons are {buttons.Length}");
            Button button = buttons[buttonIndex].AsButton();
            if (!button.IsEnabled) throw new InvalidOperationException($"Ribbon button at index {buttonIndex} is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
        //[Obsolete("Use InvokeButtonByAutomationId, as it will replace this method in future")]
        public void ClickButton(string groupName, int buttonIndex, TimeSpan? waitTimeSpan = null)
        {
            vcEngine.CheckForCrash();
            AutomationElement[] buttons = GetGroupItems<Button>(Group(groupName));
            if (buttons.Length <= buttonIndex) throw new InvalidOperationException($"No ribbon button found at index {buttonIndex}, number of buttons are {buttons.Length}");
            Button button = buttons[buttonIndex].AsButton();
            if (!button.IsEnabled) throw new InvalidOperationException($"Ribbon button at index {buttonIndex} is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        //[Obsolete("Use InvokeCommandPanelButtonByAutomationId, as it will replace this method in future")]
        public CommandPanel ClickCommandPanelButton(string groupName, string buttonName, string startOfTitle=null, TimeSpan? waitTimeSpan = null)
        {
            ClickButton(groupName, buttonName, waitTimeSpan);
            return vcEngine.GetCommandPanel();
        }

        //[Obsolete("Use InvokeCommandPanelButtonByAutomationId, as it will replace this method in future")]
        public CommandPanel ClickCommandPanelButton(string groupName, int buttonIndex, string startOfTitle = null, TimeSpan? waitTimeSpan = null)
        {
            ClickButton(groupName, buttonIndex, waitTimeSpan);
            return vcEngine.GetCommandPanel();
        }

        public void SelectBigDropdownItem(string groupName, int index, int menuIndex, TimeSpan? waitTimeSpan=null)
        {
            Menu menu = Group(groupName).FindAllChildren().ElementAtOrDefault(index)?.AsMenu();
            if (menu == null) throw new InvalidOperationException("No ribbon menu at specified index");
            if (!menu.IsEnabled) throw new InvalidOperationException("Ribbon menu item is not enabled");

            Window popup = vcEngine.MainWindow.GetCreatedWindowsForAction(() => menu.AsComboBox().Expand()).First();
            AutomationElement[] elements = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.MenuItem));
            if (elements.Length < menuIndex) throw new InvalidOperationException($"no menu item found at index '{menuIndex}'");
            MenuItem menuItem = elements[menuIndex].AsMenuItem();
            menuItem?.Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        public void SelectBigDropdownItem(string groupName, int index, params string[] text)
        {
            SelectBigDropdownItem(groupName, index, null, text);
        }

        public void SelectBigDropdownItem(string groupName, int index, TimeSpan? waitTimeSpan, params string[] text)
        {
            Menu menu = Group(groupName).FindAllChildren().ElementAtOrDefault(index)?.AsMenu();
            if (menu == null) throw new InvalidOperationException("No ribbon menu at specified index");
            if (!menu.IsEnabled) throw new InvalidOperationException("Ribbon menu item is not enabled");
            if (menu.Properties.Name == text.Last()) return;

            Window popup = vcEngine.MainWindow.GetCreatedWindowsForAction(() => menu.AsComboBox().Expand()).First();
            MenuItem[] menuItems = popup.FindAllChildren(cf => cf.ByControlType(ControlType.MenuItem)).Select(ae => ae.AsMenuItem()).ToArray();
            MenuItem menuItem = null;
            foreach (string s in text)
            {
                if (menuItems.Length == 0) throw new InvalidOperationException("No menu items found");
                var menuItemsText = menuItems.Select(m => Tuple.Create(m, m.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsLabel().Text)).ToArray();
                menuItem = menuItemsText.Where(t => s.Equals(t.Item2, StringComparison.OrdinalIgnoreCase)).Select(t => t.Item1).FirstOrDefault();
                if (menuItem == null) throw new InvalidOperationException($"No ribbon menu item found named '{s}' among '{string.Join("', '", menuItemsText.Select(t => t.Item2))}'");
                if (s != text.Last())
                {
                    menuItems = menuItem.Items.ToArray();
                }
            }
            menuItem?.Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        //[Obsolete("Use SelectDropdownItemByAutomationId, as it will replace this method in future")]
        public void SelectDropdownItem(string groupName, string itemName, string text, TimeSpan? waitTimeSpan=null)
        {
            GetDropdownMenuItem(FindMenu(groupName, itemName), text).Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
        //[Obsolete("Use SelectDropdownItemByAutomationId, as it will replace this method in future")]
        public void SelectDropdownItem(string groupName, int itemIndex, string text, TimeSpan? waitTimeSpan = null)
        {
            GetDropdownMenuItem(FindMenu(groupName, itemIndex), text).Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
        
        //[Obsolete("Use FindDropdownMenuItemByAutomationId, as it will replace this method in future")]
        public void ToggleDropdownItem(string groupName, string itemName, string text, ToggleState toggleState, TimeSpan? waitTimeSpan = null)
        {
            Menu menu = FindMenu(groupName, itemName);
            var togglePattern = GetDropdownMenuItem(menu, text).Patterns.Toggle.Pattern;
            if (togglePattern.ToggleState != toggleState)
            {
                togglePattern.Toggle();
            }
            vcEngine.WaitWhileBusy(waitTimeSpan);
            if (menu.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value == ExpandCollapseState.Expanded)
            {
                // Disable as it throws an exception when collapsing
                //menu.Patterns.ExpandCollapse.Pattern.Collapse();
                menu.Click();
            }
        }
        private MenuItem GetDropdownMenuItem(Menu menu, string text)
        {
            Window popup = vcEngine.MainWindow.GetCreatedWindowsForAction(() => menu.AsComboBox().Expand()).First();
            MenuItem[] menuItems = popup.FindAllChildren(cf => cf.ByControlType(ControlType.MenuItem)).Select(ae => ae.AsMenuItem()).ToArray();
            string menuItemNames = string.Join("', '", menuItems.Select(m => m.FindFirstChild().AsLabel().Text).ToArray());
            MenuItem menuItem = menuItems.FirstOrDefault(m => m.FindFirstChild().AsLabel().Text.Equals(text, StringComparison.OrdinalIgnoreCase));
            if (menuItem == null) throw new InvalidOperationException($"No ribbon menu item found named '{text}', available choices are '{menuItemNames}'");
            return menuItem;
        }

        //[Obsolete("Use FindComboBox instead")]
        public void SelectComboboxItem(string groupName, int itemIndex, string text)
        {
            AutomationElement item = Group(groupName).FindAllChildren().ElementAtOrDefault(itemIndex);
            if (item == null) throw new InvalidOperationException($"No ribbon combobox found at specified index {itemIndex}");

            ComboBox combobox = item.FindFirstDescendant(cf => cf.ByControlType(ControlType.ComboBox)).AsComboBox();
            var comboboxItems = combobox?.Items.Select(i => Tuple.Create(i, i.FindFirstChild().AsLabel().Text)).ToArray();
            var comboboxItem = comboboxItems?.FirstOrDefault(t => t.Item2.Equals(text, StringComparison.OrdinalIgnoreCase));

            string itemNames = string.Join("', '", comboboxItems.Select(t => t.Item2).ToArray());
            if (comboboxItem == null) throw new InvalidOperationException($"No list item found named '{text}', available choices are '{itemNames}'");
            if (!comboboxItem.Item1.IsSelected)
            {
                comboboxItem.Item1.Select();
                vcEngine.WaitWhileBusy();
            }
        }

        [Obsolete("Use FindTextBoxByAutomationId, as it will replace this method in future")]
        public TextBox FindTextBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllChildren(cf => cf.ByControlType(ControlType.Edit)).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No text box found in group '{groupName}' at index {index}");
            Wait.UntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsTextBox();
        }

        [Obsolete("Use EnterTextBoxByAutomationId, as it will replace this method in future")]
        public void EnterIntoTexBox(string groupName, int index, string text)
        {
            TextBox textbox = FindTextBox(groupName, index);
            if (!textbox.IsEnabled) throw new InvalidOperationException($"Text box {groupName} at index {index} was not enabled");
            Wait.UntilResponsive(textbox, TimeSpan.FromSeconds(5));
            textbox.Text = text;
        }
        [Obsolete("Use FindComboBoxByAutomationId, as it will replace this method in future")]
        public ComboBox FindComboBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllDescendants(cf => cf.ByClassName("ComboBox").And(cf.ByAutomationId("PART_FocusSite"))).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No text combo box found in group '{groupName}' at index {index}");
            Wait.UntilResponsive(element, TimeSpan.FromSeconds(5));
            element.Click();
            Wait.UntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsComboBox();
        }
        [Obsolete("Use FindCheckBoxByAutomationId, as it will replace this method in future")]
        public CheckBox FindCheckBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllChildren(cf => cf.ByClassName("ToggleButtonTool")).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No check box found in group '{groupName}' at index {index}");
            Wait.UntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsCheckBox();
        }
        [Obsolete("Use FindCheckBoxByAutomationId, as it will replace this method in future")]
        public CheckBox FindCheckBox(string groupName, string labelName)
        {
            AutomationElement element = Group(groupName).FindFirstDescendant(cf => cf.ByClassName("ToggleButtonTool").And(cf.ByName(labelName)));
            if (element == null) throw new InvalidOperationException($"No check box found in group '{groupName}' with name {labelName}");
            Wait.UntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsCheckBox();
        }

        [Obsolete("Use FindMenuByAutomationId, as it will replace this method in future")]
        public Menu FindMenu(string groupName, string itemName)
        {
            AutomationElement[] menues = Group(groupName).FindAllChildren();
            string itemNames = string.Join("', '", menues.Select(i => i.Properties.Name.Value));
            Menu menu = menues.FirstOrDefault(b => string.Equals(b.Properties.Name, itemName, StringComparison.OrdinalIgnoreCase))?.AsMenu();
            if (menu == null) throw new InvalidOperationException($"No ribbon menu found named '{itemName}', available choices are '{itemNames}'");
            return menu;
        }
        [Obsolete("Use FindMenuByAutomationId, as it will replace this method in future")]
        public Menu FindMenu(string groupName, int itemIndex)
        {
            var elements = Group(groupName).FindAllChildren();
            if (elements.Length <= itemIndex) throw new InvalidOperationException($"No ribbon menu found at the specified index {itemIndex}");
            return elements[itemIndex].AsMenu();
        }

        [Obsolete("Use FindDropdownMenuItemByAutomationId, as it will replace this method in future")]
        public MenuItem FindDropdownMenuItem(string groupName, string itemName, string text)
        {
            return GetDropdownMenuItem(FindMenu(groupName, itemName), text);
        }
        
        public AutomationElement FindAutomationElement(string groupAutomationId, string itemAutomationId)
        {
            vcEngine.CheckForCrash();
            Select();
            /*Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
            if (!Retry.WhileException(() => TabPage.IsSelected, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200)))
            {
                Retry.WhileException(() => {
                    Mouse.LeftClick(TabPage.GetCenter());
            //        Wait.UntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                }, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200));

                //Retry.WhileException(() => TabPage.Select(), TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(200));
            }*/

            /*var toolBar = TabPage.FindAllChildren(cf => cf.ByControlType(ControlType.ToolBar))
                .FirstOrDefault(t => t.FindChildAt(0).AutomationId.StartsWith($"{TabPage.AutomationId}{groupAutomationId}"));
            var automationElement = toolBar.FindFirstChild(cf => cf.ByAutomationId($"{TabPage.AutomationId}{groupAutomationId}{itemAutomationId}"));*/
            var automationElement = TabPage.FindFirstDescendant(cf => cf.ByAutomationId($"{TabPage.AutomationId}{groupAutomationId}{itemAutomationId}"));
            if (automationElement != null)
            {
                Wait.UntilResponsive(automationElement, TimeSpan.FromSeconds(5));
            }
            return automationElement;
        }
        private AutomationElement FindAutomationElementImpl(string groupAutomationId, string automationId)
        {
            var automationElement = FindAutomationElement(groupAutomationId, automationId);
            if (automationElement == null) throw new InvalidOperationException($"No control found with AutomationId={automationId}");
            return automationElement;
        }
        
        public TextBox FindTextBoxByAutomationId(string groupAutomationId, string automationId)
        {
            return FindAutomationElementImpl(groupAutomationId, automationId).AsTextBox();
        }
        
        public void EnterIntoTexBoxByAutomationId(string groupAutomationId, string automationId, string text)
        {
            TextBox textbox = FindTextBoxByAutomationId(groupAutomationId, automationId);
            if (!textbox.IsEnabled) throw new InvalidOperationException($"Text box {automationId} was not enabled");
            Wait.UntilResponsive(textbox, TimeSpan.FromSeconds(5));
            textbox.Text = text;
            Keyboard.Type(VirtualKeyShort.ENTER);
        }

        public CheckBox FindCheckBoxByAutomationId(string groupAutomationId, string automationId)
        {
            return FindAutomationElementImpl(groupAutomationId, automationId).AsCheckBox();
        }

        public Button FindButtonByAutomationId(string groupAutomationId, string automationId)
        {
            return FindAutomationElementImpl(groupAutomationId, automationId).AsButton();
        }

        public ComboBox FindComboBoxByAutomationId(string groupAutomationId, string automationId)
        {
            var comboBox = FindAutomationElementImpl(groupAutomationId, automationId).AsComboBox();
            Wait.UntilResponsive(comboBox, TimeSpan.FromSeconds(5));
            comboBox.Click();
            Wait.UntilResponsive(comboBox, TimeSpan.FromSeconds(5));
            return comboBox;
        }

        public Menu FindMenuByAutomationId(string groupAutomationId, string automationId)
        {
            return FindAutomationElement(groupAutomationId, automationId).AsMenu();
        }

        public void InvokeButtonByAutomationId(string groupAutomationId, string automationId, TimeSpan? waitTimeSpan = null)
        {
            var button = FindAutomationElementImpl(groupAutomationId, automationId).AsButton();
            if (!button.IsEnabled) throw new InvalidOperationException($"Ribbon button with automationId='{automationId}' is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }
        
        public CommandPanel InvokeCommandPanelButtonByAutomationId(string groupAutomationId, string automationId, TimeSpan? waitTimeSpan = null)
        {
            InvokeButtonByAutomationId(groupAutomationId, automationId, waitTimeSpan);
            return vcEngine.GetCommandPanel();
        }

        public MenuItem FindDropdownMenuItemByAutomationId(string groupAutomationId, string menuAutomationId, string menuItemAutomationId)
        {
            var menu = FindAutomationElementImpl(groupAutomationId, menuAutomationId).AsMenu();
            return FindDropdownMenuItemByAutomationId(menu, menuItemAutomationId);
        }
        private MenuItem FindDropdownMenuItemByAutomationId(Menu menu, string automationId)
        {
            Window popup = vcEngine.MainWindow.GetCreatedWindowsForAction(() => menu.AsComboBox().Expand()).First();
            var menuItem = popup.FindFirstDescendant(cf => cf.ByAutomationId(automationId))?.AsMenuItem();
            if (menuItem == null) throw new InvalidOperationException($"No ribbon menu item with automationId='{automationId}'");
            return menuItem;
        }
        public void ToggleDropdownItemByAutomationId(string groupAutomationId, string menuAutomationId, string menuItemAutiomationId, ToggleState toggleState, TimeSpan? waitTimeSpan = null)
        {
            var menu = FindAutomationElementImpl(groupAutomationId, menuAutomationId).AsMenu();
            var togglePattern = FindDropdownMenuItemByAutomationId(menu, menuItemAutiomationId).Patterns.Toggle.Pattern;
            if (togglePattern.ToggleState != toggleState)
            {
                togglePattern.Toggle();
                vcEngine.WaitWhileBusy(waitTimeSpan);
            }
            
            if (menu.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value == ExpandCollapseState.Expanded)
            {
                // Disable as it throws an exception when collapsing
                Retry.WhileException(() => menu.Patterns.ExpandCollapse.Pattern.Collapse(), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
                //menu.Click();
            }
        }
    }
}