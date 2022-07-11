using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using csJson;

namespace SophosRESTConnector
{
    public struct Tenant
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string GeographicalRegion;
        public readonly string DataRegion;
        public readonly string BillingType;
        public readonly string PartnerId;
        public readonly string ApiUrl;
        public readonly string Status;

        internal Tenant( jsonObject obj)
        {
            Id = obj["id"].String;
            Name = obj["name"].String;
            GeographicalRegion = obj["dataGeography"].String;
            DataRegion = obj["dataRegion"].String;
            BillingType = obj["billingType"].String;
            PartnerId = obj["partner"].Object["id"].String;
            ApiUrl = obj["apiHost"].String;
            Status = obj["status"].String;
        }
    }

    public enum TimeConstraintType
    {
        Before,
        After
    }

    public class TimeConstraint
    {
        public TimeConstraintType constrainttype;
        public DateTime when;
    }
    
    public struct Alert
    {

    }

    public class SophosConnector
    {
        private HttpClient client;
        private string clientid;
        private string clientsecret;

        private string accesstoken;
        private string refreshtoken;
        private string trackingid;

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

        private void VerifyCredentials()
        {
            var msg = new HttpRequestMessage();
            msg.Method = HttpMethod.Post;
            msg.RequestUri = new Uri("https://id.sophos.com/api/v2/oauth2/token");
            msg.Content = new StringContent($"grant_type=client_credentials&client_id={clientid}&client_secret={clientsecret}&scope=token");
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = client.SendAsync(msg).Result;
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
            refreshtoken = verificationresult["refresh_token"].String;
            trackingid = verificationresult["trackingId"].String;
        }

        private void ObtainPartnerId()
        {
            var msg = new HttpRequestMessage();
            msg.RequestUri = new Uri("https://api.central.sophos.com/whoami/v1");
            msg.Headers.Add("Authorization", $"Bearer {accesstoken}");

            var response = client.SendAsync(msg).Result;
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

            var response = client.SendAsync(req).Result;
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

                response = client.SendAsync(req).Result;
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

        public IEnumerable<Alert> GetAlerts( Tenant ten, TimeConstraint tc)
        {
            var req = new HttpRequestMessage();
            req.RequestUri = new Uri($"{ten.ApiUrl}/common/v1/alerts?from=2022-07-10T00:00:00.000Z");
            req.Headers.Add("X-Tenant-ID", ten.Id);
            req.Method = HttpMethod.Get;


            var response = client.SendAsync(req).Result;
            string content = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(content);

            return null;
        }
    }
}
