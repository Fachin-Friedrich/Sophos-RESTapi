using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using csJson;

namespace SophosRESTConnector
{
    
    public class SophosConnector
    {
        private HttpClient client;
        private string clientid;
        private string clientsecret;

        private string accesstoken;
        //private string refreshtoken;
        //private string trackingid;

        private string partnerid;
        private string baseurl;

        private HashSet<Tenant> tenantset;
        public IEnumerable<Tenant> Tenants
        {
            get => tenantset;
        }
        public int TenantCount
        {
            get => tenantset.Count;
        }

        private int lastthrottle;

        public int ThrottleTime
        {
            get => lastthrottle;
        }

        private void VerifyCredentials()
        {
            var msg = new HttpRequestMessage();
            msg.Method = HttpMethod.Post;
            msg.RequestUri = new Uri("https://id.sophos.com/api/v2/oauth2/token");
            msg.Content = new StringContent($"grant_type=client_credentials&client_id={clientid}&client_secret={clientsecret}&scope=token");
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = SendRequestThrottled(msg);
            var content = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Failed to establish connection to SOPHOS API with following context: {content}"
                );
            }

            var verificationresult = jsonRoot.Parse(content);
            if (verificationresult["errorCode"].String != "success")
            {
                throw new Exception(
                    $"Failed to authenticate against SOPHOS API with following error: {content}"
                );
            }

            accesstoken = verificationresult["access_token"].String;
            //refreshtoken = verificationresult["refresh_token"].String;
            //trackingid = verificationresult["trackingId"].String;
        }

        private void ObtainPartnerId()
        {
            var msg = new HttpRequestMessage();
            msg.RequestUri = new Uri("https://api.central.sophos.com/whoami/v1");
            msg.Headers.Add("Authorization", $"Bearer {accesstoken}");

            var response = SendRequestThrottled(msg);
            var content = response.Content.ReadAsStringAsync().Result;
            if( !response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Failed to obtain PartnerId with following error: {content}"
                );
            }

            var json = jsonRoot.Parse(content);
            partnerid = json["id"].String;
            baseurl = $"{json["apiHosts"].Object["global"].String}/partner/v1/";

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accesstoken}");
        }

        private void ProcessTenantList( jsonRoot json )
        {
            var arr = json["items"].Array;
            var elementcnt = arr.Elements;
            while( elementcnt-- > 0)
            {
                tenantset.Add(new Tenant(arr[elementcnt].Object));
            }
        }

        //TODO errorchecking
        private void ObtainTenants()
        {
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{baseurl}tenants?pageTotal=true");
            req.Headers.Add("X-Partner-ID", partnerid);
            req.Method = HttpMethod.Get;

            var response = SendRequestThrottled(req);
            var content = response.Content.ReadAsStringAsync().Result;

            var json = jsonRoot.Parse(content);
            ProcessTenantList(json);

            var pagecurrent = json["pages"].Object["current"].Integer;
            var pagemax = json["pages"].Object["total"].Integer;
            while( pagecurrent++ < pagemax)
            {
                req = new HttpRequestMessage();
                req.RequestUri = new Uri($"{baseurl}tenants?page={pagecurrent}");
                req.Headers.Add("X-Partner-ID", partnerid);
                req.Method = HttpMethod.Get;

                response = SendRequestThrottled(req);
                content = response.Content.ReadAsStringAsync().Result;
                json = jsonRoot.Parse(content);
                ProcessTenantList(json);
            }
        }

        public SophosConnector( string id, string secret )
        {
            clientid = id;
            clientsecret = secret;
            tenantset = new HashSet<Tenant>();
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            VerifyCredentials();
            ObtainPartnerId();
            ObtainTenants();
        }

        private void ProcessAlertList( jsonRoot json, HashSet<Alert> res )
        {
            ulong cnt = json["items"].Array.Elements;
            while( cnt-- > 0)
            {
                res.Add(new Alert(json["items"].Array[cnt].Object));
            }
        }

        private static string NextAlertPage( jsonRoot json)
        {
            try
            {
                return json["pages"].Object["nextKey"].String;
            }
            catch
            {
                return string.Empty;
            }
        }

        public IEnumerable<Alert> GetAlerts( Tenant ten, TimeConstraint tc)
        {
            var result = new HashSet<Alert>();
            
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{ten.ApiUrl}/common/v1/alerts");
            req.Headers.Add("X-Tenant-ID", ten.Id);
            req.Method = HttpMethod.Get;

            var response = SendRequestThrottled(req);
            string content = response.Content.ReadAsStringAsync().Result;
            jsonRoot json = jsonRoot.Parse(content);
            string next = NextAlertPage(json);

            ProcessAlertList(json, result);

            while( !string.IsNullOrEmpty(next))
            {
                req = new HttpRequestMessage();
                req.RequestUri = new Uri($"{ten.ApiUrl}/common/v1/alerts?pageFromKey={next}");
                req.Headers.Add("X-Tenant-ID", ten.Id);
                req.Method = HttpMethod.Get;

                response = SendRequestThrottled(req);
                content = response.Content.ReadAsStringAsync().Result;
                json = jsonRoot.Parse(content);
                next = NextAlertPage(json);

                ProcessAlertList(json, result);
            }

            return result;
        }

        private static void WriteLog( string msg)
        {
            var col = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}] {msg}");
            Console.ForegroundColor = col;
        }

        private HttpResponseMessage SendRequestThrottled( HttpRequestMessage msg)
        {
            var response = client.SendAsync(msg).Result;
            int attempts = 0;
            lastthrottle = 0;

            while( (int) response.StatusCode == 429) //SOPHOS uses code 429 for hitting rate limit
            {
                lastthrottle = ++attempts * 500;
                WriteLog($"Hit throttle with {lastthrottle}ms");
                System.Threading.Thread.Sleep(lastthrottle);
                response = client.SendAsync(msg).Result;
            }

            return response;
        }

    } //End of class

} //End of namespace
