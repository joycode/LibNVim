using System;
using System.Collections.Generic;

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

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (VimGlobalInfo.LastEdition != null) {
                for (int i = 0; i < this.Repeat; i++) {
                    if (!VimGlobalInfo.LastEdition.Redo(host)) {
                        return false;
                    }
                }
            }

            if (host.IsCurrentPositionAtEndOfLine()) {
                host.MoveToEndOfLine();
                host.CaretLeft();
            }

            return true;
        }
    }
}
