using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ForumController : ControllerBase
{
    private readonly WebSocketServerService _webSocketService;
    private readonly CommentRepository _commentRepository;

    public ForumController(WebSocketServerService webSocketService, CommentRepository commentRepository)
    {
        _webSocketService = webSocketService;
        _commentRepository = commentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Comment>>> GetAllComments()
    {
        var comments = await _commentRepository.GetAllAsync();
        return Ok(comments);
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost]
    public async Task<ActionResult> PostComment([FromBody] UserCommentDetails userComment)
    {
        var comment = new Comment
        {
            PageNews = "Forum",
            Comments = new List<UserCommentDetails> { userComment }
        };

        await _commentRepository.CreateAsync(comment);

        await _webSocketService.BroadcastMessageAsync(userComment.Content);

        return Ok();
    }
}
