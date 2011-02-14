using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionInsert : AbstractVimEditionInsertText
    {
        public EditionInsert(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
        {
        }
    }
}
