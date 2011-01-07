﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionAppendToLineEnd : AbstractVimEditionInsertText
    {
        public EditionAppendToLineEnd(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert()
        {
            this.Host.MoveToEndOfLine();
        }
    }
}
