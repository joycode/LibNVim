using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionYankLine : AbstractVimEditionRedoable
    {
        private VimRegister _register = null;

        public EditionYankLine(VimRegister register, Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
            _register = register;
        }

        public override bool Apply()
        {
            VimSpan span = null;
            if (this.Repeat == 1) {
                VimPoint from = new VimPoint(this.Host.CurrentPosition.X, 0);
                VimPoint to = this.Host.CurrentLineEndPosition;
                span = new VimSpan(from, to);
            }
            else {
                VimPoint from = new VimPoint(this.Host.CurrentPosition.X, 0);
                int dst_line = Math.Min(from.X + this.Repeat - 1, this.Host.TextLineCount - 1);
                VimPoint to = this.Host.GetLineEndPosition(dst_line);
                span = new VimSpan(from, to);
            }

            _register.Remember(this.Host.GetText(span), true, this.Host);

            return true;
        }
    }
}
