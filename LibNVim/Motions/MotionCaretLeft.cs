using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    class MotionCaretLeft : AbstractVimMotion
    {

        public MotionCaretLeft(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.CaretLeft();
            }

            return this.Host.CurrentPosition;
        }

    }
}
