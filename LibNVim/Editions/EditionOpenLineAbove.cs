using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionOpenLineAbove : AbstractVimEditionInsertText
    {
        public EditionOpenLineAbove(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
        {
            host.OpenLineAbove();
        }
    }
}
