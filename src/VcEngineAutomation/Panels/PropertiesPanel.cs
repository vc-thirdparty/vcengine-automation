using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using VcEngineAutomation.Extensions;
using VcEngineAutomation.Utils;

namespace VcEngineAutomation.Panels
{
    public class PropertiesPanel
    {
        private readonly Lazy<ToggleButton> lockButton;
        private readonly VcEngine vcEngine;
        private readonly IFormatProvider appCultureInfo;

        public PropertiesPanel(VcEngine vcEngine, bool isInDrawingContext)
        {
            this.vcEngine = vcEngine;
            Pane = vcEngine.IsR7OrAbove
                ? new DockedTabRetriever(vcEngine.MainWindow).GetPane("VcPropertyEditor")
                : new DockedTabRetriever(vcEngine.MainWindow).GetPane(isInDrawingContext ? "Drawing Properties" : "Component Properties", "VcPropertyEditorContentPane");
            lockButton = new Lazy<ToggleButton>(() => Pane.FindFirstDescendant(cf => cf.ByAutomationId(vcEngine.IsR7OrAbove ? "Property.LockButton" : "LockButton1")).AsToggleButton());
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
                if (!vcEngine.IsR7OrAbove)
                {
                    // Hack for R5
                    HeaderLabel.LeftClick();
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
                if (!vcEngine.IsR7OrAbove)
                {
                    // Hack for R5
                    HeaderLabel.LeftClick();
                }
            }
        }

        public void ClickTab(string title)
        {
            Tab tab = Pane.FindFirstDescendant(cf => cf.ByAutomationId("TabsListView")).FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab)).AsTab();
            TabItem item = null;
            foreach (TabItem tabItem in tab.TabItems)
            {
                if (tabItem.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsLabel().Text == title)
                {
                    item = tabItem;
                }
            }
            if (item == null) throw new InvalidOperationException($"No tab item found with title {title}");
            item.Select();
        }

        private void SetProperty(string tab, string propertyName, object value)
        {
            AutomationElement item = Pane.FindFirstDescendant(cf => cf.ByAutomationId("TabsListView"));
            if (item.IsVisible())
            {
                ClickTab(tab);
            }
            AutomationElement control = item.GetItemForLabel(propertyName);
            if (!control.IsEnabled) throw new InvalidOperationException($"Property '{tab}::{propertyName}' is not enabled");
            if (control.Properties.ControlType == ControlType.ComboBox)
            {
                ComboBox comboBox = control.AsComboBox();
                var itemAutomationElement = comboBox.Items.FirstOrDefault(i => i.FindFirstDescendant().AsTextBox().Text.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));
                if (itemAutomationElement == null) throw new InvalidOperationException($"Property '{tab}::{propertyName}' has no value '{value}' among '{string.Join("', '", comboBox.Items.Select(i => i.FindFirstDescendant().AsTextBox().Text))}'");
                itemAutomationElement.IsSelected = true;
                comboBox.Collapse();
            }
            else if (control.Properties.ControlType == ControlType.Edit)
            {
                control.AsTextBox().EnterWithReturn(value);
                if (!vcEngine.IsR7OrAbove)
                {
                    // Hack for R5
                    HeaderLabel.LeftClick();
                }
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

        public void SetProperty(string propertyName, object value)
        {
            string[] strings = propertyName.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 1)
            {
                SetProperty(strings[0], strings[1], value);
            }
            else
            {
                SetProperty("Default", propertyName, value);
            }
        }

        public string GetProperty(string propertyName)
        {
            string[] strings = propertyName.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 1)
            {
                return GetProperty(strings[0], strings[1]);
            }
            else
            {
                return GetProperty("Default", propertyName);
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
        

        public string GetProperty(string tabName, string propertyName)
        {
            AutomationElement item = Pane.FindFirstDescendant(cf => cf.ByAutomationId("TabsListView"));
            AutomationElement parent;
            if (item.IsVisible())
            {
                Tab tab = item.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tab)).AsTab();
                ClickTab(tabName);
                parent = tab;
            }
            else
            {
                parent = Pane.FindFirstDescendant(cf => cf.ByAutomationId("ControlsListView"));
            }
            
            AutomationElement control = parent.GetItemForLabel(propertyName);
            if (control.Properties.ControlType == ControlType.ComboBox)
            {
                ComboBox comboBox = control.AsComboBox();
                if (!comboBox.IsEnabled) throw new InvalidOperationException($"Property '{tabName}::{propertyName}' is not enabled");
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
            get { return GetProperty("Default", "Visible").Equals("True"); }
            set { SetProperty("Default", "Visible", value );}
        }
    }
}
