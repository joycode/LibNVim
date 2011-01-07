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
        /// <summary>
        /// record location key input 
        /// </summary>
        private enum LastValidLocationAction { None, Enter, Tab, Backspace };

        private VimSpan _editionRegion = null;
        private LastValidLocationAction _lastLocationAction = LastValidLocationAction.None;

        public IVimHost Host { get; private set; }
        /// <summary>
        /// the reseaon for entering Insert Mode 
        /// </summary>
        public AbstractVimEditionInsertText CausedEdition { get; private set; }
        public VimCaretShape CaretShape { get { return VimCaretShape.NormalCaret; } }

        public IVimMode PreviousMode { get; private set; }

        public ModeInsert(IVimHost host, IVimMode previousMode, AbstractVimEditionInsertText causedEdition)
        {
            _editionRegion = new VimSpan(host.CurrentPosition, true, 
                host.CurrentPosition, false);

            this.Host = host;
            this.CausedEdition = causedEdition;
            this.PreviousMode = previousMode;
        }

        public virtual bool CanProcess(VimKeyInput keyInput)
        {
            if (keyInput.Value.Length == 1) {
                return false;
            }
            return true;
        }

        private void OnKeyImmediatelyAfterLastLocationAction()
        {
            _lastLocationAction = LastValidLocationAction.None;

            if (this.Host.CurrentPosition.CompareTo(_editionRegion.Start) < 0) {
                return;
            }

            // extends edition region regarding to last location action
            _editionRegion = new VimSpan(_editionRegion.Start, _editionRegion.StartClosed, 
                this.Host.CurrentPosition, false);
        }

        /// <summary>
        /// mayby too dedicated to VS's editor
        /// </summary>
        /// <param name="args"></param>
        public virtual void KeyDown(VimKeyEventArgs args)
        {
            if (args.KeyInput.Value == VimKeyInput.Escape) {
                if (_lastLocationAction != LastValidLocationAction.None) {
                    this.OnKeyImmediatelyAfterLastLocationAction();
                }

                this.CausedEdition.Text = this.Host.GetText(_editionRegion);

                this.Host.CurrentMode = PreviousMode;
                this.Host.DismissDisplayWindows();

                // keep consistency with Vim
                new MotionCaretLeft(this.Host, 1).Move();

                args.Handled = true;
            }
            else if (args.KeyInput.Value == VimKeyInput.Enter) {
                if (this.Host.CurrentPosition.CompareTo(_editionRegion.End) == 0) {
                    _lastLocationAction = LastValidLocationAction.Enter;
                }
            }
            else if (args.KeyInput.Value == VimKeyInput.Tab) {
                if (this.Host.CurrentPosition.CompareTo(_editionRegion.End) == 0) {
                    _lastLocationAction = LastValidLocationAction.Tab;
                }
            }
            else if (args.KeyInput.Value == VimKeyInput.Backspace) {
                if (this.Host.CurrentPosition.CompareTo(_editionRegion.End) == 0) {
                    _lastLocationAction = LastValidLocationAction.Backspace;
                }
            }
            else if (args.KeyInput.Value.Length == 1) {
                if (_lastLocationAction != LastValidLocationAction.None) {
                    this.OnKeyImmediatelyAfterLastLocationAction();
                }
                VimPoint current_pos = this.Host.CurrentPosition;
                VimPoint next_pos = new VimPoint(current_pos.X, current_pos.Y + 1);
                if (current_pos.CompareTo(_editionRegion.End) == 0) {
                    _editionRegion = new VimSpan(_editionRegion.Start, _editionRegion.StartClosed,
                        next_pos, false);
                }
                else {
                    _editionRegion = new VimSpan(current_pos, true, next_pos, false);
                }
            }
            else {
                if (_lastLocationAction != LastValidLocationAction.None) {
                    this.OnKeyImmediatelyAfterLastLocationAction();
                }
            }
        }
    }
}
