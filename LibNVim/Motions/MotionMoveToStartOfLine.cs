using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToStartOfLine : AbstractVimMotion, Interfaces.IVimMotionLineWise
    {
        public MotionMoveToStartOfLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            host.MoveToStartOfLine();
            return host.CurrentPosition;
        }
    }
}
