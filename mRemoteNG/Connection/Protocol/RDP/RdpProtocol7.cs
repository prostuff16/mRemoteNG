using AxMSTSCLib;
using mRemoteNG.App;
using MSTSCLib;
using System;
using System.Windows.Forms;
using mRemoteNG.Resources.Language;

namespace mRemoteNG.Connection.Protocol.RDP
{
    public class RdpProtocol7 : RdpProtocol6
    {
        protected override RdpVersion RdpProtocolVersion => RdpVersion.Rdc7;

        public override bool Initialize()
        {
            if (!base.Initialize())
                return false;

            try
            {
                var rdpClient7 = (MsRdpClient7NotSafeForScripting)((AxHost) Control).GetOcx();
                rdpClient7.AdvancedSettings8.AudioQualityMode = (uint)connectionInfo.SoundQuality;
                rdpClient7.AdvancedSettings8.AudioCaptureRedirectionMode = connectionInfo.RedirectAudioCapture;
                // NetworkConnectionType is intentionally left at the client default (auto-detect), as in 1.76.
                // Forcing Modem applied the connection's experience flags (wallpaper, themes, font smoothing...)
                // and told the server the link was slow, which made sessions look worse than in 1.76.

                if (connectionInfo.UseVmId)
                {
                    SetExtendedProperty("DisableCredentialsDelegation", true);
                    rdpClient7.AdvancedSettings7.AuthenticationServiceClass = "Microsoft Virtual Console Service";
                    rdpClient7.AdvancedSettings8.EnableCredSspSupport = true;
                    rdpClient7.AdvancedSettings8.NegotiateSecurityLayer = false;
                    rdpClient7.AdvancedSettings7.PCB = $"{connectionInfo.VmId}";
                    if (connectionInfo.UseEnhancedMode)
                        rdpClient7.AdvancedSettings7.PCB += ";EnhancedMode=1";
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace(Language.RdpSetPropsFailed, ex);
                return false;
            }

            return true;
        }

        protected override AxHost CreateActiveXRdpClientControl()
        {
            return new AxMsRdpClient7NotSafeForScripting();
        }
    }
}
