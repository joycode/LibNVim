using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionYankPasteBefore : AbstractVimEditionRedoable
    {
        private VimRegister _register = null;

        public EditionYankPasteBefore(VimRegister register, Interfaces.IVimHost host, int repeat)
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
                this.Host.OpenLineAbove();
                string text = reg_text;
                for (int i = 0; i < (this.Repeat - 1); i++) {
                    text = text + this.Host.LineBreak + reg_text;
                }
                this.Host.InsertTextAtCurrentPosition(text);
            }
            else {
                string text = "";
                for (int i = 0; i < this.Repeat; i++) {
                    text = text + reg_text;
                }
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
