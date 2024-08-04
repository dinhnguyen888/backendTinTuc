using backendTinTuc.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CommentRepository
{
    private readonly IMongoCollection<Comment> _collection;

    public CommentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Comment>("Comments");
    }

    public async Task<List<Comment>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Comment> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Comment model)
    {
        await _collection.InsertOneAsync(model);
    }

    public async Task UpdateAsync(string id, Comment updatedModel)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, updatedModel);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task DeleteUserCommentAsync(string commentId, string fromUserId, string toUserId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId) &
                     Builders<Comment>.Filter.ElemMatch(c => c.Comments, uc => uc.FromUserId == fromUserId && uc.ToUserId == toUserId);

        var update = Builders<Comment>.Update.PullFilter(c => c.Comments, uc => uc.FromUserId == fromUserId && uc.ToUserId == toUserId);

        await _collection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteCommentsByNewsIdAsync(string newsId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, newsId);
        await _collection.DeleteManyAsync(filter);
    }
}
