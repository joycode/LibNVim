using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    abstract class AbstractVimEdition: IVimEdititon
    {
        public IVimHost Host { get; private set; }
        public int Repeat { get; private set; }

        public bool ModeSwitched { get; private set; }
        public IVimMode NewMode { get; private set; }

        public AbstractVimEdition(IVimHost host, int repeat)
        {
            this.Host = host;
            this.Repeat = repeat;
            this.ModeSwitched = false;
            this.NewMode = null;
        }

        public abstract bool Apply();

    }
}
