using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeChar : AbstractVimEditionInsertText
    {
        public EditionChangeChar(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = new VimPoint(from.X,
                Math.Min(from.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));

            this.Host.DeleteRange(new VimSpan(from, to));
        }
    }
}
