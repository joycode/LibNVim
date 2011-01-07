using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    /// <summary>
    /// Repeat stands for line number
    /// </summary>
    class MotionScrollLineCenter : AbstractVimMotion
    {
        public MotionScrollLineCenter(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            int dst_line = this.Repeat - 1;
            this.Host.GotoLine(dst_line);

            this.Host.ScrollLineCenter();

            return this.Host.CurrentPosition;
        }
    }
}
