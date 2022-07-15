using System;
using SophosRESTConnector;
using SophosRESTConnector.Requests;

namespace Experimental
{
    class Entrypoint
    {
        static void Main(string[] args)
        {
            var sophos = new SophosConnector(
                id: "b1e43585-fa37-437f-9f26-1db5f3397efc",
                secret: "e087863cb24ee3a43656acc82ea9a3c2a45315efd77e3e7058031cd56d3231df0da52525cd87cb855e0037515471dc504ea8"
            );

            var param = new TimeParameter(14, 7, 2022, TimeConstraintType.After);


            foreach( var tenant in sophos.Tenants)
            {
                var alerts = sophos.GetAlerts(tenant, param);
                if( alerts.Length == 0)
                {
                    continue;
                }

                foreach( var alert in alerts)
                {
                    Console.WriteLine($"[{alert.IncidentTime}] {tenant.Name} - {alert.AlertType}");
                }
            }
        }

    } //End of class

} //End of namespace
