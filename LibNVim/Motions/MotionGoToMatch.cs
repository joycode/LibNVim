using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionGoToMatch : AbstractVimMotion
    {
        private static char[] Left_Brackets = { '(', '[', '{' };

        public MotionGoToMatch(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            this.Host.GotoMatch();

            return this.Host.CurrentPosition;
        }
    }
}
