using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using System;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Windows
{
    public class VcMessageBox : IDisposable
    {
        private readonly Window mainWindow;
        private readonly Window window;
        private bool windowWasClosed;

        public VcMessageBox(Window mainWindow, Window window)
        {
            this.mainWindow = mainWindow;
            this.window = window;
        }

        public string Text { get { return window.FindFirstDescendant(cf => cf.ByAutomationId("MessageTextBlock")).AsTextBox().Text; } }
        public string Title => window.Title;

        public void ClickYes()
        {
            var element = window.FindFirstDescendant(cf => cf.ByAutomationId("_yes"));
            if (element == null) throw new InvalidOperationException("'Yes' button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
        }
        public void ClickOk()
        {
            windowWasClosed = true;
            var element = window.FindFirstDescendant(cf => cf.ByAutomationId("_ok"));
            if (element == null) throw new InvalidOperationException("'Ok' button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
        }
        public void ClickNo()
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("_no"));
            if (element == null) throw new InvalidOperationException("'No' button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
        }
        public void ClickCancel()
        {
            var element = window.FindFirstDescendant(cf => cf.ByAutomationId("_cancel"));
            if (element == null) throw new InvalidOperationException("'Cancel' button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
        }
        public void Cancel()
        {
            ClickCancel();
        }

        public static VcMessageBox Attach(Window mainWindow)
        {
            return Attach(mainWindow, null, null);
        }
        public static VcMessageBox Attach(Window mainWindow, Window messageBoxWindow)
        {
            return Attach(mainWindow, messageBoxWindow, null);
        }
        public static VcMessageBox Attach(Window mainWindow, Window messageBoxWindow, TimeSpan? timeout)
        {
            if (messageBoxWindow != null)
            {
                return new VcMessageBox(mainWindow, messageBoxWindow);
            }

            var window = Retry.WhileException(() => mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("_this"))?.AsWindow(), 
                timeout ?? TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(250));
            if (window == null) throw new InvalidOperationException("VC message box was not found");
            return new VcMessageBox(mainWindow, window);
        }
        public static VcMessageBox Attach(VcEngine vcEngine)
        {
            return Attach(vcEngine.MainWindow);
        }

        public static VcMessageBox AttachIfShown(VcEngine vcEngine)
        {
            return AttachIfShown(vcEngine.MainWindow);
        }

        public static VcMessageBox AttachIfShown(Window mainWindow)
        {
            return AttachIfShown(mainWindow, null);
        }
        public static VcMessageBox AttachIfShown(Window mainWindow, TimeSpan? timeout)
        {
            if (mainWindow.Patterns.Window.Pattern.WindowInteractionState.Value == WindowInteractionState.ReadyForUserInteraction) return null;
            var window = Retry.WhileException(() => mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("_this"))?.AsWindow(), 
                timeout ?? TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(250));
            if (window != null)
            {
                return new VcMessageBox(mainWindow, window);
            }
            return null;
        }

        public void Dispose()
        {
            if (!windowWasClosed && !window.IsClosed())
            {
                window.Close();
            }
        }

        public static string GetTextAndClose(VcEngine vcEngine)
        {
            using (VcMessageBox messageBox = Attach(vcEngine))
            {
                return messageBox.Text;
            }
        }

        public static string GetTextAndCloseIfShown(VcEngine vcEngine)
        {
            using (VcMessageBox messageBox = AttachIfShown(vcEngine))
            {
                return messageBox?.Text;
            }
        }
    }
}
