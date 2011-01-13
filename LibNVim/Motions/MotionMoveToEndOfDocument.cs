using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionMoveToEndOfDocument : AbstractVimMotion
    {
        public MotionMoveToEndOfDocument(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            if (this.Repeat == 1) {
                this.Host.MoveToEndOfDocument();
            }
            return this.Host.CurrentPosition;
        }
    }
}
