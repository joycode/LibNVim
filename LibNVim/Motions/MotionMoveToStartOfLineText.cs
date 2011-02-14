using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToStartOfLineText : AbstractVimMotion, Interfaces.IVimMotionLineWise
    {
        public MotionMoveToStartOfLineText(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            host.MoveToStartOfLineText();
            return host.CurrentPosition;
        }
    }
}
