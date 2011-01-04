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

        bool TriggerEscapeKey(Guid commandGroup, uint commandId, IntPtr pvaIn)
        {
            // Don't ever process a command when we are in an automation function.  Doing so will cause VsVim to 
            // intercept items like running Macros and certain wizard functionality
            if (VsShellUtilities.IsInAutomationFunction(_serviceProvider)) {
                return false;
            }

            bool triggered = false;
            if (VSConstants.GUID_VSStandardCommandSet97 == commandGroup) {
                if ((VSConstants.VSStd97CmdID)commandId == VSConstants.VSStd97CmdID.Escape) {
                    triggered = true;
                }
            }
            else if (VSConstants.VSStd2K == commandGroup) {
                if ((VSConstants.VSStd2KCmdID)commandId == VSConstants.VSStd2KCmdID.CANCEL) {
                    triggered = true;
                }
            }

            return triggered;
        }

        #region IOleCommandTarget implementation

        int IOleCommandTarget.Exec(ref Guid commandGroup, uint commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
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

            if (this.IsDebugIgnore(commandGroup, commandId) ||
                !this.TriggerEscapeKey(commandGroup, commandId, pvaIn)) {
                return _nextTarget.Exec(commandGroup, commandId, nCmdexecopt, pvaIn, pvaOut);
            }

            _host.KeyDown(new VimKeyEventArgs(new VimKeyInput("Esc")));

            return VSConstants.S_OK;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if ((cCmds == 1) && this.TriggerEscapeKey(pguidCmdGroup, prgCmds[0].cmdID, pCmdText)) {
                prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                return VSConstants.S_OK;
            }
            return _nextTarget.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #endregion
    }
}
