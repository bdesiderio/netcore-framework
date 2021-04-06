using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Data.MongoDB
{
    public abstract class MongoRepository<TEntity, TKey> : IDataRepository<TEntity, TKey>
         where TEntity : class, new()
    {
        private readonly Func<TEntity, TKey> _getKeyValue;
        IMongoDatabase _db;

        public MongoRepository(IOptions<MongoDbSettings> settings)
        {
            var url = new MongoUrl(settings.Value.MongoURL);

            var mongoClientSettings = new MongoClientSettings
            {
                Credential = url.AuthenticationMechanism != null &&
                !String.IsNullOrEmpty(url.Username) && !String.IsNullOrEmpty(url.Password) ? new MongoCredential(
                    url.AuthenticationMechanism,
                    new MongoInternalIdentity("admin", url.Username),
                    new PasswordEvidence(url.Password)) : null,

                Server = url.Server,
            };

            _db = new MongoClient(mongoClientSettings).GetDatabase(url.DatabaseName);

            if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
            {
                CustomClassMap(BsonClassMap.RegisterClassMap<TEntity>(RegisterMappings));
            }

            _getKeyValue = KeyField.Compile();
        }

        public abstract MongoDbSettings DataSource { get; }
        public abstract Expression<Func<TEntity, TKey>> KeyField { get; }

        public virtual string CollectionName => typeof(TEntity).Name;
        public virtual void CustomClassMap(BsonClassMap<TEntity> bsonClassMap) { }

        protected IMongoCollection<TEntity> Collection => GetCollection<TEntity>(CollectionName);
        protected IMongoQueryable<TEntity> QueryableCollection => GetCollection<TEntity>(CollectionName).AsQueryable();


        public async Task<int> CountAsync()
        {
            return (int)await Collection.EstimatedDocumentCountAsync();
        }

        public async Task<TEntity> GetAsync(TKey key)
        {
            return await (await Collection.FindAsync(Builders<TEntity>.Filter.Eq(KeyField, key))).FirstAsync();
        }

        public async Task<TEntity> TryGetAsync(TKey key)
        {
            return await (await Collection.FindAsync(Builders<TEntity>.Filter.Eq(KeyField, key))).FirstOrDefaultAsync();
        }

        public async Task<IList<TEntity>> GetAllAsync()
        {
            return await Collection.AsQueryable().ToListAsync();
        }

        public async Task<Paginable<TEntity>> GetAllAsync(int currentPage, int pageSize)
        {
            return
                new Paginable<TEntity>(currentPage, await Collection.AsQueryable().CountAsync(), pageSize,
                await Collection.AsQueryable().Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        public async Task AddAsync(TEntity entity)
        {
            await OnAddingAsync(entity);
            await Collection.InsertOneAsync(entity);
            await OnAddedAsync(entity);
        }

        public async Task<IEnumerable<TEntity>> AddManyAsync(IEnumerable<TEntity> entities)
        {
            await OnAddingAsync(entities);
            await Collection.InsertManyAsync(entities);
            await OnAddedAsync(entities);
            return entities;
        }

        public async Task RemoveAsync(TKey key)
        {
            //await OnRemovingAsync(key);
            await Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(KeyField, key));
            //await OnRemovedAsync(key);
        }

        protected IMongoCollection<TCollectionEntity> GetCollection<TCollectionEntity>(string collectionName = null)
        {
            return _db.GetCollection<TCollectionEntity>(collectionName ?? typeof(TCollectionEntity).Name);
        }

        protected virtual void RegisterMappings(BsonClassMap<TEntity> mapping)
        {
            mapping.MapIdField(KeyField);
            mapping.AutoMap();
            mapping.SetIgnoreExtraElements(true);
            mapping.SetIgnoreExtraElementsIsInherited(true);
        }

        protected virtual Task OnAddingAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnAddedAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnAddingAsync(IEnumerable<TEntity> entities) => Task.CompletedTask;
        protected virtual Task OnAddedAsync(IEnumerable<TEntity> entities) => Task.CompletedTask;
        protected virtual Task OnUpdatingAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnUpdatedAsync(TEntity entity) => Task.CompletedTask;
        protected virtual Task OnRemovingAsync(TEntity key) => Task.CompletedTask;
        protected virtual Task OnRemovedAsync(TEntity key) => Task.CompletedTask;
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public async Task UpdateAsync(TEntity entity)
        {
            await OnUpdatingAsync(entity);
            await Collection.ReplaceOneAsync(Builders<TEntity>.Filter.Eq(KeyField, _getKeyValue(entity)), entity);
            await OnUpdatedAsync(entity);
        }


        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await OnUpdatingAsync(entity);
                await UpdateAsync(entity);
                await OnUpdatedAsync(entity);
            }
        }

        public async Task RemoveAsync(TEntity entity)
        {
            await OnRemovingAsync(entity);
            await Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(KeyField, _getKeyValue(entity)));
            await OnRemovedAsync(entity);
        }

        public Task RemoveRangeAsync(IEnumerable<TEntity> entity)
        {
            throw new NotImplementedException();
        }
    }
}
