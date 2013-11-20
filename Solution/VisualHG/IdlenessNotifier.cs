using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace VisualHg
{
    public class IdlenessNotifier : IOleComponent
    {
        private uint componentId;

        public event EventHandler Idle = (s, e) => { };


        public void Register()
        {
            var componentManager = GetComponentManager();

            if (componentManager != null)
            {
                Register(componentManager);
            }
        }

        private void Register(IOleComponentManager componentManager)
        {
            var pcrinfo = new OLECRINFO
            {
                cbSize   = (uint)Marshal.SizeOf(typeof(OLECRINFO)),

                grfcrf   = (uint)(_OLECRF.olecrfNeedIdleTime |
                                  _OLECRF.olecrfNeedPeriodicIdleTime),

                grfcadvf = (uint)(_OLECADVF.olecadvfModal |
                                  _OLECADVF.olecadvfRedrawOff |
                                  _OLECADVF.olecadvfWarningsOff),

                uIdleTimeInterval = 100,
            };

            componentManager.FRegisterComponent(this, new[] { pcrinfo }, out componentId);
        }

        public void Revoke()
        {
            var componentManager = GetComponentManager();

            if (componentId != 0 && componentManager != null)
            {
                componentManager.FRevokeComponent(componentId);
            }
        }


        private static IOleComponentManager GetComponentManager()
        {
            return Package.GetGlobalService(typeof(SOleComponentManager)) as IOleComponentManager;
        }


        int IOleComponent.FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        int IOleComponent.FDoIdle(uint grfidlef)
        {
            Idle(this, EventArgs.Empty);

            return 0;
        }

        int IOleComponent.FPreTranslateMessage(MSG[] pMsg)
        {
            return 0; 
        }

        int IOleComponent.FQueryTerminate(int fPromptUser)
        { 
            return 1;
        }
        
        int IOleComponent.FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        { 
            return 0;
        }
        
        IntPtr IOleComponent.HwndGetWindow(uint dwWhich, uint dwReserved)
        { 
            return IntPtr.Zero;
        }

        void IOleComponent.OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
        
        void IOleComponent.OnAppActivate(int fActive, uint dwOtherThreadID) { }
        
        void IOleComponent.OnEnterState(uint uStateID, int fEnter) { }
        
        void IOleComponent.OnLoseActivation() { }
        
        void IOleComponent.Terminate() { }
    }
}
