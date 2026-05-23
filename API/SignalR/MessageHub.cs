using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRespository messageRespository, IMemberRepository memberRepository,
    IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request?.Query["userId"].ToString()
            ?? throw new HubException("Other user not found");

        var groupName = GetGroupName(GetUserId(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName);
        var messages = await messageRespository.GetMessagesThread(GetUserId(), otherUser);

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        await base.OnConnectedAsync();
    }
    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var sender = await memberRepository.GetMemberByIdAsync(GetUserId());
        var recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
            throw new HubException("Cannot send message");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content,
        };

        var groupName=GetGroupName(sender.Id, recipient.Id);
        var group= await messageRespository.GetMessageGroup(groupName);
        var userInGroup= group!=null && group.Connections.Any(x => x.UserId == message.RecipientId);
        if(userInGroup)
        {
            message.DataRead=DateTime.UtcNow;
        }
        messageRespository.AddMessage(message);

        if (await messageRespository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            var connections= await PresenceTracker.GetConnectionForUser(recipient.Id);
            if(connections!=null && connections.Count>0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections).
                    SendAsync("NewMessageReceived", message.ToDto());
            }
        }

    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await messageRespository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await messageRespository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if (group == null)
        {
            group = new Group(groupName);
            messageRespository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await messageRespository.SaveAllAsync();
    }
    private static string GetGroupName(string? caller, string? otherUser)
    {
        var stringCompare = string.CompareOrdinal(caller, otherUser) < 0;
        return stringCompare ? $"{caller}-{otherUser}" : $"{otherUser}-{caller}";
    }
    private string GetUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Cannot get member id");
    }
}
