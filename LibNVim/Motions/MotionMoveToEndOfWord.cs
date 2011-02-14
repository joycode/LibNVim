using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToEndOfWord : AbstractVimMotion, Interfaces.IVimMotionEndOfWord
    {
        public MotionMoveToEndOfWord(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.MoveToEndOfWord();
            }

            return host.CurrentPosition;
        }
    }
}
