using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Paintweb.Hubs
{
    public class ClientHub : Hub<Client>
    {
        private static ConcurrentDictionary<string, Client> GroupsByConnections = new ConcurrentDictionary<string, Client>();

        private Client MyGroup
        {
            get
            {
                Client client;
                if (GroupsByConnections.TryGetValue(Context.ConnectionId, out client) == false)
                    throw new Exception("Nie ma takiej grupy");
                return client;
            }
        }

        public async Task SendLineToGroup(string user, string message)
        {
            LineDataController.SaveLine(message);
            await MyGroup.ReceiveLine(user, message);
        }

        public async Task SendTextToGroup(string user, string message)
        {
            await MyGroup.ReceiveText(user, message);
        }
        public async Task SendClear(string user, string message)
        {
            LineDataController.ClearData(user);
            await MyGroup.ClearCanvas(user, message);
        }

        public Task SendLineToCaller(string user, string message)
        {
            return Clients.Caller.ReceiveLine(user, message);
        }

        public async Task SynchronizeCanvas(string user, string message)
        {
            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                foreach (var msg in LineDataController.GetData(user))
                {
                    tasks.Add(SendLineToCaller(user, msg));
                }
                Task.WaitAll(tasks.ToArray());
            });
        }
        public async Task AddToGroup(string user, string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            GroupsByConnections.TryAdd(Context.ConnectionId, Clients.Group(groupName));
            await Clients.Group(groupName).SendTextToGroup(user, $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string user, string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Client group;
            GroupsByConnections.Remove(Context.ConnectionId, out group);
            await Clients.Group(groupName).SendTextToGroup("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }
    }

    public interface Client
    {
        Task ReceiveLine(string user, string message);

        Task ReceiveText(string user, string message);
        Task ClearCanvas(string user, string message);
        Task SynchronizeCanvas(string user, string message);

        Task SendTextToGroup(string user, string message);
        Task RemoveFromGroup(string user, string groupName);
        Task AddToGroup(string user, string groupName);
    }
}