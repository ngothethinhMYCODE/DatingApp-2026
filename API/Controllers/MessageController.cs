using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IMessageRespository messageRespository, IMemberRepository memberRepository): BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var sender =await memberRepository.GetMemberByIdAsync(User.GetMemberId());
        var recipient=await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if(recipient==null || sender==null || sender.Id== createMessageDto.RecipientId)
            return BadRequest("Cannot send this message");

        var message=new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderId=sender.Id,
            RecipientId=recipient.Id,
            Content=createMessageDto.Content,
        };
        messageRespository.AddMessage(message);

        if(await messageRespository.SaveAllAsync()) return message.ToDto();
        return BadRequest("Failed to send message");
    } 
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMemberByContainer([FromQuery] MessageParams messageParams)
    {
        messageParams.MemberId=User.GetMemberId();
        return await messageRespository.GetMessagesForMember(messageParams);
    }
    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        return Ok(await messageRespository.GetMessagesThread(User.GetMemberId(),recipientId));
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var memberId=User.GetMemberId();
        var message= await messageRespository.GetMessage(id);
        if(message==null)return BadRequest("Cannot delete this message");
        if(message.SenderId!=memberId && message.RecipientId != memberId) 
            return BadRequest("You cannot delete this message");
        if(message.SenderId==memberId) message.SenderDeleted=true;
        if(message.RecipientId==memberId) message.RecipientDeleted=true;

        if(message is {SenderDeleted: true, RecipientDeleted: true })
        {
            messageRespository.DeleteMessage(message);
        }
        if(await messageRespository.SaveAllAsync()) return Ok();
        return BadRequest("Problem deleting the message");
    }
}
