﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionAppendToLineEnd : AbstractVimEditionRedoable
    {
        public EditionAppendToLineEnd(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            this.Host.MoveToEndOfLine();
            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}