using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Shapes;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using VcEngineAutomation.Extensions;
using VcEngineAutomation.Models;
using VcEngineAutomation.Panels;
using VcEngineAutomation.Ribbons;
using VcEngineAutomation.Utils;
using VcEngineAutomation.Windows;

namespace VcEngineAutomation
{
    public class VcEngine
    {
        public static TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        public static TimeSpan DefaultRetryInternal = TimeSpan.FromMilliseconds(500);

        private readonly Lazy<AutomationElement> viewPort;
        private readonly Lazy<AutomationElement> quickAccessToolBar;
        // Hardcoded for now until other ways to get locale for application
        private static readonly Lazy<CultureInfo> LazyAppCultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"));
        private readonly Lazy<Button> lazyUndoButton;
        private readonly Lazy<Button> lazyRedoButton;

        public static string DefaultInstallationPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Visual Components", "Visual Components Professional");

        public DockedTabRetriever TabRetriever { get; }
        public Application Application { get; }
        public AutomationBase Automation { get; }

        public string ExecutablePath { get; }
        public bool IsR9 { get; }
        public bool IsR9OrAbove { get; }
        public bool IsR8 { get; }
        public bool IsR8OrAbove { get; }
        public bool IsR7 { get; }
        public bool IsR7OrAbove { get; }
        public bool IsR6 { get; }
        public bool IsR5 { get;  }
        public string Version { get; }
        public Window MainWindow { get; }
        public Ribbon Ribbon { get; }
        public string MainWindowName { get; }
        public AutomationElement QuickAccessToolbar => quickAccessToolBar.Value;
        public ApplicationMenu ApplicationMenu { get; }
        public Options Options { get; }
        public Camera Camera { get; }
        public Visual3DToolbar Visual3DToolbar { get; }
        public PropertiesPanel PropertiesPanel => Retry.WhileException(() => new PropertiesPanel(this), TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
        public PropertiesPanel DrawingPropertiesPanel => PropertiesPanel;
        public ECataloguePanel ECataloguePanel { get; }
        public OutputPanel OutputPanel { get; }
        public AutomationElement ViewPort => viewPort.Value;
        public World World { get; }
        public static CultureInfo CultureInfo => LazyAppCultureInfo.Value;
        public string LayoutFileName
        {
            get
            {
                var title = MainWindow.Title;
                var dashPos = title.LastIndexOf('-');
                if (dashPos < 0) return null;
                var layoutName = title.Substring(0, dashPos).Trim();
                return layoutName.StartsWith("<") ? null : layoutName;
            }
        }
        public bool IsProgressDialogModal { get; set; }

        public VcEngine(Application application, AutomationBase automation)
        {
            Application = application;
            Automation = automation;

            var tuple = GetProcesInfo(Process.GetProcessById(application.ProcessId));
            ExecutablePath = tuple.Item2;
            var fileVersionInfo = tuple.Item1;
            Version = fileVersionInfo.FileVersion;
            IsR7OrAbove = fileVersionInfo.FileMajorPart >= 4
                          && 
                          (fileVersionInfo.FileMinorPart > 0 
                           || (fileVersionInfo.FileMinorPart > 0
                               && fileVersionInfo.FileBuildPart >= 4));
            IsR8OrAbove = fileVersionInfo.FileMajorPart >= 4
                          &&
                          (fileVersionInfo.FileMinorPart > 0
                           || (fileVersionInfo.FileMinorPart > 0
                               && fileVersionInfo.FileBuildPart >= 5));
            IsR9OrAbove = fileVersionInfo.FileMajorPart >= 4
                          && fileVersionInfo.FileMinorPart >= 1;
            IsR5 = fileVersionInfo.ProductVersion.StartsWith("4.0.2");
            IsR6 = Version.StartsWith("4.0.3");
            IsR7 = Version.StartsWith("4.0.4");
            IsR8 = Version.StartsWith("4.0.5") || Version.StartsWith("4.0.6");
            IsR9 = Version.StartsWith("4.1.0");

            Console.WriteLine($"VcEngine Version: {Version}");
            MainWindowName = fileVersionInfo.FileDescription;

            Console.WriteLine("Waiting for application main window");
            MainWindow = Retry.WhileException(() => Application.GetMainWindow(automation), TimeSpan.FromMinutes(2), TimeSpan.FromMilliseconds(200));
            TabRetriever = new DockedTabRetriever(MainWindow);

            Console.WriteLine("Waiting for main ribbon");
            Tab mainTab = Retry.WhileException(() => MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("XamRibbonTabs")).AsTab(), TimeSpan.FromMinutes(2));

            viewPort = new Lazy<AutomationElement>(() => MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("Viewport")));
            quickAccessToolBar = new Lazy<AutomationElement>(() => MainWindow.FindFirstDescendant(cf => cf.ByClassName("QuickAccessToolbar")));
            World = new World(this);
            Ribbon = new Ribbon(this, mainTab);
            ApplicationMenu = new ApplicationMenu(this);
            Visual3DToolbar = new Visual3DToolbar(this);
            Options = new Options(ApplicationMenu);
            Camera = new Camera(this);
            OutputPanel = new OutputPanel(this, () => TabRetriever.GetPane("VcOutput"));
            ECataloguePanel = new ECataloguePanel(this, () => TabRetriever.GetPane("VcECatalogue"));
            
            Console.WriteLine("Waiting for ribbon to become enabled");
            Retry.While(() => Retry.WhileException(() => !Ribbon.HomeTab.TabPage.IsEnabled, TimeSpan.FromMinutes(2)), TimeSpan.FromMinutes(2));

            Console.WriteLine("Setting main window as foreground");
            MainWindow.SetForeground();

            CheckForCrashAction = null;

            lazyUndoButton = new Lazy<Button>(() => FindQuickAccessToolbarButton("QATUndo", "Undo"));
            lazyRedoButton = new Lazy<Button>(() => FindQuickAccessToolbarButton("QATRedo", "Redo"));
        }

        /// <summary>
        /// Action to add custom code to check for crashes, this action should be quick
        /// </summary>
        public Action<VcEngine> CheckForCrashAction { get; set; }
        /// <summary>
        /// Function to add custom code to check is shell is busy
        /// </summary>
        public Func<VcEngine, TimeSpan, TimeSpan, bool> IsShellBusyAction { get; set; }

        private Tuple<FileVersionInfo, string> GetProcesInfo(Process process)
        {
            try
            {
                return Tuple.Create(process.MainModule.FileVersionInfo, process.MainModule.FileName);
            }
            catch (Exception)
            {
                // This fixes a problem on a test laptop where it could not retrieve the Process.MainModule for some
                // reason. This hack has been copied from https://stackoverflow.com/a/5497319/28553
                var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                using (var results = searcher.Get())
                {
                    var query = from p in Process.GetProcesses()
                        join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                        select new
                        {
                            Process = p,
                            Path = (string)mo["ExecutablePath"],
                            Id = (int)(uint)mo["ProcessId"],
                            CommandLine = (string)mo["CommandLine"],
                            ExecutablePath = (string)mo["ExecutablePath"],
                        };
                    var processObject = query.First(q => process.Id == q.Id);
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(string.IsNullOrEmpty(processObject.Path) 
                        ? Regex.Match(processObject.CommandLine, "\"(.*)\"").Groups[1].Value : 
                        processObject.Path);
                    return Tuple.Create(fileVersionInfo, processObject.ExecutablePath);
                }
            }
        }

        public CommandPanel GetCommandPanel()
        {
            return new CommandPanel(this, () => MainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CommandPanelViewModelTabItem")));
        }
        [Obsolete("Retrieve panel through automation id instead")]
        public CommandPanel GetCommandPanel(string startOfTitle)
        {
            return new CommandPanel(this, () => TabRetriever.GetPane("CommandPanelViewModel"));
        }

        public void WaitWhileBusy()
        {
            WaitWhileBusy(DefaultTimeout, DefaultRetryInternal);
        }
        public void WaitWhileBusy(TimeSpan timeout, TimeSpan uiRetry)
        {
            var aMessageBoxWindow = Retry.WhileException(() =>
                    MainWindow.FindFirstChild(cf => cf.ByControlType(ControlType.Window).And(cf.ByClassName("#32770")).Or(cf.ByAutomationId("TextboxDialog"))),
                timeout, uiRetry);
            if (aMessageBoxWindow != null) return;

            if (ShellIsBusy(timeout))
            {
                bool shellIsStillBusy = Retry.While(() => ShellIsBusy(timeout), isBusy => isBusy, timeout, DefaultRetryInternal);
                if (shellIsStillBusy) throw new TimeoutException("Timeout while waiting for progress bar to disappear");
            }
        }
        public void WaitWhileBusy(TimeSpan? timeout)
        {
            if (timeout.HasValue)
            {
                WaitWhileBusy(timeout.Value, DefaultRetryInternal);
            }
            else
            {
                WaitWhileBusy();
            }
        }
        public void WaitWhileBusy(TimeSpan timeout)
        {
            WaitWhileBusy(timeout, DefaultRetryInternal);
        }

        private bool ShellIsBusy(TimeSpan timeout)
        {
            TimeSpan retryInterval = DefaultRetryInternal;
            Retry.WhileException(() => Wait.UntilResponsive(MainWindow), timeout, retryInterval);

            if (IsShellBusyAction?.Invoke(this, timeout, retryInterval) ?? false) return true;

            if (IsProgressDialogModal)
            {
                // When the progress dialog is showing in 4.1, the window may be reported as ready for user interaction
                var interactionState = Retry.WhileException(() => MainWindow.Patterns.Window.Pattern.WindowInteractionState.Value, timeout, retryInterval);
                if (interactionState == WindowInteractionState.ReadyForUserInteraction) return false;
            }

            CheckForCrash(timeout, retryInterval);

            if (FindProgressDialog(timeout, retryInterval) != null)
            {
                return true;
            }

            return false;
        }

        public void CheckForCrash(TimeSpan timeout, TimeSpan retryInterval)
        {
            MainWindow.WaitWhileBusy(timeout, retryInterval);
            if (Retry.WhileException(() => MainWindow.Patterns.Window.Pattern.WindowInteractionState.Value, timeout, retryInterval) == WindowInteractionState.ReadyForUserInteraction) return;

            Window[] windows = Retry.WhileException(() => MainWindow.ModalWindows, timeout, retryInterval);
            if (!windows.Any()) return;

            // Catch normal VC exception stack trace
            Window window = windows.FirstOrDefault(w => w.Properties.Name.ValueOrDefault == MainWindowName && w.Properties.AutomationId.ValueOrDefault == "_this");
            if (window != null)
            {
                var text = VcMessageBox.GetTextAndClose(window);
                if (text.ToLower().Contains("unhandled exception")) throw new InvalidOperationException(text);
            }

            CheckForCrashAction?.Invoke(this);
        }
        public void CheckForCrash()
        {
            CheckForCrash(DefaultTimeout, DefaultRetryInternal);
        }
        public void CheckForCrash(TimeSpan timeout)
        {
            CheckForCrash(timeout, DefaultRetryInternal);
        }

        public void MoveFocusTo3DViewPort()
        {
            MoveMouseTo3DViewPort();
            Mouse.Click(MouseButton.Middle);
            WaitWhileBusy();
        }

        public void MoveMouseTo3DViewPort()
        {
            MoveMouseTo3DViewPort(null);
        }
        public void MoveMouseTo3DViewPort(Point moveOffset)
        {
            Mouse.MoveTo(viewPort.Value.GetCenter());
            if (moveOffset != null)
            {
                Mouse.MoveBy((int)moveOffset.X, (int)moveOffset.Y);
            }
        }

        public void LoadLayout(string layoutFile)
        {
            LoadLayout(layoutFile, TimeSpan.FromMinutes(5));
        }
        public void LoadLayout(string layoutFile, TimeSpan waitTimeSpan)
        {
            AutomationElement menuBar = ApplicationMenu.FindMenuItem("OpenBackstage", "Computer");
            menuBar.FindFirstDescendant(cf => cf.ByAutomationId("OpenFile")).AsButton().Invoke();
            Wait.UntilInputIsProcessed();
            //MainWindow.WaitWhileBusy();
            FileDialog.Attach(MainWindow).Open(layoutFile);
            Retry.While(() => FindProgressDialog() != null, waitTimeSpan);
            WaitWhileBusy(waitTimeSpan);
        }

        public void SaveLayout(string fileToSave)
        {
            SaveLayout(fileToSave, false, null);
        }
        public void SaveLayout(string fileToSave, bool overwrite)
        {
            SaveLayout(fileToSave, overwrite, null);
        }
        public void SaveLayout(string fileToSave, bool overwrite, TimeSpan? waitForTimeSpan)
        {
            if (!fileToSave.ToLower().EndsWith(".vcmx")) throw new InvalidOperationException($"File extension when saving layout file must be 'vcmx' and not '{Path.GetExtension(fileToSave)}'");
            AutomationElement menuBar = ApplicationMenu.FindMenuItem("SaveAsBackstage", "Computer");
            menuBar.FindFirstDescendant(cf => cf.ByAutomationId("OpenFile")).AsButton().Invoke();
            Wait.UntilInputIsProcessed();
            MainWindow.WaitWhileBusy();
            FileDialog.Attach(MainWindow).Save(fileToSave, overwrite);
            WaitWhileBusy(waitForTimeSpan ?? TimeSpan.FromMinutes(1));
        }

        public Button FindQuickAccessToolbarButton(string automationId, string name)
        {
            var button = QuickAccessToolbar.FindFirstChild(cf => cf.ByAutomationId(automationId))?.AsButton();
            if (button == null)
            {
                var dropdown = QuickAccessToolbar.FindFirstDescendant(cf => cf.ByAutomationId("dropdownBtn"));
                // Toggle pattern is supported but does not activate on R5 correctly
                //dropdown.Patterns.Toggle.Pattern.Toggle();
                dropdown.Click();
                var menuItem = MainWindow.Popup.FindFirstChild(cf => cf.ByName(name))?.AsMenuItem();
                if (menuItem == null) throw new InvalidOperationException($"Feature {name} is not enabled in exe.config file");
                menuItem.Invoke();
                button = QuickAccessToolbar.FindFirstChild(cf => cf.ByAutomationId(automationId))?.AsButton();
            }
            if (button == null) throw new InvalidOperationException($"Could not find {name} button");
            return button.AsButton();
        }

        public Window FindProgressDialog()
        {
            return FindProgressDialog(DefaultTimeout, DefaultRetryInternal);
        }
        public Window FindProgressDialog(TimeSpan timeout)
        {
            return FindProgressDialog(timeout, DefaultRetryInternal);
        }
        public Window FindProgressDialog(TimeSpan timeout, TimeSpan retryInterval)
        {
            return Retry.WhileException(() => MainWindow.FindFirstChild(cf => cf.ByAutomationId("ProgressBarDialog")),
                timeout,
                retryInterval)?.AsWindow();
        }

        public void DoUndo()
        {
            if (!UndoButton.IsEnabled) throw new InvalidOperationException("There is nothing to undo");
            UndoButton.Invoke();
        }
        public void DoRedo()
        {
            if (!RedoButton.IsEnabled) throw new InvalidOperationException("There is nothing to redo");
            RedoButton.Invoke();
        }

        public Button UndoButton => lazyUndoButton.Value;
        public Button RedoButton => lazyRedoButton.Value;

        public static VcEngine Attach()
        {
            Process process = Process.GetProcessesByName("VisualComponents.Essentials").Concat(Process.GetProcessesByName("VisualComponents.Engine")).FirstOrDefault();
            if (process == null)
            {
                throw new Exception("No process could be attached");
            }
            Application application = Application.Attach(process.Id);
            var automation = new UIA3Automation();
            return new VcEngine(application, automation);
        }

        public static VcEngine AttachOrLaunch()
        {
            return AttachOrLaunch(DefaultInstallationPath);
        }

        public static VcEngine AttachOrLaunch(string installationPath)
        {
            return AttachOrLaunch(new ProcessStartInfo()
            {
                WorkingDirectory = installationPath,
                FileName = Path.Combine(installationPath, "VisualComponents.Engine.exe"),
                Arguments = "-AutomationMode",
                WindowStyle = ProcessWindowStyle.Maximized
            });
        }
        public static VcEngine AttachOrLaunch(ProcessStartInfo processStartInfo)
        {
            Process process = Process.GetProcessesByName("VisualComponents.Essentials").Concat(Process.GetProcessesByName("VisualComponents.Engine")).FirstOrDefault();
            if (process == null)
            {
                Application.Launch(processStartInfo);
            }
            return Attach();
        }
    }
}
