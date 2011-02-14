using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToPreviousWord : AbstractVimMotion
    {
        public MotionMoveToPreviousWord(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.MoveToPreviousWord();
            }

            return host.CurrentPosition;
        }
    }
}
