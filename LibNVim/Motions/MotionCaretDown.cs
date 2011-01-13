﻿using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    class MotionCaretDown : AbstractVimMotion, IVimMotionBetweenLines
    {

        public MotionCaretDown(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.CaretDown();
            }

            return this.Host.CurrentPosition;
        }

    }
}
