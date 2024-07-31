using backendTinTuc.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace backendTinTuc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrawlController : ControllerBase
    {
        private readonly CrawlingData _crawlingData;

        public CrawlController(CrawlingData crawlingData)
        {
            _crawlingData = crawlingData;
        }

        [HttpGet("start")]
        public IActionResult StartCrawling()
        {
            _crawlingData.StartCrawling();

            if (_crawlingData.IsCrawlingSuccessful)
            {
                return Ok("Crawling completed successfully.");
            }
            else
            {
                return StatusCode(500, "Crawling failed.");
            }
        }

        [HttpGet("latest")]
        public IActionResult GetLatestData()
        {
            try
            {
                _crawlingData.GetLatestData();
                return Ok("Latest data fetched and stored successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("update-sections")]
        public IActionResult UpdateSectionList([FromBody] List<string> sections)
        {
            if (sections == null || sections.Count == 0)
            {
                return BadRequest("Section list cannot be empty.");
            }

            _crawlingData.UpdateSectionList(sections);
            return Ok("Section list updated successfully.");
        }

        // New method to get the current categories
        [HttpGet("sections")]
        public IActionResult GetCategories()
        {
            var categories = _crawlingData.GetSections();
            return Ok(categories);
        }
    }
}
