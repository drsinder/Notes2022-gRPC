using Microsoft.AspNetCore.SignalR;
using Notes2022.Server.Data;
using Notes2022.Server.Proto;

namespace Notes2022.Server.Hubs
{
    public class MasterHub : Hub
    {
        private readonly NotesDbContext _db;

        public MasterHub(NotesDbContext db)
        {
            _db = db;
        }

        //public override Task OnConnectedAsync()
        //{
        //    string clientId = Context.ConnectionId;

        //    return base.OnConnectedAsync();
        //}

        public async Task OpenSession(string userId, string userName)
        {
            string clientId = Context.ConnectionId;

            await UserCleanUp();

            ActiveUsers? activeUsers = _db.ActiveUsers.SingleOrDefault(p => p.Subject == userId && p.ClientId == clientId);
            if (activeUsers is not null)
            {
                activeUsers.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTime.UtcNow);
                _db.Update(activeUsers);
            }
            else
            {

                ActiveUsers me = new() { DisplayName = userName, Subject = userId,
                        StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTime.UtcNow), ClientId = clientId };
                _db.ActiveUsers.Add(me);
            }

            await _db.SaveChangesAsync();

            await SendUpdate();
        }

        public async Task CloseSession(string userId, string userName)
        {
            string clientId = Context.ConnectionId;

            await UserCleanUp();

            ActiveUsers? activeUsers = _db.ActiveUsers.SingleOrDefault(p => p.Subject == userId && p.ClientId == clientId);
            if (activeUsers is not null)
            {
                _db.ActiveUsers.Remove(activeUsers);
                await _db.SaveChangesAsync();
            }
            await SendUpdate();
        }

        private async Task UserCleanUp()
        {
            Google.Protobuf.WellKnownTypes.Timestamp then = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(DateTime.UtcNow.AddMinutes(-1).AddSeconds(-20));
            List<ActiveUsers> inactiveUsers = _db.ActiveUsers.Where(p => p.StartTime <= then ).ToList();

            if (inactiveUsers != null && inactiveUsers.Count > 0)
            {
                _db.ActiveUsers.RemoveRange(inactiveUsers);
                await _db.SaveChangesAsync();
            }
        }

        private async Task SendUpdate()
        {
            List<ActiveUsers> activeUsers = _db.ActiveUsers.ToList();
            int count = 0;
            if (activeUsers != null && activeUsers.Count > 0)
                count = activeUsers.Count;

            await Clients.All.SendAsync("ReceiveActiveUsers", count, activeUsers);
        }

        public async Task TalkRequest(string ToclientId, string FromclientId, string userName, string toName)
        {
            if (Context.ConnectionId == ToclientId)
            {
                await Clients.Single(Context.ConnectionId).SendAsync("TalkRejected", "You can not talk to yourself!");
                return;
            }

            //string toName = _db.ActiveUsers.Single(p => p.ClientId == ToclientId).DisplayName;

            await Clients.Single(ToclientId).SendAsync("TalkRequest", ToclientId, FromclientId, userName, toName);
        }

        public async Task TalkRejected(string FromclientId, string userName)
        {
            await Clients.Single(FromclientId).SendAsync("TalkRejected", $"Your request to talk to {userName} has been rejected.");
        }

        public async Task TalkAccepted(string ToclientId, string FromclientId, string userName, string toName)
        {
            string groupid = ToclientId + ":" + FromclientId;
            await Groups.AddToGroupAsync(ToclientId, groupid);
            await Groups.AddToGroupAsync(FromclientId, groupid);

            await Clients.Groups(groupid).SendAsync("TalkAccepted", ToclientId, FromclientId, userName, toName);

            Thread.Sleep(500);
            await Clients.Groups(groupid).SendAsync("PrivateMessage", "Notes 2022", "You are connected!");
        }

        public async Task PrivateMessage(string ToclientId, string FromclientId, string userName, string message)
        {
            string groupid = ToclientId + ":" + FromclientId;
            await Clients.Groups(groupid).SendAsync("PrivateMessage", userName, message);
        }

        public async Task EndTalk(string ToclientId, string FromclientId)
        {
            string groupid = ToclientId + ":" + FromclientId;

            await Clients.Groups(groupid).SendAsync("EndTalk");

            await Groups.RemoveFromGroupAsync(ToclientId, groupid);
            await Groups.RemoveFromGroupAsync(FromclientId, groupid);
        }


    }
}
