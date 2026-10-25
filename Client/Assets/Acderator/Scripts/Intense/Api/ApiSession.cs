namespace Intense.Api
{
    internal interface IApiSession
    {
        string Token { get; set; }
        string MasterVersion { get; set; }
    }

    public class ApiSession : IApiSession
    {
        public string Token { get; set; }
        public string MasterVersion { get; set; }
    }
}