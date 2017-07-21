namespace GithubDisplay
{
    public static partial class Resources
    {
        const string CLIENT_ID = "fdc07673ffd19f4bb456";
        const string CLIENT_NAME = "PR-Display";
        const string CLIENT_REDIRECT_URL = "http://localhost:52284/";

        public static string ClientId => CLIENT_ID;

        // Define in a partial class with the filename SecretResources.cs
        public static string ClientSecret => CLIENT_SECRET;

        public static string ClientName => CLIENT_NAME;

        public static string ClientRedirectUrl => CLIENT_REDIRECT_URL;

        // Define in a partial class with the filename SecretResources.cs
        public static string GithubBypassToken => GITHUB_BYPASS_TOKEN;
    }
}
