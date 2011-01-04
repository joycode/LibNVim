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
            VimPoint to = new VimPoint(this.Host.CurrentPosition.X,
                Math.Min(this.Host.CurrentPosition.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));
            this.Host.DeleteRange(this.Host.CurrentPosition, to);
            return true;
        }
    }
}
