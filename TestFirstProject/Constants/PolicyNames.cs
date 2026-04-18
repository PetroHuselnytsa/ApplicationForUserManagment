namespace TestFirstProject.Constants
{
    /// <summary>
    /// Centralized authorization policy and rate-limit policy names.
    /// Prevents typos and enables safe refactoring.
    /// </summary>
    public static class PolicyNames
    {
        public const string AdminOnly = "AdminOnly";
        public const string PremiumUser = "PremiumUser";
        public const string UserManagement = "UserManagement";

        public const string AuthRateLimit = "AuthRateLimit";
        public const string MessageRateLimit = "MessageRateLimit";
    }
}
