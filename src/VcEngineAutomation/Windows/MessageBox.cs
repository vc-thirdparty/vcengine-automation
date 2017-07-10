using System;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Windows
{
    public class MessageBox : IDisposable
    {
        private readonly Window mainWindow;
        private readonly Window window;

        private MessageBox(Window mainWindow, Window window)
        {
            this.mainWindow = mainWindow;
            this.window = window;
        }

        private bool IsClosed { get; set; }

        public string Text
        {
            get { return (window.FindFirstDescendant(cf => cf.ByAutomationId("65535")) ?? window.FindFirstDescendant(cf => cf.ByAutomationId("ContentText"))).AsLabel().Text; }
        }

        public string Title => window.Title;

        public void ClickYes()
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("6"));
            if (element == null) throw new InvalidOperationException("Yes button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
            IsClosed = true;
        }
        public void ClickNo()
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("7"));
            if (element == null) throw new InvalidOperationException("No button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
            IsClosed = true;
        }
        public void ClickOk()
        {
            AutomationElement element = window.FindFirstDescendant(cf => cf.ByAutomationId("2"));
            if (element == null) throw new InvalidOperationException("Ok button could not be found in message box");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
            IsClosed = true;
        }

        public void ForceClose()
        {
            AutomationElement element = 
                window.FindFirstDescendant(cf => cf.ByAutomationId("7")) 
                ?? window.FindFirstDescendant(cf => cf.ByAutomationId("2"))
                ?? window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
            element.AsButton().Invoke();
        }

        public static MessageBox Attach(VcEngine vcEngine)
        {
            return Attach(vcEngine.MainWindow);
        }

        public static MessageBox Attach(Window mainWindow)
        {
            var window = mainWindow.FindFirstDescendant(cf => cf.ByClassName("#32770"));
            if (window == null) throw new InvalidOperationException("WPF message box was not found");
            return new MessageBox(mainWindow, window.AsWindow());
        }

        public static MessageBox AttachIfShown(VcEngine vcEngine)
        {
            return AttachIfShown(vcEngine.MainWindow);
        }

        public static MessageBox AttachIfShown(Window parent)
        {
            var state = parent.Patterns.Window.PatternOrDefault?.WindowInteractionState?.ValueOrDefault;
            if (state == WindowInteractionState.ReadyForUserInteraction || state == null) return null;

            Window window = parent.FindWindowProtected(cf => cf.ByControlType(ControlType.Window).And(cf.ByClassName("#32770")));
            //Window window = mainWindow.ModalWindows.FirstOrDefault(w => w.Properties.ClassName == "#32770");
            if (window != null)
            {
                return new MessageBox(parent, window);
            }
            return null;
        }

        public static string GetTextAndClose(VcEngine vcEngine, string buttonToPress = null)
        {
            return GetTextAndClose(vcEngine.MainWindow, buttonToPress);
        }
        public static string GetTextAndClose(Window mainWindow, string buttonToPress=null)
        {
            using (MessageBox messageBox = Attach(mainWindow))
            {
                string text = messageBox.Text;
                if (buttonToPress != null)
                {
                    messageBox.window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button).And(cf.ByName(buttonToPress))).AsButton().Invoke();
                    messageBox.IsClosed = true;
                }
                return text;
            }
        }

        public static string GetTextAndCloseIfShown(VcEngine vcEngine)
        {
            return GetTextAndCloseIfShown(vcEngine.MainWindow);
        }
        public static string GetTextAndCloseIfShown(Window mainWindow)
        {
            using (MessageBox messageBox = AttachIfShown(mainWindow))
            {
                return messageBox?.Text;
            }
        }

        public void Dispose()
        {
            if (!IsClosed && !window.IsClosed())
            {
                window.Close();
            }
        }
    }
}
