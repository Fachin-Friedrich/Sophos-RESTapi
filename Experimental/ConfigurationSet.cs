using System;


namespace Experimental
{
    public struct InstigationPairing
    {
        public string MessageIsUp;
        public string MessageIsDown;
    }
    
    public struct ConfigurationSet
    {

        public string SophosId;
        public string SophosSecret;

        public int QueryTimeOffset;
        public int TicketCreationThreshold;

        public InstigationPairing[] SanctionedAlerts;

        private const string QueryTimeStorage = "lastquery.bin";
        private const int TimeTypeSize = 8;
        public DateTime LastQueryTime
        {
            get
            {
                try
                {
                    var fhandle = System.IO.File.Open(
                        QueryTimeStorage,
                        System.IO.FileMode.Open
                    );

                    var buf = new byte[TimeTypeSize];
                    if( fhandle.Read(buf, 0, TimeTypeSize) != TimeTypeSize)
                    {
                        fhandle.Close();
                        throw new Exception();
                    }
                    fhandle.Close();

                    long ticks = BitConverter.ToInt64(buf, 0);
                    return new DateTime(ticks);

                }catch
                {
                    var now = DateTime.Now;
                    var today = now
                        .AddHours(-now.Hour)
                        .AddMinutes(-now.Minute)
                        .AddSeconds(-now.Second)
                        .AddMilliseconds(-now.Millisecond);
                    return today;
                }
            }

            set
            {
                var buf = BitConverter.GetBytes(value.Ticks);
                var fhandle = System.IO.File.Open(
                    QueryTimeStorage,
                    System.IO.FileMode.OpenOrCreate
                );

                fhandle.Write(buf, 0, TimeTypeSize);
                fhandle.Close();
            }
        }

        public void DebugInit()
        {
            SophosId = "b1e43585-fa37-437f-9f26-1db5f3397efc";
            SophosSecret = "e087863cb24ee3a43656acc82ea9a3c2a45315efd77e3e7058031cd56d3231df0da52525cd87cb855e0037515471dc504ea8";

            QueryTimeOffset = 600;
            TicketCreationThreshold = 300;

            SanctionedAlerts = new InstigationPairing[] {
                new InstigationPairing
                {
                    MessageIsUp = "Event::Firewall::FirewallREDTunnelUp",
                    MessageIsDown = "Event::Firewall::FirewallREDTunnelDown"
                },
                new InstigationPairing
                {
                    MessageIsUp = "Event::Firewall::FirewallVPNTunnelUp",
                    MessageIsDown = "Event::Firewall::FirewallVPNTunnelDown"
                },
                new InstigationPairing
                {
                    MessageIsUp = "Event::Firewall::FirewallGatewayUp",
                    MessageIsDown = "Event::Firewall::FirewallGatewayDown"
                },
                new InstigationPairing
                {
                    MessageIsUp = "Event::Firewall::Reconnected",
                    MessageIsDown = "Event::Firewall::LostConnectionToSophosCentral"
                }
            };

        }

        public ConfigurationSet( string filename)
        {
            throw new NotImplementedException();
        }
    }
}
