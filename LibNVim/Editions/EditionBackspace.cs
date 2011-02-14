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

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtStartOfLine()) {
                return true;
            }

            VimPoint from = host.CurrentPosition;
            VimPoint to = new VimPoint(from.X, Math.Max(from.Y - this.Repeat, 0));

            VimSpan span = new VimSpan(to, from);
            VimRegister.YankRangeToDefaultRegister(host, span);
            host.DeleteRange(span);

            return true;
        }
    }
}
