using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using backendTinTuc.Models;

namespace backendTinTuc.Repositories
{
    public interface INewsRepository
    {
        Task<IEnumerable<News>> GetAllNewsAsync();
        Task<News> GetNewsByIdAsync(string id);
        Task CreateNewsAsync(News news);
        Task<bool> UpdateNewsAsync(string id, News news);
        Task<bool> DeleteNewsAsync(string id);
    }

    public class NewsRepository : INewsRepository
    {
        private readonly IMongoCollection<News> _newsCollection;
        private readonly CommentRepository _commentRepository;

        public NewsRepository(IMongoDatabase database, CommentRepository commentRepository)
        {
            _newsCollection = database.GetCollection<News>("News");
            _commentRepository = commentRepository;
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync()
        {
            return await _newsCollection.Find(news => true).ToListAsync();
        }

        public async Task<News> GetNewsByIdAsync(string id)
        {
            return await _newsCollection.Find(news => news.Id == id).FirstOrDefaultAsync();
        }

    
        public async Task CreateNewsAsync(News news)
        {
            await _newsCollection.InsertOneAsync(news);
        }

        public async Task<bool> UpdateNewsAsync(string id, News news)
        {
            var result = await _newsCollection.ReplaceOneAsync(n => n.Id == id, news);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNewsAsync(string id)
        {
            var result = await _newsCollection.DeleteOneAsync(news => news.Id == id);
            if (result.IsAcknowledged && result.DeletedCount > 0)
            {
                // Delete all comments related to the news
                await _commentRepository.DeleteCommentsByNewsIdAsync(id);
                return true;
            }
            return false;
        }
    }
}
