// Repositories/YourModelRepository.cs
using MongoDB.Driver;

public class AccountRepository
{
    private readonly IMongoCollection<Account> _collection;

    public AccountRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Account>("Account");
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Account> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Account model)
    {
        await _collection.InsertOneAsync(model);
    }

    public async Task UpdateAsync(string id, Account updatedModel)
    {
        await _collection.ReplaceOneAsync(x => x.Id == id, updatedModel);
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }
}
