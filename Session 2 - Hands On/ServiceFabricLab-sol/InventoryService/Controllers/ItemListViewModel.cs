using System.Collections.Generic;
using InventoryCommon;

namespace InventoryService.Controllers
{
    public class ItemListViewModel
    {
        public IEnumerable<InventoryItem> InventoryItems { get; set; }
        public InventoryItemType? SelectedItemType { get; set; }
    }
}