using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToEndOfLine : AbstractVimMotion, Interfaces.IVimMotionEndOfLine
    {
        public MotionMoveToEndOfLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            for (int i = 0; i < (this.Repeat - 1); i++) {
                host.CaretDown();
            }

            host.MoveToEndOfLine();
            host.CaretLeft();
            return host.CurrentPosition;
        }
    }
}
