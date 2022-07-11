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

        private Tenant[] tenantlist;
        public Tenant[] Tenants
        {
            get => tenantlist;
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

        private static ulong ProcessTenantList( jsonRoot json, Tenant[] result, ulong offset )
        {
            for( ulong i = 0; i < json["items"].Array.Elements; ++i)
            {
                result[offset + i] = new Tenant( json["items"].Array[i].Object );
            }

            return offset + json["items"].Array.Elements;
        }

        //TODO errorchecking
        private Tenant[] ObtainTenants()
        {
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{baseurl}tenants?pageTotal=true");
            req.Headers.Add("X-Partner-ID", partnerid);
            req.Method = HttpMethod.Get;

            var response = SendRequestThrottled(req);
            var content = response.Content.ReadAsStringAsync().Result;
            var json = jsonRoot.Parse(content);

            ulong total = (ulong) json["pages"].Object["items"].Integer;
            var result = new Tenant[total];
            ulong offset = ProcessTenantList(json, result, 0);

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
                offset = ProcessTenantList(json, result, offset);
            }

            return result;
        }

        public SophosConnector( string id, string secret )
        {
            clientid = id;
            clientsecret = secret;
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            VerifyCredentials();
            ObtainPartnerId();
            tenantlist = ObtainTenants();
        }

        private static ulong ProcessAlertList( jsonRoot json, Alert[] result, ulong offset )
        {
            for( ulong i = 0; i < json["items"].Array.Elements; ++i)
            {
                result[i + offset] = new Alert(json["items"].Array[i].Object);
            }

            return offset + json["items"].Array.Elements;
        }

        private static ulong ProcessEndpointList( jsonRoot json, Endpoint[] result, ulong offset)
        {
            for (ulong i = 0; i < json["items"].Array.Elements; ++i)
            {
                result[i + offset] = new Endpoint(json["items"].Array[i].Object);
            }

            return offset + json["items"].Array.Elements;
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

        private static HttpRequestMessage DuplicateRequestMessage( HttpRequestMessage msg)
        {
            var dupe = new HttpRequestMessage();
            dupe.RequestUri = msg.RequestUri;
            dupe.Method = msg.Method;

            foreach( var header in msg.Headers)
            {
                dupe.Headers.Add(header.Key, header.Value);
            }

            return dupe;
        }

        public Alert[] GetAlerts( Tenant ten, TimeConstraint tc)
        {            
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{ten.ApiUrl}/common/v1/alerts?pageTotal=true");
            req.Headers.Add("X-Tenant-ID", ten.Id);
            req.Method = HttpMethod.Get;

            var response = SendRequestThrottled(req);
            string content = response.Content.ReadAsStringAsync().Result;
            jsonRoot json = jsonRoot.Parse(content);
            string next = NextAlertPage(json);

            ulong total = (ulong) json["pages"].Object["items"].Integer;
            var result = new Alert[total];
            ulong offset = ProcessAlertList(json, result, 0);

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

                offset = ProcessAlertList(json, result, offset);
            }

            return result;
        }

        public Endpoint[] GetEndpoints( Tenant ten)
        {
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{ten.ApiUrl}/endpoint/v1/endpoints?pageTotal=true");
            req.Headers.Add("X-Tenant-ID", ten.Id);
            req.Method = HttpMethod.Get;

            var response = SendRequestThrottled(req);
            string content = response.Content.ReadAsStringAsync().Result;
            jsonRoot json = jsonRoot.Parse(content);
            string next = NextAlertPage(json);

            ulong total = (ulong)json["pages"].Object["items"].Integer;
            var result = new Endpoint[total];
            ulong offset = ProcessEndpointList(json, result, 0);

            while (!string.IsNullOrEmpty(next))
            {
                req = new HttpRequestMessage();
                req.RequestUri = new Uri($"{ten.ApiUrl}/endpoint/v1/endpoints?pageFromKey={next}");
                req.Headers.Add("X-Tenant-ID", ten.Id);
                req.Method = HttpMethod.Get;

                response = SendRequestThrottled(req);
                content = response.Content.ReadAsStringAsync().Result;
                json = jsonRoot.Parse(content);
                next = NextAlertPage(json);

                offset = ProcessEndpointList(json, result, offset);
            }

            return result;
        }

        public static void WriteLog( string msg)
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
                System.Threading.Thread.Sleep(lastthrottle);
                msg = DuplicateRequestMessage(msg);
                response = client.SendAsync(msg).Result;
            }

            return response;
        }

    } //End of class

} //End of namespace
