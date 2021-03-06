﻿using System;
using System.Collections.Generic;

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

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            host.GotoMatch();

            return host.CurrentPosition;
        }
    }
}
