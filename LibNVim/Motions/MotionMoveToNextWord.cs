using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    /// <summary>
    /// 'w'
    /// </summary>
    class MotionMoveToNextWord : AbstractVimMotion, Interfaces.IVimMotionNextWord
    {
        public MotionMoveToNextWord(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            for (int i = 0; i < this.Repeat; i++) {
                host.MoveToNextWord();
            }

            return host.CurrentPosition;
        }
    }
}
