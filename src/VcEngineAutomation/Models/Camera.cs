using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System;
using System.Linq;
using FlaUI.Core.Input;

namespace VcEngineAutomation.Models
{
    public class Camera
    {
        private readonly VcEngine vcEngine;
        private readonly Lazy<Button[]> cameraMoveButtons;

        public Camera(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
            cameraMoveButtons = new Lazy<Button[]>(() => 
                vcEngine.MainWindow.FindFirstDescendant(cf => cf.ByClassName("ViewSelectorArrowsView")).
                    FindAllChildren(cf => cf.ByControlType(ControlType.Button)).Select(ae => ae.AsButton()).ToArray());
        }

        public void RotateDown()
        {
            if (vcEngine.IsR7OrAbove) throw new NotImplementedException("Not implemented for changes in R7");
            cameraMoveButtons.Value[1].Click(true);
            Wait.UntilResponsive(cameraMoveButtons.Value[1]);
        }
        public void RotateUp()
        {
            if (vcEngine.IsR7OrAbove) throw new NotImplementedException("Not implemented for changes in R7");
            cameraMoveButtons.Value[0].Click(true);
            Wait.UntilResponsive(cameraMoveButtons.Value[0]);
        }
        public void RotateLeft()
        {
            if (vcEngine.IsR7OrAbove) throw new NotImplementedException("Not implemented for changes in R7");
            cameraMoveButtons.Value[2].Click(true);
            Wait.UntilResponsive(cameraMoveButtons.Value[2]);
        }
        public void RotateRight()
        {
            if (vcEngine.IsR7OrAbove) throw new NotImplementedException("Not implemented for changes in R7");
            cameraMoveButtons.Value[3].Click(true);
            Wait.UntilResponsive(cameraMoveButtons.Value[3]);
        }

        /*public void ViewFront()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Front");
        }
        public void ViewBottom()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Bottom");
        }
        public void ViewBack()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Back");
        }
        public void ViewTop()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Top");
        }
        public void ViewLeft()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Left");
        }
        public void ViewRight()
        {
            vcEngine.Ribbon.CoreAutomationTab.SelectDropdownItem("View", "Change", "Right");
        }

        public void ViewIso()
        {
            ViewFront();
            RotateLeft();
            RotateUp();
        }*/

        public void FillView()
        {
            vcEngine.Visual3DToolbar.FillView();
        }

        public void FillViewOnSelected()
        {
            vcEngine.Visual3DToolbar.FillOnSelected();
        }
    }
}
