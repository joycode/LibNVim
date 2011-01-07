using System;
using System.Collections.Generic;
using System.Linq;
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
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));

            this.Host.DeleteRange(new VimSpan(from, to));

            return true;
        }
    }
}
