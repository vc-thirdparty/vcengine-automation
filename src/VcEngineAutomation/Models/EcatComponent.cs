using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Definitions;

namespace VcEngineAutomation.Models
{
    public class EcatComponent
    {
        public EcatComponent(AutomationElement automationElement)
        {
            this.AutomationElement = automationElement;
        }
        public AutomationElement AutomationElement { get; }


        public void Load()
        {
            AutomationElement.DoubleClick();
        }

        public string Name
        {
            get { return AutomationElement.FindFirstChild(cf => cf.ByControlType(ControlType.Text)).AsLabel().Text; }
        }
    }
}
