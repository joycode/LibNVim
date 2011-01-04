using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToEndOfLine : AbstractVimMotion, Interfaces.IVimMotionEndOfLine
    {
        public MotionMoveToEndOfLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < (this.Repeat - 1); i++) {
                this.Host.CaretDown();
            }

            this.Host.MoveToEndOfLine();
            this.Host.CaretLeft();
            return this.Host.CurrentPosition;
        }
    }
}
