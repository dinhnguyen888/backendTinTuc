using backendTinTuc.Models;
using backendTinTuc.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace backendTinTuc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsRepository _newsRepository;
        private readonly CommentRepository _commentRepository;

        public NewsController(INewsRepository newsRepository, CommentRepository commentRepository)
        {
            _newsRepository = newsRepository;
            _commentRepository = commentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var news = await _newsRepository.GetAllNewsAsync();
            var newsDTOs = news.Select(n => new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                LinkDetail = n.LinkDetail,
                ImageUrl = n.ImageUrl,
                Description = n.Description,
                Content = n.Content,
                Type = n.Type
            });
            return Ok(newsDTOs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            var news = await _newsRepository.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            var newsDTO = new NewsDTO
            {
                Title = news.Title,
                LinkDetail = news.LinkDetail,
                ImageUrl = news.ImageUrl,
                Description = news.Description,
                Content = news.Content,
                Type = news.Type
            };
            return Ok(newsDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateNews([FromBody] NewsDTO newsDTO)
        {
            if (newsDTO == null)
            {
                return BadRequest();
            }

            var news = new News
            {
                Title = newsDTO.Title,
                LinkDetail = newsDTO.LinkDetail,
                ImageUrl = newsDTO.ImageUrl,
                Description = newsDTO.Description,
                Content = newsDTO.Content,
                Type = newsDTO.Type
            };

            // Insert news into the database
            await _newsRepository.CreateNewsAsync(news);

            // Create an empty Comment model associated with the news ID
            var comment = new Comment
            {
                Id = news.Id, // Use the same ID as the news item
                Comments = new List<UserCommentDetails>() // Empty comments list
            };

            // Insert the comment model into the database
            await _commentRepository.CreateAsync(comment);

            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(string id, [FromBody] NewsDTO newsDTO)
        {
            if (newsDTO == null)
            {
                return BadRequest();
            }

            var news = new News
            {
                Id = id,
                Title = newsDTO.Title,
                LinkDetail = newsDTO.LinkDetail,
                ImageUrl = newsDTO.ImageUrl,
                Description = newsDTO.Description,
                Content = newsDTO.Content,
                Type = newsDTO.Type
            };

            var updated = await _newsRepository.UpdateNewsAsync(id, news);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            var deleted = await _newsRepository.DeleteNewsAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            // Optionally, you can also delete the associated comments
            await _commentRepository.DeleteCommentsByNewsIdAsync(id);

            return Ok();
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetNewsByType(string type)
        {
            var news = await _newsRepository.GetNewsByTypeAsync(type);
            var newsDTOs = news.Select(n => new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                LinkDetail = n.LinkDetail,
                ImageUrl = n.ImageUrl,
                Description = n.Description,
                Content = n.Content,
                Type = n.Type
            });
            return Ok(newsDTOs);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchByTitle([FromQuery] string title)
        {
            var news = await _newsRepository.SearchNewsByTitleAsync(title);
            var newsDTOs = news.Select(n => new NewsDTO
            {
                Id = n.Id,
                Title = n.Title,
                LinkDetail = n.LinkDetail,
                ImageUrl = n.ImageUrl,
                Description = n.Description,
                Content = n.Content,
                Type = n.Type
            });
            return Ok(newsDTOs);
        }
    }
}
