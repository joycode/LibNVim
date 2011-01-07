﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LibNVim.Interfaces;

namespace LibNVim.Modes
{
    class ModeNormal : IVimMode
    {
        private VimKeyInputEvaluation _keyInputEvaluation = null;

        public IVimHost Host { get; private set; }
        public VimCaretShape CaretShape { get { return VimCaretShape.Block; } }

        public ModeNormal(IVimHost host)
        {
            this.Host = host;
        }

        private bool DoEdition(IVimEdititon edition)
        {
            bool result = edition.Apply();
            if (result) {
                if (edition is IVimEditionRedoable) {
                    this.Host.LastEdition = edition as IVimEditionRedoable;
                }
            }

            return result;
        }

        public virtual bool CanProcess(VimKeyInput keyInput)
        {
            if (keyInput.Value.Length == 1) {
                // default is true, but when asked, say no
                return false;
            }
            else {
                // special command input
                if (keyInput.Value.Equals(VimKeyInput.Escape)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        public virtual void KeyDown(VimKeyEventArgs args)
        {
            bool handled = true;

            if (_keyInputEvaluation == null) {
                _keyInputEvaluation = new VimKeyInputEvaluation(this.Host);
            }

            IVimAction action = null;
            VimKeyInputEvaluation.KeyEvalState eval_state = _keyInputEvaluation.Evaluate(args.KeyInput, out action);
            if (eval_state == VimKeyInputEvaluation.KeyEvalState.Success) {
                if (action != null) {
                    // TODO live with some actions unimplemented
                    if (action is IVimMotion) {
                        (action as IVimMotion).Move();
                    }
                    else if (action is IVimEdititon) {
                        this.DoEdition(action as IVimEdititon);
                    }
                    else {
                        Debug.Assert(false);
                    }
                }

                _keyInputEvaluation = null;
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.Escape) {
                _keyInputEvaluation = null;
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.Error) {
                _keyInputEvaluation = null;
            }
            else if (eval_state == VimKeyInputEvaluation.KeyEvalState.InProcess) {
            }
            else {
                Debug.Assert(false);
                _keyInputEvaluation = null;
            }

            args.Handled = handled;
        }
    }
}
