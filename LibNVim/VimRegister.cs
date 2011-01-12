using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimRegister
    {
        public readonly static VimRegister SystemRegister = new VimRegister("+");
        public readonly static VimRegister DefaultRegister = new VimRegister("0");

        /// <summary>
        /// yank to DefaultRegister
        /// </summary>
        /// <param name="host"></param>
        /// <param name="span"></param>
        public static void YankRangeToDefaultRegister(Interfaces.IVimHost host, VimSpan span)
        {
            VimRegister.DefaultRegister.Remember(host.GetText(span), false, host);
        }

        /// <summary>
        /// yank lines to DefaultRegister
        /// </summary>
        /// <param name="host"></param>
        /// <param name="span"></param>
        public static void YankLineToDefaultRegister(Interfaces.IVimHost host, VimSpan span)
        {
            VimPoint from = new VimPoint(span.Start.X, 0);
            VimPoint to = host.GetLineEndPosition(span.End.X);
            VimSpan line_span = new VimSpan(from, to);

            VimRegister.DefaultRegister.Remember(host.GetText(line_span), true, host);
        }

        private string _text = null;
        private bool _isTextLines = false;

        public string Name { get; private set; }
        public bool IsTextLines { get { return _isTextLines; } }

        public VimRegister(string name)
        {
            this.Name = name;
        }

        public string GetText(Interfaces.IVimHost host)
        {
            if (this.IsSystemRegister()) {
                return host.ClipboardText;
            }
            else {
                return _text;
            }
        }

        public void Remember(string text, bool isTextLines, Interfaces.IVimHost host)
        {
            if (this.IsSystemRegister()) {
                host.ClipboardText = text;
            }
            else {
                _text = text;
                _isTextLines = isTextLines;
            }
        }

        private bool IsSystemRegister()
        {
            return (this == SystemRegister);
        }
    }
}
