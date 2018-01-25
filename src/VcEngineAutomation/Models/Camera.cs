namespace VcEngineAutomation.Models
{
    public class Camera
    {
        private readonly VcEngine vcEngine;

        public Camera(VcEngine vcEngine)
        {
            this.vcEngine = vcEngine;
        }

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
