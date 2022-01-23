using Newtonsoft.Json;

namespace SynoAI.Notifiers.Pushover
{
    public class PushoverRequest
    {
        /// <summary>
        /// The application's API token.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }
        /// <summary>
        /// The user/group key (not e-mail address) of the user; viewable when logged into the dashboard (often referred to as USER_KEY in the documentation and code examples).
        /// </summary>
        [JsonProperty("user")]
        public string User { get; set; }
        /// <summary>
        /// The message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        /// <summary>
        /// The user's device name to send the message directly to that device, rather than all of the user's devices (multiple devices may be separated by a comma).
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; set; }
        /// <summary>
        /// The message's title, otherwise the app name is used.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        /// <summary>
        /// Send as -2 to generate no notification/alert, -1 to always send as a quiet notification, 1 to display as high-priority and bypass the user's quiet hours, or 2 to also require confirmation from the user
        /// </summary>
        [JsonProperty("priority")]
        public PushoverPriority Priority { get; set; }
        /// <summary>
        /// The name of one of the sounds supported by device clients to override the user's default sound choice.
        /// </summary>
        [JsonProperty("sound")]
        public string Sound { get; set; }
    }
}
