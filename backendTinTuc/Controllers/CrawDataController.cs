using backendTinTuc.Service;
using Microsoft.AspNetCore.Mvc;

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

            // Check if the crawling was successful
            if (_crawlingData.IsCrawlingSuccessful)
            {
                return Ok("Crawling completed successfully.");
            }
            else
            {
                return StatusCode(500, "Crawling failed.");
            }
        }
    }
}
