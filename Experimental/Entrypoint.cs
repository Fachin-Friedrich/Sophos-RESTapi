using System;
using SophosRESTConnector;

namespace Experimental
{
    class Entrypoint
    {
        static void Main(string[] args)
        {
            var q = new SophosConnector(
                id: "b1e43585-fa37-437f-9f26-1db5f3397efc",
                secret: "e087863cb24ee3a43656acc82ea9a3c2a45315efd77e3e7058031cd56d3231df0da52525cd87cb855e0037515471dc504ea8"
            );

            var tc = new TimeConstraint();
            tc.constrainttype = TimeConstraintType.After;
            tc.when = DateTime.Now.AddDays(-7);

            foreach( var t in q.Tenants)
            {
                if( t.Name.StartsWith("Mobile Pflege Petker") == false)
                {
                    continue;
                }
                
                Console.WriteLine(t.Name);
                var alerts = q.GetAlerts(t, tc);
                System.Threading.Thread.Sleep(2000);
            }

        }
    }
}
