using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    /// <summary>
    /// 对应 'w'
    /// </summary>
    class MotionMoveToNextWord : AbstractVimMotion, Interfaces.IVimMotionWordWise
    {
        public MotionMoveToNextWord(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.MoveToNextWord();
            }

            return this.Host.CurrentPosition;
        }

        public override VimPoint MoveInRangeEdition()
        {
            return base.MoveInRangeEdition();
        }
    }
}
