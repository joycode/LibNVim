using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    class MotionCaretUp : AbstractVimMotion, IVimMotionBetweenLines
    {

        public MotionCaretUp(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.CaretUp();
            }

            return host.CurrentPosition;
        }

    }
}
