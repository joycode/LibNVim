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

        public override VimPoint Move()
        {
            int dst_line = this.Repeat - 1;

            if (dst_line != this.Host.CurrentPosition.X) {
                this.Host.GotoLine(dst_line);
            }

            return this.Host.CurrentPosition;
        }
    }
}
