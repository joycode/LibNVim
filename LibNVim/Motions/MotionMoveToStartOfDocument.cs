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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            host.MoveToStartOfDocument();
            return host.CurrentPosition;
        }
    }
}
