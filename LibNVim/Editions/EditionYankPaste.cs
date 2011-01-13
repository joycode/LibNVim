﻿using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionYankPaste : AbstractVimEditionRedoable
    {
        private VimRegister _register = null;

        public EditionYankPaste(VimRegister register, Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
            _register = register;
        }

        public override bool Apply()
        {
            string reg_text = _register.GetText(this.Host);
            if (reg_text == null) {
                return false;
            }

            if (_register.IsTextLines) {
                string text = reg_text;
                for (int i = 0; i < (this.Repeat - 1); i++) {
                    text = text + this.Host.LineBreak + reg_text;
                }

                this.Host.OpenLineBelow();
                this.Host.InsertTextAtCurrentPosition(text);
            }
            else {
                string text = "";
                for (int i = 0; i < this.Repeat; i++) {
                    text = text + reg_text;
                }

                this.Host.CaretRight();
                this.Host.InsertTextAtCurrentPosition(text);
            }

            if (this.Host.IsCurrentPositionAtEndOfLine()) {
                this.Host.MoveToEndOfLine();
                this.Host.CaretLeft();
            }

            return true;
        }
    }
}
