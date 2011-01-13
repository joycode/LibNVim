using System;
using System.Collections.Generic;

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

            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = new VimPoint(from.X, Math.Max(from.Y - this.Repeat, 0));

            VimSpan span = new VimSpan(to, from);
            VimRegister.YankRangeToDefaultRegister(this.Host, span);
            this.Host.DeleteRange(span);

            return true;
        }
    }
}
