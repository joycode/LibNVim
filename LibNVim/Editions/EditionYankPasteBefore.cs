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
            if (_register.IsTextLines) {
                this.Host.OpenLineAbove();
                string text = _register.Text;
                for (int i = 0; i < (this.Repeat - 1); i++) {
                    text = text + this.Host.LineBreak + _register.Text;
                }
                this.Host.InsertTextAtCurrentPosition(text);
            }
            else {
                string text = "";
                for (int i = 0; i < this.Repeat; i++) {
                    text = text + _register.Text;
                }
                this.Host.InsertTextAtCurrentPosition(text);
            }

            return true;
        }
    }
}
