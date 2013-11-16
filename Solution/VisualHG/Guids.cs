using System;

namespace VisualHg
{
    public static class Guids
    {
        public const string Package = "dadada00-dfd3-4e42-a61c-499121e136f3";
        public const string Provider = "dadada00-63c7-4363-b107-ad5d9d915d45";
        public const string Service = "dadada00-d8ac-4ba7-8b05-5166c8f08ef5";
        public const string CommandSet = "dadada00-1fd3-4e26-9c1d-c9cb723cea0e";
        public const string OptionsPage = "dadada00-09a5-4795-a3ca-c3b49448184d";
        public const string ToolWindow = "dadada00-d3b4-4d5c-a138-a87ca494f6c2";
       
        public static readonly Guid PackageGuid = new Guid(Package);
        public static readonly Guid ProviderGuid = new Guid(Provider);
        public static readonly Guid ServiceGuid = new Guid(Service);
        public static readonly Guid CommandSetGuid = new Guid(CommandSet);
    };
}
