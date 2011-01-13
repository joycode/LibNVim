using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionOpenLineBlow : AbstractVimEditionInsertText
    {
        public EditionOpenLineBlow(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert()
        {
            this.Host.OpenLineBelow();
        }
    }
}
