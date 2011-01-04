using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToPreviousWord : AbstractVimMotion
    {
        public MotionMoveToPreviousWord(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.MoveToPreviousWord();
            }

            return this.Host.CurrentPosition;
        }
    }
}
