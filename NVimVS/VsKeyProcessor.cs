﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows.Input;
using LibNVim;
using LibNVim.Interfaces;

namespace NVimVS
{
    public class VsKeyProcessor : KeyProcessor
    {
        private IVimHost _host = null;

        public override bool IsInterestedInHandledEvents
        {
            get { return true; }
        }

        public VsKeyProcessor(IVimHost host)
        {
            _host = host;
        }

        public override void TextInput(TextCompositionEventArgs args)
        {
            try {
                VimKeyEventArgs vim_args = new VimKeyEventArgs(new VimKeyInput(args.Text));

                if (!String.IsNullOrEmpty(args.Text) && 1 == args.Text.Length) {
                    // Only want to intercept text coming from the keyboard.  Let other 
                    // components edit without having to come through us
                    var keyboard = args.Device as KeyboardDevice;
                    if (keyboard != null) {
                        if (_host.CanProcess(vim_args.KeyInput)) {
                            _host.KeyDown(vim_args);
                        }
                    }
                }

                if (vim_args.Handled) {
                    args.Handled = true;
                }
                else {
                    base.TextInput(args);
                }
            }
            catch (Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
#endif
            }
        }

        public override void KeyDown(KeyEventArgs args)
        {
            base.KeyDown(args);
        }
    }
}
