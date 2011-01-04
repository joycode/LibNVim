using System;
using System.Collections.Generic;
using System.Linq;
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

        public override bool Apply()
        {
            this.Host.Redo();

            return true;
        }
    }
}
