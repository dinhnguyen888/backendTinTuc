using backendTinTuc.Models;
using backendTinTuc.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backendTinTuc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsRepository _newsRepository;

        public NewsController(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var news = await _newsRepository.GetAllNewsAsync();
            var newsDTOs = news.Select(n => new NewsDTO
            {
                Source = new SourceDTO { Id = n.Source.Id, Name = n.Source.Name },
                Author = n.Author,
                Title = n.Title,
                Description = n.Description,
                Url = n.Url,
                UrlToImage = n.UrlToImage,
                PublishedAt = n.PublishedAt,
                Content = n.Content
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
                Source = new SourceDTO { Id = news.Source.Id, Name = news.Source.Name },
                Author = news.Author,
                Title = news.Title,
                Description = news.Description,
                Url = news.Url,
                UrlToImage = news.UrlToImage,
                PublishedAt = news.PublishedAt,
                Content = news.Content
            };
            return Ok(newsDTO);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNews([FromBody] NewsDTO newsDTO)
        {
            if (newsDTO == null)
            {
                return BadRequest();
            }

            var news = new News
            {
                Source = new Source { Id = newsDTO.Source.Id, Name = newsDTO.Source.Name },
                Author = newsDTO.Author,
                Title = newsDTO.Title,
                Description = newsDTO.Description,
                Url = newsDTO.Url,
                UrlToImage = newsDTO.UrlToImage,
                PublishedAt = newsDTO.PublishedAt,
                Content = newsDTO.Content
            };

            await _newsRepository.CreateNewsAsync(news);
            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        }

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
                Source = new Source { Id = newsDTO.Source.Id, Name = newsDTO.Source.Name },
                Author = newsDTO.Author,
                Title = newsDTO.Title,
                Description = newsDTO.Description,
                Url = newsDTO.Url,
                UrlToImage = newsDTO.UrlToImage,
                PublishedAt = newsDTO.PublishedAt,
                Content = newsDTO.Content
            };

            var updated = await _newsRepository.UpdateNewsAsync(id, news);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            var deleted = await _newsRepository.DeleteNewsAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
