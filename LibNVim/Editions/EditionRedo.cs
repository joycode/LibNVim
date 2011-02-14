using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    class EditionRedo : AbstractVimEdition
    {
        public EditionRedo(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply(Interfaces.IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.Redo();
            }

            return true;
        }
    }
}
