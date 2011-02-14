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

        public override bool Apply(Interfaces.IVimHost host)
        {
            VimSpan span = null;
            if (this.Repeat == 1) {
                VimPoint from = new VimPoint(host.CurrentPosition.X, 0);
                VimPoint to = host.CurrentLineEndPosition;
                span = new VimSpan(from, to);
            }
            else {
                VimPoint from = new VimPoint(host.CurrentPosition.X, 0);
                int dst_line = Math.Min(from.X + this.Repeat - 1, host.TextLineCount - 1);
                VimPoint to = host.GetLineEndPosition(dst_line);
                span = new VimSpan(from, to);
            }

            _register.Remember(host.GetText(span), true, host);

            return true;
        }
    }
}
