using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using backendTinTuc.Models;

namespace backendTinTuc.Service
{
    public class CrawlingData
    {
        private readonly IMongoDatabase _database;
        private readonly CommentRepository _commentRepository; // Add CommentRepository
        private readonly string baseUrl = "https://vnexpress.net/";
        private List<string> sectionList; // Global list for categories

        public bool IsCrawlingSuccessful { get; private set; }

        public CrawlingData(IMongoDatabase database, CommentRepository commentRepository)
        {
            _database = database;
            _commentRepository = commentRepository; // Initialize CommentRepository
            sectionList = new List<string> { "chinh-tri", "dan-sinh", "lao-dong-viec-lam", "giao-thong" };
            IsCrawlingSuccessful = false;
        }

        // Method to update the category list
        public void UpdateSectionList(List<string> section)
        {
            sectionList = section;
            Console.WriteLine("Section list updated.");
        }

        public List<string> GetSections()
        {
            return sectionList;
        }

        public void StartCrawling()
        {
            try
            {
                var listDataExport = new List<News>();
                var newsCollection = _database.GetCollection<News>("News");

                foreach (var section in sectionList)
                {
                    var requestUrl = baseUrl + $"thoi-su/{section}";
                    Console.WriteLine($"Loading URL: {requestUrl}");
                    var document = LoadDocument(requestUrl);
                    Console.WriteLine("Page loaded successfully.");

                    // Define the number of pages to crawl
                    const int totalPage = 1;

                    for (var i = 1; i <= totalPage; i++)
                    {
                        var requestPerPage = baseUrl + $"thoi-su/{section}-p{i}";
                        Console.WriteLine($"Processing page {i}: {requestPerPage}");
                        var documentForListItem = LoadDocument(requestPerPage);

                        var listNodeProductItem = documentForListItem.DocumentNode.QuerySelectorAll(".item-news.thumb-left.item-news-common");
                        if (listNodeProductItem != null)
                        {
                            foreach (var node in listNodeProductItem)
                            {
                                var newsItem = ParseNewsItem(node, section);

                                if (newsItem != null)
                                {
                                    // Check if the news item already exists in the database
                                    var existingNews = newsCollection.Find(n => n.LinkDetail == newsItem.LinkDetail).FirstOrDefault();
                                    if (existingNews == null)
                                    {
                                        listDataExport.Add(newsItem);
                                        // Insert news item into the database
                                        newsCollection.InsertOne(newsItem);
                                        Console.WriteLine("News item inserted into MongoDB.");

                                        // Create and insert an empty Comment model with the same ID as the news item
                                        var comment = new Comment
                                        {
                                            Id = newsItem.Id,
                                            Comments = new List<UserCommentDetails>()
                                        };
                                        _commentRepository.CreateAsync(comment).Wait(); // Ensure the task is completed synchronously
                                        Console.WriteLine("Comment model created for the news item.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Duplicate news item found: {newsItem.LinkDetail}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No news items found on page {i} for category {section}");
                        }
                    }
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

        public void GetLatestData()
        {
            try
            {
                foreach (var section in sectionList)
                {
                    var requestUrl = baseUrl + $"thoi-su/{section}";
                    var document = LoadDocument(requestUrl);
                    var latestNode = document.DocumentNode.QuerySelector(".item-news.thumb-left.item-news-common:first-child");

                    if (latestNode != null)
                    {
                        var latestNews = ParseNewsItem(latestNode, section);

                        if (latestNews != null)
                        {
                            var newsCollection = _database.GetCollection<News>("News");

                            // Check if the news item already exists in the database
                            var existingNews = newsCollection.Find(n => n.LinkDetail == latestNews.LinkDetail).FirstOrDefault();

                            if (existingNews == null)
                            {
                                newsCollection.InsertOne(latestNews);
                                Console.WriteLine("Inserted latest data into MongoDB.");

                                // Create and insert an empty Comment model with the same ID as the news item
                                var comment = new Comment
                                {
                                    Id = latestNews.Id,
                                    Comments = new List<UserCommentDetails>()
                                };
                                _commentRepository.CreateAsync(comment).Wait(); // Ensure the task is completed synchronously
                                Console.WriteLine("Comment model created for the latest news item.");
                            }
                            else
                            {
                                Console.WriteLine("The latest data already exists in MongoDB.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No latest news item found.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during data retrieval: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private HtmlDocument LoadDocument(string url)
        {
            var web = new HtmlWeb()
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };
            return web.Load(url);
        }

        private News ParseNewsItem(HtmlNode node, string category)
        {
            var thumbArtNode = node.QuerySelector(".thumb-art > a");
            var thumbImgNode = node.QuerySelector(".thumb-art > a > picture > img");

            var linkDetail = thumbArtNode?.Attributes["href"]?.Value;
            var title = thumbArtNode?.Attributes["title"]?.Value;
            var imgSrc = thumbImgNode?.Attributes["src"]?.Value;

            if (string.IsNullOrEmpty(linkDetail))
            {
                return null;
            }

            var fullDetailLink = linkDetail.StartsWith("http") ? linkDetail : baseUrl + linkDetail;
            var detailDocument = LoadDocument(fullDetailLink);
            Console.WriteLine($"Loaded detail page: {fullDetailLink}");

            var descriptionNode = detailDocument.DocumentNode.QuerySelector(".description");
            var fckDetailNodes = detailDocument.DocumentNode.SelectNodes("//p");

            var content = new StringBuilder();
            if (fckDetailNodes != null)
            {
                foreach (var fckDetailNode in fckDetailNodes)
                {
                    var cleanNode = HtmlNode.CreateNode("<p>" + fckDetailNode.InnerHtml + "</p>");
                    content.AppendLine(cleanNode.OuterHtml);
                }
            }

            return new News()
            {
                Title = title?.RemoveBreakLineTab(),
                LinkDetail = linkDetail,
                ImageUrl = imgSrc,
                Description = descriptionNode?.InnerText.RemoveBreakLineTab(),
                Content = content.ToString().Trim(),
                Type = category
            };
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
