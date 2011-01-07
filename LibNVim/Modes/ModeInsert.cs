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
        /// special key input to deal with
        /// </summary>
        private enum NoneCharKeyInputType { None, Unsupport, Enter, Tab, Backspace};

        /// <summary>
        /// effective insert region, in order to track text inputed
        /// </summary>
        private VimSpan _insertArea = null;
        private NoneCharKeyInputType _activeNoneCharKeyInput = NoneCharKeyInputType.None;

        public IVimHost Host { get; private set; }
        public VimCaretShape CaretShape { get { return VimCaretShape.NormalCaret; } }

        /// <summary>
        /// the reseaon for entering Insert Mode 
        /// </summary>
        public AbstractVimEditionInsertText CausedEdition { get; private set; }
        /// <summary>
        /// mode before entering insert mode, when quit insert mode, we need to retore to this mode
        /// </summary>
        public IVimMode PreviousMode { get; private set; }

        public ModeInsert(IVimHost host, IVimMode previousMode, AbstractVimEditionInsertText causedEdition)
        {
            _insertArea = new VimSpan(host.CurrentPosition, host.CurrentPosition);

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

        private void OnKeyInputImmediatelyAfterLastNoneCharKeyInput()
        {
            if (this.Host.CurrentPosition.CompareTo(_insertArea.Start) < 0) {
                return;
            }

            // extends edition region regarding to last location action
            _insertArea = _insertArea.ExtendEndTo(this.Host.CurrentPosition);
        }

        /// <summary>
        /// mayby too dedicated to VS's editor
        /// </summary>
        /// <param name="args"></param>
        public virtual void KeyDown(VimKeyEventArgs args)
        {
            // important for dealing some unexpected scene
            bool is_unsupported_key_last_input = false;

            if (_activeNoneCharKeyInput != NoneCharKeyInputType.None) {
                if (_activeNoneCharKeyInput == NoneCharKeyInputType.Unsupport) {
                    is_unsupported_key_last_input = true;
                }
                else {
                    // extends edition region regarding to last supported special key input
                    _insertArea = _insertArea.ExtendEndTo(this.Host.CurrentPosition);
                }
                
                _activeNoneCharKeyInput = NoneCharKeyInputType.None;
            }

            if (args.KeyInput.Value.Length == 1) {
                // KeyDown event is fired up before text's change, so the real position after input should be one char right by current.
                VimPoint previous_pos = this.Host.CurrentPosition;
                VimPoint next_pos = new VimPoint(previous_pos.X, previous_pos.Y + 1);

                if (!is_unsupported_key_last_input) {
                    // in normally char input typing, simply extends insert area to current position
                    _insertArea = _insertArea.ExtendEndTo(next_pos);
                }
                else {
                    _insertArea = new VimSpan(previous_pos, next_pos);
                }
            }
            else if (args.KeyInput.Value == VimKeyInput.Escape) {
                if (!is_unsupported_key_last_input) {
                    // Escape after normal key/char input, do some extra checks
                    if (this.Host.CurrentPosition.CompareTo(_insertArea.End) != 0) {
                        // you can't expect to get precise input text here, because everything is under control, but something wrong happened
                        // so we can only do some poor guess: 
                        // insert area's Start wouldn't change, only need to extend area's End to current cursor's position
                        _insertArea = _insertArea.ExtendEndTo(this.Host.CurrentPosition);
                    }
                }

                this.CausedEdition.Text = this.Host.GetText(_insertArea);

                this.Host.CurrentMode = PreviousMode;
                this.Host.DismissDisplayWindows();

                // keep consistency with Vim
                new MotionCaretLeft(this.Host, 1).Move();

                args.Handled = true;
            }
            else {
                _activeNoneCharKeyInput = NoneCharKeyInputType.None;

                if (this.Host.CurrentPosition.CompareTo(_insertArea.End) == 0) {
                    // speical key input happens at the end of text input area, 
                    // we think it's a valid input, save it as a active special key input,
                    // and in the next loop, to deal with it
                    if (args.KeyInput.Value == VimKeyInput.Enter) {
                        _activeNoneCharKeyInput = NoneCharKeyInputType.Enter;
                    }
                    else if (args.KeyInput.Value == VimKeyInput.Tab) {
                        _activeNoneCharKeyInput = NoneCharKeyInputType.Tab;
                    }
                    else if (args.KeyInput.Value == VimKeyInput.Backspace) {
                        _activeNoneCharKeyInput = NoneCharKeyInputType.Backspace;
                    }
                    else {
                        _activeNoneCharKeyInput = NoneCharKeyInputType.Unsupport;
                    }
                }
            }
        }
    }
}
