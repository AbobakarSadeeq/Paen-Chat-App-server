using DataAccess.DataContext_Class;
using DataAccess.Entities;
using Microsoft.AspNetCore.SignalR;
using Presentation.ViewModel.UserHub;

namespace paen_chat_app_server.SignalRChatHub
{
    public class ChatHub : Hub
    {
        private readonly DataContext _dataContext;
         private HashSet<UsersHub> connectedUsers = new HashSet<UsersHub>();
        public ChatHub(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        //public async Task SendMessage(string userId, string userName, string message)
        //{
        //    var identifier = Context.UserIdentifier;
        //    await Clients.All.SendAsync("ReceiveMessage", userId, message);
        //}

       


        public async Task JoinGroup(string groupName, string userId)
        {
            // if group not found then it will make automatically for you.
            // if user is not there then add user to group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            // sending message to group about user has been joined
            await Clients.Group(groupName).SendAsync("UserJoinGroup", userId, groupName);
            // anyone is connecting to the group that must havve a default value which should not store 
            //await Clients.Groups(groupName).SendAsync("DefaultValue", "defaultVal", userId);


        }



        public async Task SendMessageToGroup(Message message, string groupName)
        {
            await Clients.Group(groupName).SendAsync("SendMessage", message);

        }

        public async Task RemoveUserFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        //  When connection is ON then first method inside the hub-class this method execute.
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        


        public async Task OnConnectedUserAsync(int userId, string userName)
        {
            connectedUsers.Add(new UsersHub
            {
                UserId = userId,
                UserName = userName,
                ConnectionId = Context.ConnectionId,
            });

            // it is used for to tell how many are connected to hub or group
             
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }





    }

}

