using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Motions;
using LibNVim.Interfaces;
using LibNVim.Editions;

namespace LibNVim.Modes
{
    class ModeInsert : IVimMode
    {
        public IVimHost Host { get; private set; }
        /// <summary>
        /// the reseaon for entering Insert Mode 
        /// </summary>
        public AbstractVimEdition CausedEdition { get; private set; }
        public VimCaretShape CaretShape { get { return VimCaretShape.NormalCaret; } }

        public IVimMode PreviousMode { get; private set; }

        public ModeInsert(IVimHost host, IVimMode previousMode, AbstractVimEdition causedEdition)
        {
            this.Host = host;
            this.CausedEdition = causedEdition;
            this.PreviousMode = previousMode;
        }

        public void KeyDown(VimKeyEventArgs args)
        {
            if (args.KeyInput.Value != "Esc") {
                return;
            }

            this.Host.CurrentMode = PreviousMode;
            this.Host.DismissDisplayWindows();

            // keep consistency with Vim
            new MotionCaretLeft(this.Host, 1).Move();

            args.Handled = true;
        }
    }
}
