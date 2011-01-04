using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToStartOfLineText : AbstractVimMotion, Interfaces.IVimMotionLineWise
    {
        public MotionMoveToStartOfLineText(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            this.Host.MoveToStartOfLineText();
            return this.Host.CurrentPosition;
        }
    }
}
