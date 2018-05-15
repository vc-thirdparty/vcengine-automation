using System.Collections.Generic;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Shapes;
using FlaUI.Core.WindowsAPI;

namespace VcEngineAutomation.Extensions
{
    public static class AutomationElementExtensions
    {
        public static void LeftClick(this AutomationElement element)
        {
            Mouse.LeftClick(element.GetCenter());
        }
        public static Point GetCenter(this AutomationElement element)
        {
            return element.Properties.BoundingRectangle.Value.Center;
        }
        public static bool IsVisible(this AutomationElement element)
        {
            return !element.Properties.IsOffscreen.ValueOrDefault;
        }
        public static string ToDebugString(this AutomationElement item)
        {
            if (item == null) return "NULL";
            string extra = string.Empty;
            if (item.Properties.ControlType == ControlType.Text)
            {
                Label label = item.AsLabel();
                extra = $"Text={label.Text}, ";
            }
            if (item.Properties.ControlType == ControlType.Edit)
            {
                TextBox textBox = item.AsTextBox();
                extra = $"Text={textBox.Text}, ";
            }
            if (item.Properties.ControlType == ControlType.TabItem)
            {
                //extra = $"Selected={item.AsTabItem().IsSelected}, ";
            }
            if (item.Properties.ControlType == ControlType.ListItem)
            {
                extra = $"Selected={item.AsTreeItem().IsSelected}, Text={item.AsLabel().Text}, ";
            }
            if (item.Properties.ControlType == ControlType.MenuItem)
            {
                extra = $"Text={item.AsMenuItem().Text}, ";
            }
            /*if (item is WPFListItem)
            {
                extra = $"Text={((WPFListItem)item).Text}, ";
            }*/
            return $"Name={item.Properties.Name.ToDebugString()}, " +
                   $"ClassName={item.Properties.ClassName.ToDebugString()}, " +
                   $"ControlType={item.Properties.LocalizedControlType.ToDebugString()}({item.GetType().Name}), " +
                   $"AutomationId={item.Properties.AutomationId.ToDebugString()}, " +
                   $"Enabled={item.Properties.IsEnabled.ToDebugString()}, " +
                   //$"IsVisible={item.Current.is}, " +
                   $"IsOffScreen={item.Properties.IsOffscreen.ToDebugString()}, " +
                   $"ProcessId={item.Properties.ProcessId.ToDebugString()}, " +
                   extra;
        }

        public static object ToDebugString<T>(this AutomationProperty<T> property)
        {
            T value;
            if (property.TryGetValue(out value))
            {
                return value != null ? value.ToString() : "[Null]";
            }
            return "[Unavailable]";
        }

        public static void KeyIn(this AutomationElement item, VirtualKeyShort key)
        {
            item.Focus();
            Keyboard.Type(key);
        }

        public static void Print(this IEnumerable<AutomationElement> items)
        {
            foreach (AutomationElement item in items)
            {
                System.Diagnostics.Debug.WriteLine(item.ToDebugString());
            }
        }
        public static void Print(this AutomationElement item)
        {
            System.Diagnostics.Debug.WriteLine(item.ToDebugString());
        }
        public static void PrintHiearchy(this AutomationElement item, string indent = "")
        {
            if (item == null) return;
            System.Diagnostics.Debug.WriteLine(indent + item.ToDebugString());
            foreach (AutomationElement childItem in item.FindAllChildren())
            {
                PrintHiearchy(childItem, indent + "  ");
            }
        }
        public static void PrintHiearchy(this IEnumerable<AutomationElement> items, string indent = "")
        {
            foreach (var item in items)
            {
                item.PrintHiearchy(indent);
            }
        }

        public static string ToHiearchyString(this AutomationElement item, string indent = "")
        {
            if (item == null) return "[null]";
            string output = $"{indent}{item.ToDebugString()}\r\n";
            foreach (AutomationElement childItem in item.FindAllChildren())
            {
                output += ToHiearchyString(childItem, indent + "  ");
            }
            return output;
        }

        public static AutomationElement GetParent(this AutomationElement item)
        {
            return item.Automation.TreeWalkerFactory.GetControlViewWalker().GetParent(item);
        }
    }
}
