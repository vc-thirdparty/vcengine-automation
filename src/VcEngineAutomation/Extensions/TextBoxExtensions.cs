using System;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;

namespace VcEngineAutomation.Extensions
{
    public static class TextBoxExtensions
    {
        public static void EnterWithReturn(this TextBox textbox, double value)
        {
            Retry.WhileException(() =>
            {
                textbox.Enter(value.ToString("F3", VcEngine.CultureInfo));
                Keyboard.Type(VirtualKeyShort.ENTER);
            }, VcEngine.DefaultTimeout, VcEngine.DefaultRetryInternal);
        }

        public static void EnterWithReturn(this TextBox textbox, object value)
        {
            Retry.WhileException(() =>
            {
                textbox.Enter(value.ToString());
                Keyboard.Type(VirtualKeyShort.ENTER);
            }, VcEngine.DefaultTimeout, VcEngine.DefaultRetryInternal);
        }
    }
}
