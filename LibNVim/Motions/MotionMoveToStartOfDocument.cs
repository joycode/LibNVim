using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToStartOfDocument : AbstractVimMotion
    {
        public MotionMoveToStartOfDocument(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            this.Host.MoveToStartOfDocument();
            return this.Host.CurrentPosition;
        }
    }
}
