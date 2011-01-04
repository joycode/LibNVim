using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeRange : AbstractVimRangeEdition
    {
        public EditionChangeRange(Interfaces.IVimHost host, int repeat, Interfaces.IVimMotion motion)
            : base(host, repeat, motion)
        {
        }

        public override bool Apply()
        {
            VimPoint from = this.Host.CurrentPosition;
            VimPoint to = this.Motion.Move();

            if (this.Motion is Interfaces.IVimMotionWordWise)
            {
                this.Host.MoveToPreviousWord();
                this.Host.MoveToEndOfWord();
                to = new VimPoint(this.Host.CurrentPosition.X, this.Host.CurrentPosition.Y + 1);
            }
            else if (this.Motion is Interfaces.IVimMotionEndOfLine) {
                // 单独处理 '$': 实际位置与要做的操作偏左一个字符位置
                // 由于 Vim 中除非空行, 末尾光标永远比 LineEnd 少一位, 所以需要额外右移一位
                this.Host.CaretRight();
                to = this.Host.CurrentPosition;
            }

            for (int i = 0; i < this.Repeat; i++) {
                if (this.Motion is Interfaces.IVimMotionBetweenLines) {
                    this.Host.DeleteLineRange(from, to);
                }
                else {
                    this.Host.DeleteRange(from, to);
                }
            }

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
