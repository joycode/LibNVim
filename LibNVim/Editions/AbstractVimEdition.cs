using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;
using System.Diagnostics;

namespace LibNVim.Editions
{
    abstract class AbstractVimEdition: IVimEdititon
    {
        public int Repeat { get; protected set; }

        public bool ModeSwitched { get; protected set; }
        public IVimMode NewMode { get; protected set; }

        protected AbstractVimEdition(IVimHost host, int repeat)
        {
            Debug.Assert(repeat != 0);

            this.Repeat = repeat;
            this.ModeSwitched = false;
            this.NewMode = null;
        }

        public abstract bool Apply(IVimHost host);
    }
}
