using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using InventoryCommon;
using InventoryRepository.ServiceFabric;

namespace InventoryRepository.Controllers
{
    [RoutePrefix("api/inventory")]
    public class InventoryController : ApiController
    {
        private readonly IReliableStateManager _stateManager;

        public InventoryController()
        {
            _stateManager = WebHostStartup.StateManager;
        }

        [HttpGet]
        [Route("")]
        public async Task<IEnumerable<InventoryItem>> GetItems()
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                var inventoryDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, InventoryItem>>("inventoryDictionary");
                var items = await inventoryDictionary.CreateEnumerableAsync(tx);

                var result = new List<InventoryItem>();
                using (var enumerator = items.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        result.Add(enumerator.Current.Value);
                    }
                    return result;
                }
            }
        }

        [HttpGet]
        [Route("{itemId}")]
        public async Task<InventoryItem> GetItem(Guid itemId)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                var inventoryDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, InventoryItem>>("inventoryDictionary");
                var item = await inventoryDictionary.TryGetValueAsync(tx, itemId);
                return item.HasValue ? item.Value : null;
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<InventoryItem> AddNewItem(InventoryItem item)
        {
            using (var tx = _stateManager.CreateTransaction())
            {
                var inventoryDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, InventoryItem>>("inventoryDictionary");
                if (item.ItemId == Guid.Empty) item.ItemId = Guid.NewGuid();
                await inventoryDictionary.AddAsync(tx, item.ItemId, item);
                await tx.CommitAsync();
            }
            return item;
        }

        [HttpPost]
        [Route("{itemId}/addinventory/{quantity}")]
        public Task AddInventory(Guid itemId, Int32 quantity)
        {
            if (quantity < 0) throw new ArgumentException("Quantity must be a positive number", nameof(quantity));
            return UpdateInventoryAsync(itemId, quantity);
        }

        [HttpPost]
        [Route("{itemId}/removeinventory/{quantity}")]
        public Task RemoveInventory(Guid itemId, Int32 quantity)
        {
            if (quantity < 0) throw new ArgumentException("Quantity must be a positive number", nameof(quantity));
            return UpdateInventoryAsync(itemId, -1 * quantity);
        }

        private async Task UpdateInventoryAsync(Guid itemId, Int32 quantity)
        {
            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                // Use the user’s name to look up their data
                var inventoryDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<Guid, InventoryItem>>("inventoryDictionary");

                var originalItem = await inventoryDictionary.TryGetValueAsync(tx, itemId);
                if (originalItem.HasValue)
                {
                    var updatedItem = InventoryItem.CreateCopy(originalItem.Value);
                    updatedItem.InventoryCount = updatedItem.InventoryCount + quantity;
                    await inventoryDictionary.TryUpdateAsync(tx, itemId, updatedItem, originalItem.Value);

                    await tx.CommitAsync();
                }
            }
        }
    }
}