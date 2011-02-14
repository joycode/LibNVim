using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// Repeat stands for line number
    /// </summary>
    class MotionGotoLine : AbstractVimMotion
    {
        public MotionGotoLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            int dst_line = this.Repeat - 1;

            if (dst_line != host.CurrentPosition.X) {
                host.GotoLine(dst_line);
            }

            return host.CurrentPosition;
        }
    }
}
