using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

namespace VisualHg
{
    public class IdleNotifier : IOleComponent
    {
        private uint _wComponentID;
        private IOleComponentManager _cmService;

        public event EventHandler Idle = (s, e) => { };


        public void Register(IOleComponentManager cmService)
        {
            _cmService = cmService;

            if (_cmService != null)
            {
                RegisterComponent();
            }
        }

        private void RegisterComponent()
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

            _cmService.FRegisterComponent(this, new[] { pcrinfo }, out _wComponentID);
        }

        public void Revoke()
        {
            if (_cmService != null)
            {
                _cmService.FRevokeComponent(_wComponentID);
            }
        }


        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FDoIdle(uint grfidlef)
        {
            Idle(this, EventArgs.Empty);

            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0; 
        }

        public int FQueryTerminate(int fPromptUser)
        { 
            return 1;
        }
        
        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        { 
            return 0;
        }
        
        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        { 
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
        
        public void OnAppActivate(int fActive, uint dwOtherThreadID) { }
        
        public void OnEnterState(uint uStateID, int fEnter) { }
        
        public void OnLoseActivation() { }
        
        public void Terminate() { }
    }
}
