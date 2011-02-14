using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    class EditionDeleteChar : AbstractVimEditionRedoable
    {
        public EditionDeleteChar(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtEndOfLine()) {
                return true;
            }

            VimPoint from = host.CurrentPosition;
            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, host.CurrentLineEndPosition.Y - 1));

            VimSpan span = new VimSpan(from, to);
            VimRegister.YankRangeToDefaultRegister(host, span);
            host.DeleteRange(span);

            return true;
        }
    }
}
