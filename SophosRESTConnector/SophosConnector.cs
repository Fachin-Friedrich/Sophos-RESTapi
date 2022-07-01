using System;
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

        private jsonRoot verificationresult;

        private string accesstoken
        {
            get => verificationresult["access_token"].String;
        }

        private string refreshtoken
        {
            get => verificationresult["refresh_token"].String;
        }

        private string trackingid
        {
            get => verificationresult["trackingId"].String;
        }

        public SophosConnector( string id, string secret )
        {
            clientid = id;
            clientsecret = secret;
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            var msg = new HttpRequestMessage();
            msg.Method = HttpMethod.Post;
            msg.RequestUri = new Uri("https://id.sophos.com/api/v2/oauth2/token");
            msg.Content = new StringContent($"grant_type=client_credentials&client_id={id}&client_secret={secret}&scope=token" );
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = client.SendAsync(msg).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            if( !response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Failed to establish connection to SOPHOS API with following context: {content}"
                );
            }

            verificationresult = jsonRoot.Parse(content);
            if( verificationresult["errorCode"].String != "success")
            {
                throw new Exception(
                    $"Failed to authenticate against SOPHOS API with following error: {content}"
                );
            }
        }
    }
}
