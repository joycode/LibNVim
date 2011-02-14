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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            if (this.Repeat == 1) {
                host.MoveToEndOfDocument();
            }
            return host.CurrentPosition;
        }
    }
}
