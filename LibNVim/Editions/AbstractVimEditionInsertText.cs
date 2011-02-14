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

        protected abstract void OnBeforeInsert(Interfaces.IVimHost host);

        public override bool Apply(Interfaces.IVimHost host)
        {
            this.OnBeforeInsert(host);
            Modes.ModeInsert mode = new Modes.ModeInsert(host, host.CurrentMode, this);
            host.CurrentMode = mode;
            return true;
        }

        public override bool Redo(Interfaces.IVimHost host)
        {
            this.OnBeforeInsert(host);
            host.InsertTextAtCurrentPosition(this.Text);
            return true;
        }
    }
}
