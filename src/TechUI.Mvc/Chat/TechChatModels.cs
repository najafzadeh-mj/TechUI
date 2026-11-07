namespace TechUI.Mvc.Chat
{
    /// <summary>
    /// Conversation style of the standalone chat component.
    /// </summary>
    public enum TechChatMode
    {
        /// <summary>One request, one reply (AI assistant, FAQ bot).</summary>
        Assistant,

        /// <summary>Human support: messages are delivered to an inbox and
        /// replies arrive by polling or a realtime push.</summary>
        Support
    }

    /// <summary>
    /// Body posted by the chat composer. The endpoint is fully
    /// provider-agnostic: TechUI ships no AI or chat-backend dependency.
    /// </summary>
    public sealed class TechChatRequest
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>Client-generated id used to group a conversation.</summary>
        public string? ConversationId { get; set; }

        /// <summary>Display name of the visitor, when known.</summary>
        public string? Sender { get; set; }

        public List<TechChatMessage> History { get; set; } = new();
    }

    public sealed class TechChatMessage
    {
        public long Id { get; set; }

        /// <summary>user / assistant / agent / system</summary>
        public string Role { get; set; } = "user";

        public string? Sender { get; set; }

        public string Content { get; set; } = string.Empty;

        /// <summary>Display time chosen by the server (e.g. 14:32).</summary>
        public string? Time { get; set; }
    }

    /// <summary>
    /// Reply contract. In assistant mode <see cref="Reply"/> is rendered
    /// immediately. In support mode the endpoint may only acknowledge
    /// (<see cref="Delivered"/>), and replies are returned from the poll
    /// endpoint inside <see cref="Messages"/>.
    /// </summary>
    public sealed class TechChatResponse
    {
        public string? Reply { get; set; }

        public string? Sender { get; set; }

        public long? MessageId { get; set; }

        public bool? Delivered { get; set; }

        /// <summary>Lets the poll endpoint show the "agent is typing"
        /// indicator while a reply is being prepared.</summary>
        public bool? Typing { get; set; }

        public List<TechChatMessage> Messages { get; set; } = new();
    }
}