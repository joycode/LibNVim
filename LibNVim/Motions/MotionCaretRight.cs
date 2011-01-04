using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Motions
{
    class MotionCaretRight : AbstractVimMotion
    {
        public MotionCaretRight(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        /// <summary>
        /// 最多移到行(非空)尾的前一个字符位置
        /// </summary>
        /// <returns></returns>
        public override VimPoint Move()
        {
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.CaretRight();
            }

            if (this.Host.IsCurrentPositionAtEndOfLine())
            {
                // 如果移到了行尾, 则需要左移一位
                this.Host.CaretLeft();
            }

            return this.Host.CurrentPosition;
        }
    }
}
