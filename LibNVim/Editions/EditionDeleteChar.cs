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

        public override bool Apply()
        {
            if (this.Host.IsCurrentPositionAtEndOfLine()) {
                return true;
            }

            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));

            VimSpan span = new VimSpan(from, to);
            VimRegister.YankRangeToDefaultRegister(this.Host, span);
            this.Host.DeleteRange(span);

            return true;
        }
    }
}
