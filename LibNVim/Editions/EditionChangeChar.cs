using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeChar : AbstractVimEditionInsertText
    {
        public EditionChangeChar(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
        {
            VimPoint from = host.CurrentPosition;
            // when cursor is at the end of line, nothing to delete
            if (from.Y == host.CurrentLineEndPosition.Y) {
                return;
            }

            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, host.CurrentLineEndPosition.Y - 1));

            VimSpan span = new VimSpan(from, to).GetClosedEnd();
            VimRegister.YankRangeToDefaultRegister(host, span);
            host.DeleteRange(span);
        }
    }
}
