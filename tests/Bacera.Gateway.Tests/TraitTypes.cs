namespace Bacera.Gateway;

public static class TraitTypes
{
    public const string Areas = "Area";
    public const string Types = "Type";
    public const string Parties = "Party";

    public static class Value
    {
        // Party
        public const string FirstParty = "1st-Party";
        public const string ThirdParty = "3rd-Party";

        // Area
        public const string Client = "Client";
        public const string Tenant = "Tenant";
        public const string Admin = "Admin";

        // Type
        public const string Service = "Service";
        public const string WebApi = "Web API";
        public const string Utility = "Utility";
        public const string RestSharp = "RestSharp";
        public const string Infrastructure = "Infrastructure";
    }
}