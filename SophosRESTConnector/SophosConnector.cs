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
            client.DefaultRequestHeaders.Add("X-Partner-ID", partnerid);
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
            tenantset = new HashSet<Tenant>();
            var response = client.GetAsync($"{baseurl}tenants?pageTotal=true").Result;
            var content = response.Content.ReadAsStringAsync().Result;

            var json = jsonRoot.Parse(content);
            ProcessTenantList(json);

            var pagecurrent = json["pages"].Object["current"].Integer;
            var pagemax = json["pages"].Object["total"].Integer;
            while( pagecurrent++ < pagemax)
            {
                string pageurl = $"{baseurl}tenants?page={pagecurrent}";
                response = client.GetAsync(pageurl).Result;
                content = response.Content.ReadAsStringAsync().Result;
                json = jsonRoot.Parse(content);
                ProcessTenantList(json);
            }
        }

        public SophosConnector( string id, string secret )
        {
            clientid = id;
            clientsecret = secret;
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            VerifyCredentials();
            ObtainPartnerId();
            ObtainTenants();
        }
    }
}
