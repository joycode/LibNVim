using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;
using System.Diagnostics;

namespace LibNVim.Motions
{
    abstract class AbstractVimMotion : IVimMotion
    {
        public int Repeat { get; private set; }

        protected AbstractVimMotion(IVimHost host, int repeat)
        {
            Debug.Assert(repeat != 0);

            this.Repeat = repeat;
        }

        public abstract VimPoint Move(IVimHost host);
    }
}
