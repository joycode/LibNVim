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
            VimPoint to = new VimPoint(this.Host.CurrentPosition.X,
                Math.Min(this.Host.CurrentPosition.Y + this.Repeat - 1, this.Host.CurrentLineEndPosition.Y - 1));
            this.Host.DeleteRange(this.Host.CurrentPosition, to);
        }
    }
}
