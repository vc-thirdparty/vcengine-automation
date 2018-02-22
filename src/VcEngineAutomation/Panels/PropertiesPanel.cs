using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Panels
{
    public class PropertiesPanel
    {
        private readonly Lazy<ToggleButton> lockButton;
        private readonly IFormatProvider appCultureInfo;

        public PropertiesPanel(VcEngine vcEngine)
        {
            Pane = vcEngine.MainWindow.FindFirstChild(cf => cf.ByAutomationId("dockManager")).FindFirstDescendant("VcPropertyEditorPanel");
            lockButton = new Lazy<ToggleButton>(() => Pane.FindFirstDescendant(cf => cf.ByAutomationId("Property.LockButton")).AsToggleButton());
            appCultureInfo = VcEngine.CultureInfo;
        }

        public AutomationElement CoordinatePane
        {
            get { return Pane.FindFirstDescendant(cf => cf.ByClassName("PropertyEditorView")); }
        }

        public AutomationElement Pane { get; set; }

        public void MoveRelative(Position relative)
        {
            var pos = Position;
            pos.X += relative.X;
            pos.Y += relative.Y;
            pos.Z += relative.Z;
            Position = pos;
        }

        public string Header => HeaderLabel.Text.Replace("  ", " ");
        public Label HeaderLabel => Pane.FindFirstDescendant(cf => cf.ByAutomationId("SelectedObjectName")).AsLabel();

        public Position Position
        {
            get
            {
                TextBox[] textBoxs = CoordinatePane.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(ae => ae.AsTextBox()).ToArray();
                return new Position
                {
                    X = double.Parse(textBoxs[0].Text, appCultureInfo),
                    Y = double.Parse(textBoxs[1].Text, appCultureInfo),
                    Z = double.Parse(textBoxs[2].Text, appCultureInfo)
                };
            }
            set
            {
                TextBox[] textBoxs = CoordinatePane.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(ae => ae.AsTextBox()).ToArray();
                if (!double.Parse(textBoxs[0].Text, appCultureInfo).Equals(value.X))
                {
                    textBoxs[0].EnterWithReturn(value.X);
                }
                if (!double.Parse(textBoxs[1].Text, appCultureInfo).Equals(value.Y))
                {
                    textBoxs[1].EnterWithReturn(value.Y);
                }
                if (!double.Parse(textBoxs[2].Text, appCultureInfo).Equals(value.Z))
                {
                    textBoxs[2].EnterWithReturn(value.Z);
                }
            }
        }

        public Rotation Rotation
        {
            get
            {
                TextBox[] textBoxs = CoordinatePane.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(ae => ae.AsTextBox()).ToArray();
                return new Rotation
                {
                    Rx = double.Parse(textBoxs[5].Text, appCultureInfo),
                    Ry = double.Parse(textBoxs[4].Text, appCultureInfo),
                    Rz = double.Parse(textBoxs[3].Text, appCultureInfo)
                };
            }
            set
            {
                TextBox[] textBoxs = CoordinatePane.FindAllDescendants(cf => cf.ByControlType(ControlType.Edit)).Select(ae => ae.AsTextBox()).ToArray();
                if (!double.Parse(textBoxs[5].Text, appCultureInfo).Equals(value.Rx))
                {
                    textBoxs[5].EnterWithReturn(value.Rx);
                }
                if (!double.Parse(textBoxs[4].Text, appCultureInfo).Equals(value.Ry))
                {
                    textBoxs[4].EnterWithReturn(value.Ry);
                }
                if (!double.Parse(textBoxs[3].Text, appCultureInfo).Equals(value.Rz))
                {
                    textBoxs[3].EnterWithReturn(value.Rz);
                }
            }
        }

        private Tuple<string,string> SplitPropertyIntoTabAndName(string propertyName)
        {
            string[] strings = propertyName.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 1)
            {
                return Tuple.Create(strings[0], strings[1]);
            }
            return Tuple.Create("Default", strings[0]);
        }
        private string GetAutomationId(string propertyName)
        {
            return $"Property.{propertyName}";
        }

        public void ShowTabForProperty(string propertyName)
        {
            ShowTab(SplitPropertyIntoTabAndName(propertyName).Item1);
        }
        public void ShowTab(string tabName)
        {
            AutomationElement item = Pane.FindFirstDescendant(cf => cf.ByAutomationId("TabsListView"));
            if (item.IsVisible())
            {
                var tab = item.FindFirstDescendant(cf => cf.ByAutomationId($"TabItem.{tabName}"))?.AsTabItem();
                if (tab == null) throw new InvalidOperationException($"Component has no tab named '{tabName}'");
                if (!tab.IsSelected)
                {
                    tab.Select();
                }
                return;
            }
            if (tabName != "Default") throw new InvalidOperationException($"Component has no tab named '{tabName}'");
        }
        [Obsolete("Use ShowTab instead")]
        public void ClickTab(string title)
        {
            ShowTab(title);
        }

        private AutomationElement FindPropertyControl(string propertyName, bool verifyIsEnabled)
        {
            ShowTabForProperty(propertyName);
            AutomationElement control = Pane.FindFirstDescendant(cf => cf.ByAutomationId(GetAutomationId(propertyName)));
            if (control == null) throw new InvalidOperationException($"No such property '{propertyName}'");
            if (verifyIsEnabled && !control.IsEnabled) throw new InvalidOperationException($"Property '{propertyName}' is not enabled");
            return control;
        }

        public void ClickButton(string propertyName)
        {
            FindPropertyControl(propertyName, true).AsButton().Invoke();
        }

        public void SetProperty(string propertyName, object value)
        {
            var control = FindPropertyControl(propertyName, true);
            if (control.Properties.ControlType == ControlType.ComboBox)
            {
                ComboBox comboBox = control.AsComboBox();
                var itemAutomationElement = comboBox.Items.FirstOrDefault(i => i.FindFirstDescendant().AsTextBox().Text.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));
                if (itemAutomationElement == null) throw new InvalidOperationException($"Property '{propertyName}' has no value '{value}' among '{string.Join("', '", comboBox.Items.Select(i => i.FindFirstDescendant().AsTextBox().Text))}'");
                itemAutomationElement.IsSelected = true;
                comboBox.Collapse();
            }
            else if (control.Properties.ControlType == ControlType.Edit)
            {
                control.AsTextBox().EnterWithReturn(value);
            }
            else if (control.Properties.ControlType == ControlType.CheckBox)
            {
                control.AsCheckBox().IsChecked = (bool)value;
            }
            else
            {
                throw new InvalidOperationException($"No support for the control type '{control.Properties.ControlType.Value}'");
            }
        }

        public string GetProperty(string propertyName)
        {
            var control = FindPropertyControl(propertyName, false);
            if (control.Properties.ControlType == ControlType.ComboBox)
            {
                ComboBox comboBox = control.AsComboBox();
                return comboBox.SelectedItem.FindFirstDescendant().AsTextBox().Text;
            }
            else if (control.Properties.ControlType == ControlType.CheckBox)
            {
                return control.AsCheckBox().ToggleState == ToggleState.On ? "True" : "False";
            }
            else
            {
                return control.AsTextBox().Text;
            }
        }
        public double GetPropertyAsDouble(string key)
        {
            return double.Parse(GetProperty(key), appCultureInfo);
        }
        public int GetPropertyAsInt(string key)
        {
            return int.Parse(GetProperty(key), appCultureInfo);
        }
        public bool GetPropertyAsBool(string key)
        {
            return bool.Parse(GetProperty(key));
        }

        public bool IsLocked
        {
            get { return lockButton.Value.ToggleState == ToggleState.On; }
            set
            {
                ToggleState state = lockButton.Value.ToggleState;
                if (value && state == ToggleState.Off || !value && state == ToggleState.On)
                {
                    lockButton.Value.Toggle();
                }
            }
        }
        public bool IsVisible
        {
            get { return GetProperty("Visible").Equals("True"); }
            set { SetProperty("Visible", value );}
        }
    }
}
