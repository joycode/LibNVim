﻿using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionInsertToLineStart : AbstractVimEditionInsertText
    {
        public EditionInsertToLineStart(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
        {
            host.MoveToStartOfLineText();
        }
    }
}
