using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using VcEngineAutomation.Extensions;
using VcEngineAutomation.Panels;

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
                    Helpers.WaitUntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                    if (!TabPage.IsSelected) throw new InvalidOperationException($"Ribbon tab ({AutomationId}) was not selected");
                }
            }
            else*/
            {
                Helpers.WaitUntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                //Mouse.MoveTo(TabPage.GetCenter());
                if (!TabPage.IsSelected)
                {
                    Mouse.LeftClick(TabPage.GetCenter());
                    Helpers.WaitUntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                    if (!TabPage.IsSelected)
                    {
                        if (this != vcEngine.Ribbon.HomeTab)
                        {
                            vcEngine.Ribbon.HomeTab.Select();
                            Helpers.WaitUntilResponsive(TabPage, TimeSpan.FromSeconds(5));
                        }
                        Mouse.LeftClick(TabPage.GetCenter());
                        Helpers.WaitUntilResponsive(TabPage, TimeSpan.FromSeconds(5));
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
        public AutomationElement Group(int groupIndex)
        {
            AutomationElement[] groups = Groups();
            if (groups.Length <= groupIndex) throw new InvalidOperationException("No ribbon ribbonGroup at specified index");
            return groups.ElementAt(groupIndex);
        }
        public AutomationElement Group(string name)
        {
            AutomationElement[] groups = Groups();
            string groupNames = string.Join("', '", groups.Select(i => i.Properties.Name.Value));
            AutomationElement ribbonGroup = groups.FirstOrDefault(g => string.Equals(g.Properties.Name, name, StringComparison.OrdinalIgnoreCase));
            if (ribbonGroup == null) throw new InvalidOperationException($"No ribbon group found named '{name}', available choices are '{groupNames}'");
            return ribbonGroup;
        }
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
        public void ClickButton(string groupName, string buttonName)
        {
            var button = FindButtonByName(groupName, buttonName);
            if (!button.IsEnabled()) throw new InvalidOperationException($"Ribbon button '{buttonName}' is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy();
        }

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
                Helpers.WaitUntilResponsive(button, TimeSpan.FromSeconds(5));
                if (button.Patterns.Toggle.Pattern.ToggleState.Value != ToggleState.On)
                {
                    button.Click(true);
                    Helpers.WaitUntilResponsive(button, TimeSpan.FromSeconds(5));
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

        public void ClickButton(int groupIndex, int buttonIndex)
        {
            vcEngine.CheckForCrash();
            AutomationElement[] buttons = GetGroupItems<Button>(Group(groupIndex));
            if (buttons.Length <= buttonIndex) throw new InvalidOperationException($"No ribbon button found at index {buttonIndex}, number of buttons are {buttons.Length}");
            Button button = buttons[buttonIndex].AsButton();
            if (!button.Properties.IsEnabled) throw new InvalidOperationException($"Ribbon button at index {buttonIndex} is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy();
        }
        public void ClickButton(string groupName, int buttonIndex)
        {
            vcEngine.CheckForCrash();
            AutomationElement[] buttons = GetGroupItems<Button>(Group(groupName));
            if (buttons.Length <= buttonIndex) throw new InvalidOperationException($"No ribbon button found at index {buttonIndex}, number of buttons are {buttons.Length}");
            Button button = buttons[buttonIndex].AsButton();
            if (!button.Properties.IsEnabled) throw new InvalidOperationException($"Ribbon button at index {buttonIndex} is not enabled");
            Invoke(button);
            vcEngine.WaitWhileBusy();
        }


        public CommandPanel ClickCommandPanelButton(string groupName, string buttonName, string startOfTitle=null)
        {
            ClickButton(groupName, buttonName);
            return GetCommandPanel(startOfTitle);
        }

        public CommandPanel ClickCommandPanelButton(string groupName, int buttonIndex, string startOfTitle = null)
        {
            ClickButton(groupName, buttonIndex);
            return GetCommandPanel(startOfTitle);
        }

        private CommandPanel GetCommandPanel(string startOfTitle)
        {
            if (startOfTitle == null)
            {
                return vcEngine.GetCommandPanel();
            }
            else
            {
                return vcEngine.GetCommandPanel(startOfTitle);
            }
        }


        public void SelectBigDropdownItem(string groupName, int index, int menuIndex)
        {
            Menu menu = Group(groupName).FindAllChildren().ElementAtOrDefault(index)?.AsMenu();
            if (menu == null) throw new InvalidOperationException("No ribbon menu at specified index");
            if (!menu.Properties.IsEnabled) throw new InvalidOperationException("Ribbon menu item is not enabled");

            Window popup = vcEngine.MainWindow.GetCreatedWindowsForAction(() => menu.AsComboBox().Expand()).First();
            AutomationElement[] elements = popup.FindAllDescendants(cf => cf.ByControlType(ControlType.MenuItem));
            if (elements.Length < menuIndex) throw new InvalidOperationException($"no menu item found at index '{menuIndex}'");
            MenuItem menuItem = elements[menuIndex].AsMenuItem();
            menuItem?.Invoke();
            vcEngine.WaitWhileBusy();
        }
        public void SelectBigDropdownItem(string groupName, int index, params string[] text)
        {
            Menu menu = Group(groupName).FindAllChildren().ElementAtOrDefault(index)?.AsMenu();
            if (menu == null) throw new InvalidOperationException("No ribbon menu at specified index");
            if (!menu.Properties.IsEnabled) throw new InvalidOperationException("Ribbon menu item is not enabled");
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
                    menuItems = menuItem.SubMenuItems.ToArray();
                }
            }
            menuItem?.Invoke();
            vcEngine.WaitWhileBusy();
        }

        public void SelectDropdownItem(string groupName, string itemName, string text)
        {
            AutomationElement[] menues = Group(groupName).FindAllChildren();
            string itemNames = string.Join("', '", menues.Select(i => i.Properties.Name.Value));
            Menu menu = menues.FirstOrDefault(b => string.Equals(b.Properties.Name, itemName, StringComparison.OrdinalIgnoreCase))?.AsMenu();
            if (menu == null) throw new InvalidOperationException($"No ribbon menu found named '{itemName}', available choices are '{itemNames}'");
            SelectDropdownItem(menu, text);
            vcEngine.WaitWhileBusy();
        }
        public void ToggleDropdownItem(string groupName, string itemName, string text, ToggleState toggleState)
        {
            AutomationElement[] menues = Group(groupName).FindAllChildren();
            string itemNames = string.Join("', '", menues.Select(i => i.Properties.Name.Value));
            Menu menu = menues.FirstOrDefault(b => string.Equals(b.Properties.Name, itemName, StringComparison.OrdinalIgnoreCase))?.AsMenu();
            if (menu == null) throw new InvalidOperationException($"No ribbon menu found named '{itemName}', available choices are '{itemNames}'");
            var togglePattern = GetDropdownMenuItem(menu, text).Patterns.Toggle.Pattern;
            if (togglePattern.ToggleState != toggleState)
            {
                togglePattern.Toggle();;
            }
            menu.Patterns.ExpandCollapse.Pattern.Collapse();
            vcEngine.WaitWhileBusy();
        }
        public void SelectDropdownItem(string groupName, int itemIndex, string text)
        {
            var elements = Group(groupName).FindAllChildren();
            if (elements.Length <= itemIndex) throw new InvalidOperationException($"No ribbon menu found at the specified index {itemIndex}");
            Menu menu = elements[itemIndex].AsMenu();
            SelectDropdownItem(menu, text);
            vcEngine.WaitWhileBusy();
        }
        private void SelectDropdownItem(Menu menu, string text)
        {
            GetDropdownMenuItem(menu, text).Invoke();
            vcEngine.WaitWhileBusy();
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

        [Obsolete("Use FindComboBox instead")]
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

        public TextBox FindTextBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllChildren(cf => cf.ByControlType(ControlType.Edit)).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No text box found in group '{groupName}' at index {index}");
            Helpers.WaitUntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsTextBox();
        }

        public void EnterIntoTexBox(string groupName, int index, string text)
        {
            TextBox textbox = FindTextBox(groupName, index);
            if (!textbox.IsEnabled()) throw new InvalidOperationException($"Text box {groupName} at index {index} was not enabled");
            Helpers.WaitUntilResponsive(textbox, TimeSpan.FromSeconds(5));
            textbox.Text = text;
        }
        public ComboBox FindComboBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllDescendants(cf => cf.ByClassName("ComboBox").And(cf.ByAutomationId("PART_FocusSite"))).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No text combo box found in group '{groupName}' at index {index}");
            Helpers.WaitUntilResponsive(element, TimeSpan.FromSeconds(5));
            element.Click();
            Helpers.WaitUntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsComboBox();
        }
        public CheckBox FindCheckBox(string groupName, int index)
        {
            AutomationElement element = Group(groupName).FindAllChildren(cf => cf.ByClassName("ToggleButtonTool")).ElementAtOrDefault(index);
            if (element == null) throw new InvalidOperationException($"No check box found in group '{groupName}' at index {index}");
            Helpers.WaitUntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsCheckBox();
        }
        public CheckBox FindCheckBox(string groupName, string labelName)
        {
            AutomationElement element = Group(groupName).FindFirstDescendant(cf => cf.ByClassName("ToggleButtonTool").And(cf.ByName(labelName)));
            if (element == null) throw new InvalidOperationException($"No check box found in group '{groupName}' with name {labelName}");
            Helpers.WaitUntilResponsive(element, TimeSpan.FromSeconds(5));
            return element.AsCheckBox();
        }
    }
}