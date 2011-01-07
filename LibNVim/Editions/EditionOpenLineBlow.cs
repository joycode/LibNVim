using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionOpenLineBlow : AbstractVimEditionInsertText
    {
        public EditionOpenLineBlow(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert()
        {
            this.Host.OpenLineBelow();
        }

        /// <summary>
        /// bugfix: after Redo(), Cursor ends at a wrong position, manual moves it to the end of the line
        /// </summary>
        /// <returns></returns>
        public override bool Redo()
        {
            bool res = base.Redo();
            if (res) {
                new Motions.MotionMoveToEndOfLine(this.Host, 1).Move();
            }

            return res;
        }
    }
}
