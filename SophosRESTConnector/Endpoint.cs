using System;
using csJson;

namespace SophosRESTConnector
{
    public enum EndpointType
    {
        computer,
        server,
        securityVm
    }
    
    public enum EndpointStatus
    {
        good,
        suspicious,
        bad,
        unkown
    }

    public enum EndpointProductCode
    {
        coreAgent, 
        interceptX, 
        xdr, 
        endpointProtection, 
        deviceEncryption, 
        mtr, 
        mtd, 
        ztna
    }

    public enum EndpointIsolation
    {
        notIsolated,
        SelfIsolated,
        IsolatedByAdmin,
        Unkown
    }

    public struct EndpointProduct
    {
        public readonly EndpointProductCode Code;
        public readonly string VersionString;
        public readonly bool IsInstalled;

        internal EndpointProduct( jsonObject obj)
        {
            Code = (EndpointProductCode)Enum.Parse(
                typeof(EndpointProductCode),
                obj["code"].String
            );
            VersionString = obj["version"].String;
            IsInstalled = obj["status"].String == "installed";
        }

    } //End of class

    public struct Endpoint
    {
        public readonly EndpointProduct[] AssignedProducts;
        public readonly string AssociatedUser;
        public readonly EndpointStatus StatusOverall;
        public readonly EndpointStatus StatusServices;
        public readonly EndpointStatus StatusThreats;
        public readonly string Id;
        public readonly string Hostname;
        public readonly string[] IP4Addresses;
        public readonly string[] IP6Addresses;
        public readonly string[] MACAddresses;
        public readonly string OperatingSystemName;
        public readonly long OperatingSystemBuild;
        public readonly bool IsServer;
        public readonly bool IsTamperProtected;
        public readonly EndpointType Type;

        private static string[] LoadStringArray( jsonObject obj, string fieldname)
        {
            try
            {
                ulong cnt = obj[fieldname].Array.Elements;
                var result = new string[cnt];
                while (cnt-- > 0)
                {
                    result[cnt] = obj[fieldname].Array[cnt].String;
                }

                return result;
            }
            catch
            {
                return new string[0];
            }
        }

        private static EndpointProduct[] LoadProducts( jsonArray arr)
        {
            ulong cnt = arr.Elements;
            var result = new EndpointProduct[cnt];
            while( cnt-- > 0)
            {
                result[cnt] = new EndpointProduct(arr[cnt].Object);
            }

            return result;
        }

        private static EndpointIsolation GetIsolationStatus( jsonObject obj)
        {
            if( obj["status"].String == "notIsolated")
            {
                return EndpointIsolation.notIsolated;
            }

            if( obj["adminIsolated"].Bool)
            {
                return EndpointIsolation.IsolatedByAdmin;
            }

            if( obj["selfIsolated"].Bool)
            {
                return EndpointIsolation.SelfIsolated;
            }

            return EndpointIsolation.Unkown;
        }

        private static string GetUserName( jsonObject obj)
        {
            try
            {
                return obj["associatedPerson"].Object["name"].String;
            }
            catch { }

            try
            {
                return obj["associatedPerson"].Object["viaLogin"].String;
            }
            catch { }

            return string.Empty;
        }

        internal Endpoint( jsonObject obj)
        {
            AssignedProducts = LoadProducts(obj["assignedProducts"].Array);

            AssociatedUser = GetUserName(obj);
            Id = obj["id"].String;
            Hostname = obj["hostname"].String;
            OperatingSystemName = obj["os"].Object["name"].String;
            OperatingSystemBuild = obj["os"].Object["build"].Integer;
            IsServer = obj["os"].Object["isServer"].Bool;
            IsTamperProtected = obj["tamperProtectionEnabled"].Bool;

            IP4Addresses = LoadStringArray(obj, "ip4Addresses");
            IP6Addresses = LoadStringArray(obj, "ip6Addresses");
            MACAddresses = LoadStringArray(obj, "macAddresses");

            StatusOverall = (EndpointStatus)Enum.Parse(
                typeof(EndpointStatus),
                obj["health"].Object["overall"].String
            );

            StatusServices = (EndpointStatus)Enum.Parse(
                typeof(EndpointStatus),
                obj["health"].Object["services"].Object["status"].String
            );

            StatusThreats = (EndpointStatus)Enum.Parse(
                typeof(EndpointStatus),
                obj["health"].Object["threats"].Object["status"].String
            );

            Type = (EndpointType)Enum.Parse(
                typeof(EndpointType),
                obj["type"].String
            );
        }

    } //End of class

} //End of namespace
