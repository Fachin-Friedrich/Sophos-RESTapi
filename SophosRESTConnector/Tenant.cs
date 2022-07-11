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

        internal Tenant(jsonObject obj)
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
    } //End of class

} //End of namespace
