using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    class EditionDeleteRange : AbstractVimRangeEdition
    {
        public EditionDeleteRange(IVimHost host, int repeat, IVimMotion motion)
            : base(host, repeat, motion)
        {
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = this.Motion.Move();

            if (this.Motion is Interfaces.IVimMotionWordWise) {
                if (this.Host.IsCurrentPositionAtStartOfLineText()) {
                    this.Host.CaretUp();
                    this.Host.MoveToEndOfLine();
                    to = this.Host.CurrentPosition;
                }
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfLine) {
                // 单独处理 '$': 实际位置与要做的操作偏左一个字符位置
                // 由于 Vim 中除非空行, 末尾光标永远比 LineEnd 少一位, 所以需要额外右移一位
                this.Host.CaretRight();
                to = this.Host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                if (this.Motion is IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(from, to);
                }
                else {
                    this.Host.DeleteRange(from, to);
                }
            }

            if (this.Host.IsCurrentPositionAtEndOfLine())
            {
                this.Host.CaretLeft();
            }

            return true;
        }
    }
}
