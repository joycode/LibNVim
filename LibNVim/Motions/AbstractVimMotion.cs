using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    abstract class AbstractVimMotion : IVimMotion
    {

        public IVimHost Host { get; private set; }
        public int Repeat { get; private set; }

        public AbstractVimMotion(IVimHost host, int repeat)
        {
            this.Host = host;
            this.Repeat = repeat;
        }

        public abstract VimPoint Move();

        public virtual VimPoint MoveInRangeEdition()
        {
            return this.Move();
        }

        public virtual VimPoint TestMove()
        {
            VimPoint old = this.Host.CurrentPosition;
            VimPoint ret = this.Move();
            this.Host.MoveCursor(old);

            return ret;
        }

    }
}
