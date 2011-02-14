using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;
using LibNVim.Interfaces;

namespace LibNVim.Modes
{
    class ModeNormal : IVimMode
    {
        private VimKeyInputEvaluation _keyInputEvaluation = null;
        private string _statusText = "";

        public IVimHost Host { get; private set; }
        public VimCaretShape CaretShape { get { return VimCaretShape.Block; } }

        public ModeNormal(IVimHost host)
        {
            this.Host = host;
        }

        private bool DoEdition(IVimEdititon edition)
        {
            bool result = edition.Apply(this.Host);
            if (result) {
                if (edition is IVimEditionRedoable) {
                    VimGlobalInfo.LastEdition = edition as IVimEditionRedoable;
                }
            }

            return result;
        }

        public virtual bool CanProcess(VimKeyInput keyInput)
        {
            return true;
        }

        private bool IsSpecialKeyInput(string keyInput)
        {
            return (keyInput.Length > 1);
        }

        private void ResetKeyValuationStatus()
        {
            _keyInputEvaluation = null;
            _statusText = "";
            this.Host.UpdateStatus(_statusText);
        }

        public virtual void KeyDown(VimKeyEventArgs args)
        {
            bool handled = true;

            if (_keyInputEvaluation == null) {
                _keyInputEvaluation = new VimKeyInputEvaluation(this.Host);
            }

            IVimAction action = null;
            string status_char = "";
            VimKeyInputEvaluation.KeyEvalState eval_state = _keyInputEvaluation.Evaluate(args.KeyInput, out action, out status_char);
            if (eval_state == VimKeyInputEvaluation.KeyEvalState.InProcess) {
                Debug.Assert(status_char != null);
                if (status_char == VimKeyInputEvaluation.Status_Char_Backspace) {
                    if (_statusText.Length > 0) {
                        _statusText = _statusText.Substring(0, _statusText.Length - 1);
                    }
                    else {
                        Debug.Assert(false);
                    }
                }
                else {
                    _statusText += status_char;
                }
                this.Host.UpdateStatus(_statusText);
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.Success) {
                if (action != null) {
                    // TODO live with some actions unimplemented
                    if (action is IVimMotion) {
                        (action as IVimMotion).Move(this.Host);
                    }
                    else if (action is IVimEdititon) {
                        this.DoEdition(action as IVimEdititon);
                    }
                    else {
                        Debug.Assert(false);
                    }
                }

                this.ResetKeyValuationStatus();
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.Escape) {
                this.ResetKeyValuationStatus();
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.Error) {
                if (this.IsSpecialKeyInput(args.KeyInput.Value)) {
                    handled = false;
                }
                this.ResetKeyValuationStatus();
            }
            else {
                Debug.Assert(false);
                this.ResetKeyValuationStatus();
            }

            args.Handled = handled;
        }
    }
}
