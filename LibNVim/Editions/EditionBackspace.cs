using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionBackspace : AbstractVimEditionRedoable
    {
        public EditionBackspace(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            if (this.Host.CurrentPosition.Y == 0) {
                return true;
            }

            VimPoint to = new VimPoint(this.Host.CurrentPosition.X,
                Math.Max(this.Host.CurrentPosition.Y - this.Repeat, 0));
            this.Host.DeleteRange(this.Host.CurrentPosition, to);
            return true;
        }
    }
}
