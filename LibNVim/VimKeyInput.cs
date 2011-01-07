using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimKeyInput
    {
        public static string Escape = "<Esc>";
        public static string Enter = "<Enter>";
        public static string Tab = "<Tab>";
        public static string Backspace = "<Backspace>";
        public static string Delete = "<Delete>";
        public static string Arrow_Left = "<Left>";
        public static string Arrow_Right = "<Right>";
        public static string Arrow_Up = "<Up>";
        public static string Arrow_Down = "<Down>";
        public static string Page_Up = "<PageUp>";
        public static string Page_Down = "<PageDown>";

        public string Value { get; private set; }

        public VimKeyInput(string value)
        {
            this.Value = value;
        }
    }
}
