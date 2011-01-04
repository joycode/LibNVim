using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToEndOfWord : AbstractVimMotion, Interfaces.IVimMotionWordWise
    {
        public MotionMoveToEndOfWord(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.MoveToEndOfWord();
            }

            return this.Host.CurrentPosition;
        }
    }
}
