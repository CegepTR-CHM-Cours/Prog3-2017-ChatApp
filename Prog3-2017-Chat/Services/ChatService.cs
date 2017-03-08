using ChatApp.Models;
using ForEvolve.NETCore.Azure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class ChatService : IChatService
    {
        private readonly TableStorageRepository<ChatEntryEntity> ChatEntryRepository;

        public ChatService(TableStorageRepository<ChatEntryEntity> chatEntryRepository)
        {
            ChatEntryRepository = chatEntryRepository ?? throw new ArgumentNullException(nameof(chatEntryRepository));
        }

        public async Task<ChatEntryDto> CreateAsync(CreateChatEntryDto entry)
        {
            var entityToCreate = new ChatEntryEntity
            {
                PartitionKey = "ChatApp-OnlyRoom",
                RowKey = $"{entry.Username}|{DateTime.Now.ToFileTime()}",
                Username = entry.Username,
                Message = entry.Message
            };
            var createdEntity = await ChatEntryRepository.InsertOrMergeAsync(entityToCreate);
            var updatedEntity = await ChatEntryRepository.ReadOneAsync(createdEntity.PartitionKey, createdEntity.RowKey);
            return new ChatEntryDto
            {
                Username = updatedEntity.Username,
                Message = updatedEntity.Message,
                CreatedDate = updatedEntity.Timestamp.DateTime
            };
        }

        public async Task<IEnumerable<ChatEntryDto>> GetAsync()
        {
            var all = await ChatEntryRepository.ReadAllAsync();
            return all.Select(entity => new ChatEntryDto
            {
                Username = entity.Username,
                Message = entity.Message,
                CreatedDate = entity.Timestamp.DateTime
            });
        }
    }

    public interface IChatService
    {
        Task<IEnumerable<ChatEntryDto>> GetAsync();
        Task<ChatEntryDto> CreateAsync(CreateChatEntryDto entry);
    }

}
