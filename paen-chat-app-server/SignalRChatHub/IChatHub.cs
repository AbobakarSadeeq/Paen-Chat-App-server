

namespace paen_chat_app_server.SignalRChatHub
{
    public interface IChatHub
    {
        Task SendMessageToUser(string message);
    }
}
