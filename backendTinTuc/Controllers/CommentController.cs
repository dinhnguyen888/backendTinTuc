using backendTinTuc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly CommentRepository _commentRepository;
    private readonly AccountRepository _accountRepository;

    public CommentsController(CommentRepository commentRepository, AccountRepository accountRepository)
    {
        _commentRepository = commentRepository;
        _accountRepository = accountRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<object>>> Get()
    {
        var comments = await _commentRepository.GetAllAsync();
        var commentsWithUserNames = new List<object>();

        foreach (var comment in comments)
        {
            var commentWithNames = new
            {
                comment.Id,
                Comments = await Task.WhenAll(comment.Comments.Select(async c =>
                {
                    var fromUser = await _accountRepository.GetByIdAsync(c.FromUserId);
                    var toUser = await _accountRepository.GetByIdAsync(c.ToUserId);

                    return new
                    {
                        c.Content,
                        FromUserId = c.FromUserId,
                        FromUserName = fromUser?.Name,
                        ToUserId = c.ToUserId,
                        ToUserName = toUser?.Name,
                        c.CreateAt
                    };
                }))
            };

            commentsWithUserNames.Add(commentWithNames);
        }

        return Ok(commentsWithUserNames);
    }

    [HttpGet("{id:length(24)}", Name = "GetComment")]
    public async Task<ActionResult<object>> Get(string id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        var commentWithNames = new
        {
            comment.Id,
            Comments = await Task.WhenAll(comment.Comments.Select(async c =>
            {
                var fromUser = await _accountRepository.GetByIdAsync(c.FromUserId);
                var toUser = await _accountRepository.GetByIdAsync(c.ToUserId);

                return new
                {
                    c.Content,
                    FromUserId = c.FromUserId,
                    FromUserName = fromUser?.Name,
                    ToUserId = c.ToUserId,
                    ToUserName = toUser?.Name,
                    c.CreateAt
                };
            }))
        };

        return Ok(commentWithNames);
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> Create(Comment comment)
    {
        await _commentRepository.CreateAsync(comment);
        return CreatedAtRoute("GetComment", new { id = comment.Id.ToString() }, comment);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Comment updatedComment)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        updatedComment.Id = id;
        await _commentRepository.UpdateAsync(id, updatedComment);
        return NoContent();
    }

    [Authorize(Roles = "Admin, User")]
    [HttpDelete("{commentId:length(24)}/{fromUserId}/{toUserId}")]
    public async Task<IActionResult> DeleteUserComment(string commentId, string fromUserId, string toUserId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            return NotFound();
        }

        await _commentRepository.DeleteUserCommentAsync(commentId, fromUserId, toUserId);
        return NoContent();
    }

    [HttpPost("add-comment")]
    public async Task<IActionResult> AddComment([FromBody] CommentDTO commentDto)
    {
        var comment = await _commentRepository.GetByIdAsync(commentDto.NewsId);
        if (comment == null)
        {
            return NotFound();
        }

        var userCommentDetails = new UserCommentDetails
        {
            FromUserId = commentDto.FromUserId,
            ToUserId = commentDto.ToUserId,
            Content = commentDto.Content,
            CreateAt = DateTime.UtcNow
        };

        comment.Comments.Add(userCommentDetails);
        await _commentRepository.UpdateAsync(commentDto.NewsId, comment);

        return Ok(userCommentDetails);
    }

    [HttpPost("remove-comment")]
    public async Task<IActionResult> RemoveComment([FromBody] CommentDTO commentDto)
    {
        var comment = await _commentRepository.GetByIdAsync(commentDto.NewsId);
        if (comment == null)
        {
            return NotFound();
        }

        var originalCount = comment.Comments.Count;
        comment.Comments.RemoveAll(c => c.FromUserId == commentDto.FromUserId && c.ToUserId == commentDto.ToUserId && c.Content == commentDto.Content);

        if (comment.Comments.Count == originalCount)
        {
            return NotFound("Comment not found for the provided users.");
        }

        await _commentRepository.UpdateAsync(commentDto.NewsId, comment);

        return Ok();
    }
}
