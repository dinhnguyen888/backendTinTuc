using MongoDB.Driver;

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

    public async Task DeleteCommentAsync(string commentId, string userId)
    {
        var filter = Builders<Comment>.Filter.Eq(c => c.Id, commentId) &
                     Builders<Comment>.Filter.ElemMatch(c => c.Comments, uc => uc.UserComment.UserId == userId);

        var update = Builders<Comment>.Update.Set(x => x.Comments[-1].UserComment.UserId, (string)null);

        await _collection.UpdateOneAsync(filter, update);
    }
}