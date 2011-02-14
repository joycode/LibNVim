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
            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, host.CurrentLineEndPosition.Y - 1));

            VimSpan span = new VimSpan(from, to);
            VimRegister.YankRangeToDefaultRegister(host, span);
            host.DeleteRange(span);
        }
    }
}
