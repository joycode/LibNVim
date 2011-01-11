using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    /// <summary>
    /// the special '.' ;)
    /// </summary>
    class EditionDot : AbstractVimEdition
    {
        public EditionDot(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            if (this.Host.LastEdition != null) {
                for (int i = 0; i < this.Repeat; i++) {
                    if (!this.Host.LastEdition.Redo()) {
                        return false;
                    }
                }
            }

            if (this.Host.IsCurrentPositionAtEndOfLine()) {
                this.Host.MoveToEndOfLine();
                this.Host.CaretLeft();
            }

            return true;
        }
    }
}
