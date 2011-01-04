using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeChar : AbstractVimEditionRedoable
    {
        public EditionChangeChar(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            VimPoint to = new VimPoint(this.Host.CurrentPosition.X,
                Math.Min(this.Host.CurrentPosition.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));
            this.Host.DeleteRange(this.Host.CurrentPosition, to);

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;

            return true;
        }
    }
}
