using backendTinTuc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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
                        c.CommentId,
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
                    c.CommentId,
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

    //[HttpPost]
    //public async Task<ActionResult<Comment>> Create(Comment comment)
    //{
    //    await _commentRepository.CreateAsync(comment);
    //    return CreatedAtRoute("GetComment", new { id = comment.Id.ToString() }, comment);
    //}

    //[HttpPut("{id:length(24)}")]
    //public async Task<IActionResult> Update(string id, Comment updatedComment)
    //{
    //    var comment = await _commentRepository.GetByIdAsync(id);
    //    if (comment == null)
    //    {
    //        return NotFound();
    //    }

    //    updatedComment.Id = id;
    //    await _commentRepository.UpdateAsync(id, updatedComment);
    //    return NoContent();
    //}

    //[Authorize(Roles = "Admin")]
    //[HttpDelete("{commentId:length(24)}/{fromUserId}/{toUserId}")]
    //public async Task<IActionResult> DeleteUserComment(string commentId, string fromUserId, string toUserId)
    //{
    //    var comment = await _commentRepository.GetByIdAsync(commentId);
    //    if (comment == null)
    //    {
    //        return NotFound();
    //    }

    //    await _commentRepository.DeleteUserCommentAsync(commentId, fromUserId, toUserId);
    //    return NoContent();
    //}

    [HttpPost("add-comment")]
    public async Task<IActionResult> AddComment([FromBody] CommentDTO commentDto)
    {
        var commentInNews = await _commentRepository.GetByIdAsync(commentDto.NewsId);
        if (commentInNews == null)
        {
            return NotFound();
        }

        var userCommentDetails = new UserCommentDetails
        {
            CommentId = ObjectId.GenerateNewId().ToString(),
            FromUserId = commentDto.FromUserId,
            ToUserId = commentDto.ToUserId,
            Content = commentDto.Content,
            ToCommentId = commentDto.ToCommentId,
            CreateAt = DateTime.UtcNow
        };

        var index = commentInNews.Comments.FindIndex(c => c.CommentId == userCommentDetails.ToCommentId);
        if (index != -1)
        {
            // Chèn comment mới ngay sau comment khớp với CommentId
            commentInNews.Comments.Insert(index + 1, userCommentDetails);
        }
        else
        {
            // Thêm comment mới vào cuối danh sách nếu không tìm thấy khớp
            commentInNews.Comments.Add(userCommentDetails);
        }
       
        await _commentRepository.UpdateAsync(commentDto.NewsId, commentInNews);

        return Ok(userCommentDetails);
    }


    [HttpPost("remove-comment")]
    public async Task<IActionResult> RemoveComment([FromBody] string commentId)
    {
        // Bước 1: Xóa CommentId cụ thể
        var filter = Builders<Comment>.Filter.ElemMatch(c => c.Comments, uc => uc.CommentId == commentId);
        var update = Builders<Comment>.Update.PullFilter(c => c.Comments, uc => uc.CommentId == commentId);

        var result = await _commentRepository.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
        {
            return NotFound("Comment not found.");
        }

        // Bước 2: Xóa tất cả các bình luận có ToCommentId trùng với commentId đã bị xóa
        var filterReplies = Builders<Comment>.Filter.ElemMatch(c => c.Comments, uc => uc.ToCommentId == commentId);
        var updateReplies = Builders<Comment>.Update.PullFilter(c => c.Comments, uc => uc.ToCommentId == commentId);

        var resultReplies = await _commentRepository.UpdateOneAsync(filterReplies, updateReplies);

        return Ok();

    }
       [HttpPost("delete-comment")]
    public async Task<IActionResult> DeleteComment([FromBody] string id)
    {
        var filter = Builders<Comment>.Filter.ElemMatch(c => c.Comments, uc => uc.CommentId == id);
        var update = Builders<Comment>.Update.Set("Comments.$.Content", "tin nhắn đã bị xóa");

        var result = await _commentRepository.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
        {
            return NotFound("Comment not found.");
        }

        return Ok();
    }
}
