using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionRepeatLastEdition : AbstractVimEdition
    {
        public EditionRepeatLastEdition(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            if (this.Host.LastEdition != null) {
                for (int i = 0; i < this.Repeat; i++) {
                    if (!this.Host.LastEdition.Apply()) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
