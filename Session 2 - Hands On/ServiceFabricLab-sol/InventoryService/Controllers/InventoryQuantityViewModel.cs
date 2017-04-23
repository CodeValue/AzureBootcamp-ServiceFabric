using System;
using InventoryCommon;

namespace InventoryService.Controllers
{
    public class InventoryQuantityViewModel
    {
        public Guid ItemId { get; set; }
        public InventoryItemType ItemType { get; set; }
        public String Display { get; set; }
        public Boolean IsAdd { get; set; }
        public Int32 Quantity { get; set; }
    }
}