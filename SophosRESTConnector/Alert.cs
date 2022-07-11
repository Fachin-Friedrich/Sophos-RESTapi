using csJson;
using System;

namespace SophosRESTConnector
{
    public enum AlertCategory
    {
        azure, 
        adSync, 
        applicationControl, 
        appReputation, 
        blockListed, 
        connectivity, 
        cwg, 
        denc, 
        downloadReputation, 
        endpointFirewall, 
        fenc, 
        forensicSnapshot, 
        general, 
        isolation, 
        malware, 
        mtr, 
        mobiles, 
        policy, 
        protection, 
        pua, 
        runtimeDetections, 
        security, 
        smc, 
        systemHealth, 
        uav, 
        uncategorized, 
        updating, 
        utm, 
        virt, 
        wireless, 
        xgEmail, 
        ztnaAuthentication, 
        ztnaGateway, 
        ztnaResource
    }

    public enum AlertProduct
    {
        other, 
        endpoint, 
        server, 
        mobile, 
        encryption, 
        emailGateway, 
        webGateway, 
        phishThreat, 
        wireless, 
        firewall, 
        ztna
    }

    public enum AlertSeverity
    {
        low,
        medium,
        high
    }

    public enum AlertManagementType
    {
        invalid,
        mobile, 
        computer, 
        server, 
        securityVm, 
        utm, 
        accessPoint, 
        wirelessNetwork, 
        mailbox, 
        slec, 
        xgFirewall, 
        ztnaGateway
    }
    
    public struct Alert
    {
        public readonly string Id;
        public readonly string[] AllowedActions;
        public readonly AlertCategory Category;
        public readonly string Description;
        public readonly string GroupKey;
        public readonly string ManagementAgentId;
        public readonly AlertManagementType ManagementAgentType;
        public readonly string ManagementAgentName;
        public readonly AlertProduct Product;
        public readonly AlertSeverity Severity;
        public readonly string AlertType;
        public readonly DateTime IncidentTime;

        internal Alert( jsonObject obj)
        {
            Id = obj["id"].String;
            Category = (AlertCategory) Enum.Parse(typeof(AlertCategory), obj["category"].String);
            Description = obj["description"].String;
            GroupKey = obj["groupKey"].String;
            Product = (AlertProduct)Enum.Parse(typeof(AlertProduct), obj["product"].String);
            Severity = (AlertSeverity) Enum.Parse(typeof(AlertSeverity), obj["severity"].String);
            AlertType = obj["type"].String;

            IncidentTime = DateTime.Parse(obj["raisedAt"].String);

            try
            {
                ManagementAgentId = obj["managedAgent"].Object["id"].String;
            }
            catch
            {
                ManagementAgentId = string.Empty;
            }

            try
            {
                ManagementAgentName = obj["managedAgent"].Object["name"].String;
            }
            catch
            {
                ManagementAgentName = string.Empty;
            }

            try
            {
                ManagementAgentType = (AlertManagementType)Enum.Parse(
                    typeof(AlertManagementType),
                    obj["managedAgent"].Object["type"].String
                );
            }
            catch
            {
                ManagementAgentType = AlertManagementType.invalid;
            }

            try
            {
                ulong cnt = obj["allowedActions"].Array.Elements;
                AllowedActions = new string[cnt];
                while( cnt-- > 0)
                {
                    AllowedActions[cnt] = obj["allowedActions"].Array[cnt].String;
                }
            }
            catch
            {
                AllowedActions = new string[0];
            }
        }

    } //End of class

} //End of namespace
