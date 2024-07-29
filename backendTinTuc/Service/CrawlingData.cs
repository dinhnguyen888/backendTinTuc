using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using backendTinTuc.Models; // Đảm bảo namespace của lớp News được import

namespace backendTinTuc.Service
{
    public class CrawlingData
    {
        private readonly IMongoDatabase _database;
        private readonly string baseUrl = "https://vnexpress.net/";
        public bool IsCrawlingSuccessful { get; private set; }

        public CrawlingData(IMongoDatabase database)
        {
            _database = database;
            IsCrawlingSuccessful = false;
        }

        public void StartCrawling()
        {
            try
            {
                var web = new HtmlWeb()
                {
                    AutoDetectEncoding = false,
                    OverrideEncoding = Encoding.UTF8
                };

                var ThoiSuList = new List<string> { "chinh-tri", "dan-sinh", "lao-dong-viec-lam", "giao-thong" };
                var listDataExport = new List<News>(); // Sử dụng News thay vì NewsCrawler

                foreach (var thoiSu in ThoiSuList)
                {
                    var requestUrl = baseUrl + $"thoi-su/{thoiSu}";
                    Console.WriteLine($"Loading URL: {requestUrl}");
                    var documentForGetTotalPageMale = web.Load(requestUrl);
                    Console.WriteLine("Page loaded successfully.");

                   // var textTotalPageNode = documentForGetTotalPageMale.DocumentNode.QuerySelector(".button-page.flexbox .btn-page:last-child");

                  //  var textTotalPage = textTotalPageNode != null ? textTotalPageNode.InnerText.RemoveBreakLineTab() : "1";

                  //  if (!int.TryParse(textTotalPage, out int totalPage))
                 //   {
                        const int totalPage = 1; // ở mỗi chuyên mục lấy totalPage trang
                 //   }

                    Console.WriteLine($"Total pages: {totalPage}");

                    for (var i = 1; i <= totalPage; i++)
                    {
                        var requestPerPage = baseUrl + $"thoi-su/{thoiSu}-p{i}";
                        Console.WriteLine($"Processing page {i}: {requestPerPage}");
                        var documentForListItem = web.Load(requestPerPage);

                        var listNodeProductItem = documentForListItem.DocumentNode.QuerySelectorAll(".item-news.thumb-left.item-news-common");

                        if (listNodeProductItem != null)
                        {
                            foreach (var node in listNodeProductItem)
                            {
                                var thumbArtNode = node.QuerySelector(".thumb-art > a");
                                var thumbImgNode = node.QuerySelector(".thumb-art > a > picture > img");

                                var linkDetail = thumbArtNode?.Attributes["href"]?.Value;
                                var title = thumbArtNode?.Attributes["title"]?.Value;
                                var imgSrc = thumbImgNode?.Attributes["src"]?.Value;

                                if (!string.IsNullOrEmpty(linkDetail))
                                {
                                    var fullDetailLink = linkDetail.StartsWith("http") ? linkDetail : baseUrl + linkDetail;
                                    var detailDocument = web.Load(fullDetailLink);
                                    Console.WriteLine($"Loaded detail page: {fullDetailLink}");

                                    var descriptionNode = detailDocument.DocumentNode.QuerySelector(".description");
                                    var fckDetailNodes = detailDocument.DocumentNode.QuerySelectorAll(".fck_detail .Normal");

                                    var content = new StringBuilder();
                                    if (fckDetailNodes != null)
                                    {
                                        foreach (var fckDetailNode in fckDetailNodes)
                                        {
                                            content.AppendLine(fckDetailNode.OuterHtml); // Lấy toàn bộ nội dung HTML
                                        }
                                    }

                                    listDataExport.Add(new News()
                                    {
                                        Title = title?.RemoveBreakLineTab(),
                                        LinkDetail = linkDetail,
                                        ImageUrl = imgSrc,
                                        Description = descriptionNode?.InnerText.RemoveBreakLineTab(),
                                        Content = content.ToString(),
                                        Type = thoiSu // Thêm kiểu vào trường Type
                                    });
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No news items found on page {i} for category {thoiSu}");
                        }
                    }
                }

                if (listDataExport.Count > 0)
                {
                    var newsCollection = _database.GetCollection<News>("News");
                    newsCollection.InsertMany(listDataExport);
                    Console.WriteLine("Data inserted into MongoDB.");
                }
                else
                {
                    Console.WriteLine("No data to export. The list is empty.");
                }

                IsCrawlingSuccessful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during crawling: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                IsCrawlingSuccessful = false;
            }
        }
    }
}

public static class StringExtensions
{
    public static string RemoveBreakLineTab(this string input)
    {
        return input.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
    }
}
