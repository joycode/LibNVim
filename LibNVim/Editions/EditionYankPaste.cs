using System;
using System.Collections.Generic;
using System.Linq;
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
            if (_register.Text == null) {
                return false;
            }

            if (_register.IsTextLines) {
                string text = _register.Text;
                for (int i = 0; i < (this.Repeat - 1); i++) {
                    text = text + this.Host.LineBreak + _register.Text;
                }

                this.Host.OpenLineBelow();
                this.Host.InsertTextAtCurrentPosition(text);
            }
            else {
                string text = "";
                for (int i = 0; i < this.Repeat; i++) {
                    text = text + _register.Text;
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
