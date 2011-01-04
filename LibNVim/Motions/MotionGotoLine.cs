using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoLine : AbstractVimMotion
    {
        public MotionGotoLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            int dst_line = this.Repeat - 1;
            this.Host.GotoLine(dst_line);

            return this.Host.CurrentPosition;
        }
    }
}
