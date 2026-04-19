namespace TestFirstProject
{
    /// <summary>
    /// Centralized policy name constants to avoid magic strings across the application.
    /// </summary>
    public static class PolicyNames
    {
        public const string AdminOnly = "AdminOnly";
        public const string PremiumUser = "PremiumUser";
        public const string AuthenticatedUser = "AuthenticatedUser";
        public const string AuthRateLimit = "auth";
    }
}
