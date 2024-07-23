using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly CommentRepository _commentRepository;

    public CommentsController(CommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Comment>>> Get()
    {
        var comments = await _commentRepository.GetAllAsync();
        return Ok(comments);
    }

    [HttpGet("{id:length(24)}", Name = "GetComment")]
    public async Task<ActionResult<Comment>> Get(string id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
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
    [HttpDelete("{commentId:length(24)}/{userId}")]
    public async Task<IActionResult> Delete(string commentId, string userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            return NotFound();
        }

        await _commentRepository.DeleteCommentAsync(commentId, userId);
        return NoContent();
    }
}
