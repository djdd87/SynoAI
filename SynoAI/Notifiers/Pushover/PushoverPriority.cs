namespace SynoAI.Notifiers.Pushover
{
    /// <summary>
    /// Send as -2 to generate no notification/alert, -1 to always send as a quiet notification, 1 to display as high-priority and bypass the user's quiet hours, or 2 to also require confirmation from the user
    /// </summary>
    public enum PushoverPriority
    {
        /// <summary>
        /// Generate no notification/alert.
        /// </summary>
        NoNotification = -2,
        /// <summary>
        /// Always send as a quiet notification.
        /// </summary>
        Quiet = -1,
        /// <summary>
        /// Normal behaviour.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Display as high-priority and bypass the user's quiet hours.
        /// </summary>
        HighPriority = 1,
        /// <summary>
        /// High priority and also require confirmation from the user.
        /// </summary>
        RequireConfirmation = 2,
    }
}
