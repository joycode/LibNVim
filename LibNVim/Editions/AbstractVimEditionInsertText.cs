using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    abstract class AbstractVimEditionInsertText : AbstractVimEditionRedoable
    {
        public string Text { get; set; }

        protected AbstractVimEditionInsertText(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected abstract void OnBeforeInsert();

        public override bool Apply()
        {
            this.OnBeforeInsert();
            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }

        public override bool Redo()
        {
            this.OnBeforeInsert();
            this.Host.InsertTextAtCurrentPosition(this.Text);
            return true;
        }
    }
}
