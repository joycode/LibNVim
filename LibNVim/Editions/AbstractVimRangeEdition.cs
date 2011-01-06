using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    abstract class AbstractVimRangeEdition : AbstractVimEditionRedoable, IVimRangeEdition
    {
        public IVimMotion Motion { get; protected set; }

        public AbstractVimRangeEdition(IVimHost host, int repeat, IVimMotion motion)
            : base(host, repeat)
        {
            this.Motion = motion;
        }
    }
}
