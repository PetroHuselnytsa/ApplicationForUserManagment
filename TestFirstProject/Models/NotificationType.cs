namespace TestFirstProject.Models
{
    /// <summary>
    /// Enum representing the different types of notifications in the system.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>A new message was received.</summary>
        NewMessage = 0,

        /// <summary>The user's role was changed by an admin.</summary>
        RoleChanged = 1,

        /// <summary>General system announcement.</summary>
        SystemAnnouncement = 2,

        /// <summary>A conversation was started with the user.</summary>
        ConversationStarted = 3,

        /// <summary>The user's account was updated.</summary>
        AccountUpdated = 4
    }
}
