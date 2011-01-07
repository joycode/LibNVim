using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using LibNVim;

namespace VsNVim
{
    /// <summary>
    /// This class needs to intercept commands which the core VIM engine wants to process and call into the VIM engine 
    /// directly.  It needs to be very careful to not double use commands that will be processed by the KeyProcessor.  In 
    /// general it just needs to avoid processing text input
    /// </summary>
    internal sealed class VsKeyProcessorAdditional : IOleCommandTarget
    {
        private readonly VsHost _host;
        private readonly IVsTextView _textView;
        private readonly IOleCommandTarget _nextTarget;
        private readonly System.IServiceProvider _serviceProvider;

        internal VsKeyProcessorAdditional(VsHost host, IVsTextView view,  System.IServiceProvider provider)
        {
            _host = host;
            _textView = view;
            _serviceProvider = provider;
            var hr = view.AddCommandFilter(this, out _nextTarget);
        }

        bool IsDebugIgnore(VSConstants.VSStd2KCmdID commandId)
        {
            switch (commandId) {
                // A lot of my debugging is essentially figuring out which command is messing up normal mode.
                // Unfortunately VS likes to throw a lot of commands all of the time.  I list them here so they don't
                // come through my default mode where I can then set a break point
                case VSConstants.VSStd2KCmdID.SolutionPlatform:
                case VSConstants.VSStd2KCmdID.FILESYSTEMEDITOR:
                case VSConstants.VSStd2KCmdID.REGISTRYEDITOR:
                case VSConstants.VSStd2KCmdID.FILETYPESEDITOR:
                case VSConstants.VSStd2KCmdID.USERINTERFACEEDITOR:
                case VSConstants.VSStd2KCmdID.CUSTOMACTIONSEDITOR:
                case VSConstants.VSStd2KCmdID.LAUNCHCONDITIONSEDITOR:
                case VSConstants.VSStd2KCmdID.EDITOR:
                case VSConstants.VSStd2KCmdID.VIEWDEPENDENCIES:
                case VSConstants.VSStd2KCmdID.VIEWFILTER:
                case VSConstants.VSStd2KCmdID.VIEWOUTPUTS:
                case VSConstants.VSStd2KCmdID.RENAME:
                case VSConstants.VSStd2KCmdID.ADDOUTPUT:
                case VSConstants.VSStd2KCmdID.ADDFILE:
                case VSConstants.VSStd2KCmdID.MERGEMODULE:
                case VSConstants.VSStd2KCmdID.ADDCOMPONENTS:
                case VSConstants.VSStd2KCmdID.ADDWFCFORM:
                    return true;
            }

            return false;
        }

        bool IsDebugIgnore(VSConstants.VSStd97CmdID commandId)
        {
            switch (commandId) {
                case VSConstants.VSStd97CmdID.SolutionCfg:
                case VSConstants.VSStd97CmdID.SearchCombo:
                    return true;
            }

            return false;
        }

        bool IsDebugIgnore(Guid commandGroup, uint commandId)
        {
            if (VSConstants.VSStd2K == commandGroup) {
                return this.IsDebugIgnore((VSConstants.VSStd2KCmdID)commandId);
            }
            else if (VSConstants.GUID_VSStandardCommandSet97 == commandGroup) {
                return this.IsDebugIgnore((VSConstants.VSStd97CmdID)commandId);
            }

            return false;
        }

        bool TryConvert(Guid commandGroup, uint commandId, IntPtr pVariableIn, out VimKeyInput ki)
        {
            if (VSConstants.GUID_VSStandardCommandSet97 == commandGroup) {
                return TryConvert((VSConstants.VSStd97CmdID)commandId, pVariableIn, out ki);
            }
            else if (VSConstants.VSStd2K == commandGroup) {
                return TryConvert((VSConstants.VSStd2KCmdID)commandId, pVariableIn, out ki);
            }
            else {
                ki = null;
                return false;
            }
        }

        bool TryConvert(VSConstants.VSStd97CmdID cmdId, IntPtr pVariantIn, out VimKeyInput ki)
        {
            ki = null;

            switch (cmdId) {
                case VSConstants.VSStd97CmdID.SingleChar:
                    break;
                case VSConstants.VSStd97CmdID.Escape:
                    ki = new VimKeyInput(VimKeyInput.Escape);
                    break;
                case VSConstants.VSStd97CmdID.Delete:
                    ki = new VimKeyInput(VimKeyInput.Delete);
                    break;
                case VSConstants.VSStd97CmdID.F1Help:
                    break;
            }

            return ki != null;
        }

        bool TryConvert(VSConstants.VSStd2KCmdID cmdId, IntPtr pVariantIn, out VimKeyInput ki)
        {
            ki = null;

            switch (cmdId) {
                case VSConstants.VSStd2KCmdID.TYPECHAR:
                    break;
                case VSConstants.VSStd2KCmdID.RETURN:
                    ki = new VimKeyInput(VimKeyInput.Enter);
                    break;
                case VSConstants.VSStd2KCmdID.CANCEL:
                    ki = new VimKeyInput(VimKeyInput.Escape);
                    break;
                case VSConstants.VSStd2KCmdID.DELETE:
                    ki = new VimKeyInput(VimKeyInput.Delete);
                    break;
                case VSConstants.VSStd2KCmdID.BACKSPACE:
                    ki = new VimKeyInput(VimKeyInput.Backspace);
                    break;
                case VSConstants.VSStd2KCmdID.LEFT:
                case VSConstants.VSStd2KCmdID.LEFT_EXT:
                case VSConstants.VSStd2KCmdID.LEFT_EXT_COL:
                    ki = new VimKeyInput(VimKeyInput.Arrow_Left);
                    break;
                case VSConstants.VSStd2KCmdID.RIGHT:
                case VSConstants.VSStd2KCmdID.RIGHT_EXT:
                case VSConstants.VSStd2KCmdID.RIGHT_EXT_COL:
                    ki = new VimKeyInput(VimKeyInput.Arrow_Right);
                    break;
                case VSConstants.VSStd2KCmdID.UP:
                case VSConstants.VSStd2KCmdID.UP_EXT:
                case VSConstants.VSStd2KCmdID.UP_EXT_COL:
                    ki = new VimKeyInput(VimKeyInput.Arrow_Up);
                    break;
                case VSConstants.VSStd2KCmdID.DOWN:
                case VSConstants.VSStd2KCmdID.DOWN_EXT:
                case VSConstants.VSStd2KCmdID.DOWN_EXT_COL:
                    ki = new VimKeyInput(VimKeyInput.Arrow_Down);
                    break;
                case VSConstants.VSStd2KCmdID.TAB:
                    ki = new VimKeyInput(VimKeyInput.Tab);
                    break;
                case VSConstants.VSStd2KCmdID.PAGEDN:
                case VSConstants.VSStd2KCmdID.PAGEDN_EXT:
                    ki = new VimKeyInput(VimKeyInput.Page_Down);
                    break;
                case VSConstants.VSStd2KCmdID.PAGEUP:
                case VSConstants.VSStd2KCmdID.PAGEUP_EXT:
                    ki = new VimKeyInput(VimKeyInput.Page_Up);
                    break;
                default:
                    break;
            }

            return ki != null;
        }

        bool TriggerEscapeKey(Guid commandGroup, uint commandId, IntPtr pvaIn)
        {
            // Don't ever process a command when we are in an automation function.  Doing so will cause VsVim to 
            // intercept items like running Macros and certain wizard functionality
            if (VsShellUtilities.IsInAutomationFunction(_serviceProvider)) {
                return false;
            }

            bool triggered = false;

            VimKeyInput key_input = null;
            if (this.TryConvert(commandGroup, commandId, pvaIn, out key_input)) {
                if (key_input.Value == VimKeyInput.Escape) {
                    triggered = true;
                }
            }

            return triggered;
        }

        #region IOleCommandTarget implementation

        int IOleCommandTarget.Exec(ref Guid commandGroup, uint commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            try {
                if (commandGroup == VSConstants.VSStd2K) {
                    switch ((VSConstants.VSStd2KCmdID)commandId) {
                        case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
                        case VSConstants.VSStd2KCmdID.SnippetProp:
                        case VSConstants.VSStd2KCmdID.SnippetRef:
                        case VSConstants.VSStd2KCmdID.SnippetRepl:
                        case VSConstants.VSStd2KCmdID.ECMD_INVOKESNIPPETFROMSHORTCUT:
                        case VSConstants.VSStd2KCmdID.ECMD_CREATESNIPPET:
                        case VSConstants.VSStd2KCmdID.ECMD_INVOKESNIPPETPICKER2:
                            break;
                    }
                }

                bool handled = true;
                VimKeyInput key_input = null;

                if (this.IsDebugIgnore(commandGroup, commandId)) {
                    handled = false;
                }
                else if (!this.TryConvert(commandGroup, commandId, pvaIn, out key_input)) {
                    handled = false;
                }
                else if (!_host.CanProcess(key_input)) {
                    handled = false;
                }

                if (handled) {
                    VimKeyEventArgs args = new VimKeyEventArgs(key_input);
                    _host.KeyDown(args);
                    if (args.Handled) {
                        return VSConstants.S_OK;
                    }
                }

                return _nextTarget.Exec(commandGroup, commandId, nCmdexecopt, pvaIn, pvaOut);
            }
            catch (Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
#endif
                return -1;
            }
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            try {
                VimKeyInput ki = null;
                if ((cCmds == 1) &&
                    this.TryConvert(pguidCmdGroup, prgCmds[0].cmdID, pCmdText, out ki) &&
                    _host.CanProcess(ki)) {
                    prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                    return VSConstants.S_OK;
                }
                return _nextTarget.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
            catch (Exception ex) {
#if DEBUG
                System.Windows.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
#endif
                return -1;
            }
        }

        #endregion
    }
}
