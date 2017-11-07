using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;

namespace VcEngineAutomation.Extensions
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Returns the popup window as normal Window.Popup doesnt work
        /// </summary>
        public static Window[] GetCreatedWindowsForAction(this Window mainWindow, Action action)
        {
            AutomationElement[] windows = mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.Window));
            action();
            return mainWindow.FindAllChildren(cf => cf.ByControlType(ControlType.Window)).Except(windows).Select(ae => ae.AsWindow()).ToArray();
        }
    }

    public static class StringExtensions
    {
        public static string RemoveInvalidFilePathCharacters(this string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) => current.Replace(c.ToString(), "_"));
        }
    }

    public static class WindowExtensions
    {
        private const int DefaultSleepForComException = 200;

        public static TextBox GetTextBoxForLabel(this AutomationElement parent, string labelName)
        {
            return GetItemForLabel(parent, labelName).AsTextBox();
        }

        public static AutomationElement GetItemForLabel(this AutomationElement parent, string labelName)
        {
            var items = parent.FindAllDescendants();
            var label = items.Select((item, index) => new {item, index}).FirstOrDefault(i => i.item.Properties.Name == labelName);
            if (label == null) throw new InvalidOperationException($"Could not find the label with name '{labelName}'");
            return items[label.index + 1];
        }

        public static string ToDebugString(this Window item)
        {
            if (item == null) return "NULL";
            string extra = AutomationElementExtensions.ToDebugString(item);
            return $"Title={item.Properties.Name.ToDebugString()}, " +
                   $"IsModal={item.IsModal}, " +
                   $"WindowsInteractionState={item.Patterns.Window.Pattern.WindowInteractionState.ValueOrDefault}, " +
                   $"IsTopMost={item.Patterns.Window.Pattern.IsTopmost.ValueOrDefault}, " +
                   extra;
        }

        public static void Print(this Window item)
        {
            System.Diagnostics.Debug.WriteLine(item.ToDebugString());
        }

        public static bool IsClosed(this Window item)
        {
            if (item.IsOffscreen) return true;
            return item.Properties.ProcessId == 0;
        }

        public static void WaitWhileBusy(this Window item)
        {
            Retry.While(
                () => item.Patterns.Window.PatternOrDefault?.WindowInteractionState?.ValueOrDefault,
                v => v != null &&
                    v != WindowInteractionState.Running &&
                    v != WindowInteractionState.BlockedByModalWindow &&
                    v != WindowInteractionState.ReadyForUserInteraction,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromMilliseconds(100));
        }

        public static bool CanWindowBeResized(this Window window, int attemptIndex=0)
        {
            try
            {
                if (!window.Patterns.Window.IsSupported) return false;
                bool canWindowBeResized = window.Patterns.Window.Pattern.CanMaximize.ValueOrDefault && window.Patterns.Window.Pattern.CanMinimize.ValueOrDefault;
                return canWindowBeResized;
            }
            catch (COMException)
            {
                if (attemptIndex == 5)
                {
                    throw;
                }
                WaitWhileBusy(window);
                return CanWindowBeResized(window, attemptIndex + 1);
            }
        }

        public static Window FindWindowProtected(this Window window, Func<ConditionFactory, ConditionBase> condition, int index = 0)
        {
            try
            {
                return window.FindFirstDescendant(condition)?.AsWindow();
            }
            catch (COMException)
            {
                if (index >= 5) throw;
                Thread.Sleep(200);
                return FindWindowProtected(window, condition, index + 1);
            }
        }

        public static Window[] FindAllWindowsProtected(this Window window, Func<ConditionFactory, ConditionBase> condition, int index = 0)
        {
            try
            {
                return window.FindAllDescendants(condition).Select(ae => ae.AsWindow()).ToArray();
            }
            catch (COMException)
            {
                if (index >= 5) throw;
                Thread.Sleep(200);
                return FindAllWindowsProtected(window, condition, index + 1);
            }
        }

        public static Window[] FindModalWindowsProtected(this Window window)
        {
            return FindModalWindowsProtected(window, 0);
        }

        private static Window[] FindModalWindowsProtected(this Window window, int index )
        {
            try
            {
                Window[] modalWindows = window.ModalWindows;
                Window progressBarDialog = modalWindows.FirstOrDefault(w => w.Properties.AutomationId.ValueOrDefault == "ProgressBarDialog");
                modalWindows = modalWindows.Except(new[] { progressBarDialog }).ToArray();
                if (!modalWindows.Any() && progressBarDialog != null)
                {
                    return progressBarDialog.FindModalWindowsProtected();
                }
                return modalWindows;
            }
            catch (COMException)
            {
                if (index >= 5) throw;
                Thread.Sleep(DefaultSleepForComException);
                return FindModalWindowsProtected(window, index + 1);
            }
        }


        public static Window[] RetryUntilAnyModalWindow(this Window window)
        {
            return RetryUntilAnyModalWindow(window, null);
        }
        public static Window[] RetryUntilAnyModalWindow(this Window window, TimeSpan? waitTimeSpan)
        {
            return Retry.While(window.FindModalWindowsProtected, windows => !windows.Any(), waitTimeSpan ?? TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(500));
        }

    }

    public static class ComboboxExtensions
    {
        public static void SelectItem(this ComboBox valueComboBox, string value)
        {
            valueComboBox.Click();
            var options = valueComboBox.Items.Select(i => Tuple.Create(i, i.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsLabel().Text)).ToArray();
            var listItem = options.FirstOrDefault(t => t.Item2.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));
            if (listItem == null) throw new InvalidOperationException($"there is no value '{value}' among '{string.Join("', '", options.Select(t => t.Item2))}'");
            if (!listItem.Item1.IsSelected)
            {
                listItem.Item1.IsSelected = true;
            }
            Wait.UntilInputIsProcessed();
            if (valueComboBox.Patterns.ExpandCollapse.Pattern.ExpandCollapseState.Value == ExpandCollapseState.Expanded)
            {
                valueComboBox.Collapse();
            }
        }

        public static string GetSelectedText(this ComboBox comboBox)
        {
            return comboBox.SelectedItem?.FindFirstChild().AsLabel().Text;
        }
        /*   public static void ClickItem(this ComboBox comboBox, string name)
           {
               Retry.ForDefault(() => comboBox.Enabled);
               ListItems items = comboBox.Items;
               string itemNames = string.Join("', '", items.Select(i => i.Text));
               ListItem listItem = items.FirstOrDefault(g => string.Equals(g.Text, name, StringComparison.OrdinalIgnoreCase));
               listItem.Should().NotBeNull($"no list item found named '{name}', available choices are '{itemNames}'");
               listItem?.Select(true);
           }*/
    }/*
    public static class KeyboardExtensions
    {
        public static void PressShortcut(this AttachedKeyboard keyboard, KeyboardInput.SpecialKeys specialKeys, string keys)
        {
            keyboard.HoldKey(specialKeys);
            keyboard.Enter(keys);
            keyboard.LeaveKey(specialKeys);
        }
    }*/
}