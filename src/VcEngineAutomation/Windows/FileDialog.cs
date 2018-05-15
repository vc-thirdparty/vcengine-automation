using System;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Windows
{
    public class FileDialog
    {
        private readonly Window mainWindow;
        private readonly Window window;
        private readonly Lazy<TextBox> fileNameTextBox;

        public FileDialog(Window mainWindow, Window window)
        {
            this.mainWindow = mainWindow;
            this.window = window;
            var isOpenFileDialog = window.Title.Equals("Open", StringComparison.OrdinalIgnoreCase);
            fileNameTextBox = new Lazy<TextBox>(
                () => this.window.FindFirstDescendant(cf => 
                    cf.ByAutomationId(isOpenFileDialog ? "1148" : "1001").And(cf.ByControlType(ControlType.Edit))).AsTextBox());
        }

        private string FileName
        {
            set
            {
                if (value != null)
                {
                    fileNameTextBox.Value.Enter(value);
                    Wait.UntilInputIsProcessed();
                }
            }
        }

        public void Save(string filename, bool overwrite=true, bool waitForWriteIsCompleted=true)
        {
            if (Path.GetFileName(filename).Intersect(Path.GetInvalidFileNameChars()).Any()) throw new InvalidOperationException($"Filename '{0}' contains invalid filename chars");
            if (Path.GetDirectoryName(filename).Intersect(Path.GetInvalidPathChars()).Any()) throw new InvalidOperationException($"Filename '{0}' contains invalid filename chars");
            if (File.Exists(filename))
            {
                if (!overwrite)
                {
                    Cancel();
                    Keyboard.Type(VirtualKeyShort.ESCAPE);
                    throw new InvalidOperationException("File exists but parameter 'overwrite' in FileDialog.Save() was false");
                }
                File.Delete(filename);
            }
            FileName = filename;
            fileNameTextBox.Value.KeyIn(VirtualKeyShort.RETURN);
            Wait.UntilInputIsProcessed();
            mainWindow.WaitWhileBusy();

            Thread.Sleep(500);
            if (window.Properties.ProcessId != 0)
            {
                var messageBox = MessageBox.Attach(window);
                var text = messageBox.Text;
                messageBox.ForceClose();
                throw new InvalidOperationException($"Window displayed on top of save window with text='{text}'");
            }

            if (waitForWriteIsCompleted)
            {
                // Ignore IOException until file is ready to be used
                Retry.WhileException(() => File.OpenRead(filename).Dispose(), TimeSpan.FromMinutes(10));
            }
        }
        public void Open(string fileName)
        {
            if (Path.GetFileName(fileName).Intersect(Path.GetInvalidFileNameChars()).Any()) throw new InvalidOperationException($"Filename '{0}' contains invalid filename chars");
            if (Path.GetDirectoryName(fileName).Intersect(Path.GetInvalidPathChars()).Any()) throw new InvalidOperationException($"Filename '{0}' contains invalid filename chars");
            if (!File.Exists(fileName))
            {
                Cancel();
                Keyboard.Type(VirtualKeyShort.ESCAPE);
                throw new FileNotFoundException("File could not be found", fileName);
            }
            FileName = fileName;
            fileNameTextBox.Value.KeyIn(VirtualKeyShort.RETURN);
            Wait.UntilInputIsProcessed();
            mainWindow.WaitWhileBusy();

            /*if (!window.IsClosed())
            {
                var messageBox = MessageBox.Attach(window);
                var text = messageBox.Text;
                messageBox.ForceClose();
                throw new XunitException($"Window displayed on top of save window with text='{text}'");
            }*/
        }

        public void Cancel()
        {
            var element = window.FindFirstChild(cf => cf.ByAutomationId("2"));
            if (element == null) throw new InvalidOperationException("Cancel button is not found");
            element.AsButton().Invoke();
            mainWindow.WaitWhileBusy();
        }

        private static Window FindWindow(Window mainWindow)
        {
            return Retry.While(() => mainWindow.FindFirstDescendant(cf => cf.ByClassName("#32770"))?.AsWindow(),
                w => w == null,
                TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
        }

        public static FileDialog Attach(Window mainWindow)
        {
            var window = Retry.WhileException(() => FindWindow(mainWindow), TimeSpan.FromSeconds(5));
            return new FileDialog(mainWindow, window);
        }

        public static FileDialog Attach(VcEngine vcEngine)
        {
            return Attach(vcEngine.MainWindow);
        }
    }
}
