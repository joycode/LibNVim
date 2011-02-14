using System;
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

        public override bool Apply(Interfaces.IVimHost host)
        {
            string reg_text = _register.GetText(host);
            if (reg_text == null) {
                return false;
            }

            if (_register.IsTextLines) {
                string text = reg_text;
                for (int i = 0; i < (this.Repeat - 1); i++) {
                    text = text + host.LineBreak + reg_text;
                }

                host.OpenLineBelow();
                host.InsertTextAtCurrentPosition(text);
            }
            else {
                string text = "";
                for (int i = 0; i < this.Repeat; i++) {
                    text = text + reg_text;
                }

                host.CaretRight();
                host.InsertTextAtCurrentPosition(text);
            }

            if (host.IsCurrentPositionAtEndOfLine()) {
                host.MoveToEndOfLine();
                host.CaretLeft();
            }

            return true;
        }
    }
}
