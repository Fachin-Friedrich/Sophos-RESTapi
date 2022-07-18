using System;
using SophosRESTConnector;
using SophosRESTConnector.Requests;
using System.Collections.Generic;

namespace Experimental
{
    internal class AlertRecord
    {
        public Alert alert;
        public DateTime latest;
        public Tenant tenant;

        public AlertRecord( Alert palert, Tenant ptenant)
        {
            alert = palert;
            latest = palert.IncidentTime;
            tenant = ptenant;
        }
    }

    class Entrypoint
    {
        static ConfigurationSet config;
        static SophosConnector sophos;
        static Dictionary<string, AlertRecord> flaggedalerts;
        
        private static void writeLog( string msg)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[{DateTime.Now}] {msg}");
            Console.ForegroundColor = col;
        }

        private static void ProcessInstigatorDown( Tenant tenant, Alert alert )
        {
            int sanctionindex = 0;
            while (
                sanctionindex < config.SanctionedAlerts.Length &&
                alert.AlertType != config.SanctionedAlerts[sanctionindex].MessageIsDown
            ) {
                ++sanctionindex; 
            }

            if ( sanctionindex == config.SanctionedAlerts.Length)
            {
                return;
            }

            string recordkey = alert.ManagementAgentId + config.SanctionedAlerts[sanctionindex].MessageIsDown;
            if ( flaggedalerts.ContainsKey(recordkey))
            {
                var record = flaggedalerts[recordkey];
                if( alert.IncidentTime > record.latest)
                {
                    record.latest = alert.IncidentTime;
                }
            }
            else
            {
                flaggedalerts.Add(
                    recordkey,
                    new AlertRecord(alert,tenant)
                );
            }
        }

        private static object GetAssociatedTicket( Alert alert)
        {
            return null;
        }

        private static void ProcessInstigatorUp( Tenant tenant, Alert alert)
        {
            int sanctionindex = 0;
            while (
                sanctionindex < config.SanctionedAlerts.Length &&
                alert.AlertType != config.SanctionedAlerts[sanctionindex].MessageIsUp
            ) { ++sanctionindex; }

            if (sanctionindex == config.SanctionedAlerts.Length)
            {
                return;
            }

            string recordkey = alert.ManagementAgentId + config.SanctionedAlerts[sanctionindex].MessageIsDown;
            if ( flaggedalerts.ContainsKey(recordkey))
            {
                var record = flaggedalerts[recordkey];
                if ( alert.IncidentTime > record.latest)
                {
                    writeLog($"Try Removing alert for {alert.ManagementAgentId}");
                    flaggedalerts.Remove(recordkey);
                }

                return;
            }

            var associdatedticket = GetAssociatedTicket(alert);
            if( associdatedticket != null)
            {
                //TODO Close ticket

                return;
            }

            //TODO throw warning for unrecognized closed
        }

        private static void CreateAssociatedTicket( Tenant tenant, Alert alert)
        {

        }

        private static void ForwardAlert( Tenant tenant, Alert alert )
        {
            int sanctionindex = 0;
            while (
                sanctionindex < config.SanctionedAlerts.Length && 
                alert.AlertType != config.SanctionedAlerts[sanctionindex].MessageIsDown &&
                alert.AlertType != config.SanctionedAlerts[sanctionindex].MessageIsUp
            ) { ++sanctionindex; }

            if (sanctionindex == config.SanctionedAlerts.Length)
            {
                return;
            }


        }

        static void Main(string[] args)
        {
            
            
            flaggedalerts = new Dictionary<string, AlertRecord>();
            config = new ConfigurationSet();
            config.DebugInit();
            sophos = new SophosConnector(
                config.SophosId,
                config.SophosSecret
            );

            while (true) //main loop
            {
                flaggedalerts.Clear();

                var now = DateTime.Now;
                var lastquery = config.LastQueryTime;
                config.LastQueryTime = now;

                var queryfilter = new TimeParameter(
                    lastquery.AddSeconds(-config.QueryTimeOffset),
                    TimeConstraintType.After
                );

                foreach( var tenant in sophos.Tenants)
                {
                    var alerts = sophos.GetAlerts(tenant, queryfilter);
                    foreach( var alert in alerts)
                    {
                        ProcessInstigatorDown(tenant, alert );
                    }

                    foreach( var alert in alerts)
                    {
                        ProcessInstigatorUp(tenant, alert);
                    }

                    foreach( var alert in alerts)
                    {
                        ForwardAlert(tenant, alert);
                    }
                }

                foreach( var record in flaggedalerts)
                {
                    writeLog($"Creating ticket for on {record.Value.tenant.Name} for {record.Value.alert.ManagementAgentId}");
                    writeLog($"incident time {record.Value.alert.IncidentTime}");
                    
                    CreateAssociatedTicket(
                        record.Value.tenant,
                        record.Value.alert
                    );
                }

                break;
            }

        }

    } //End of class

} //End of namespace
