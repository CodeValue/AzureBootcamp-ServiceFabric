using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryCommon
{
    public class InventoryItem
    {
        public static InventoryItem CreateCopy(InventoryItem originalItem)
        {
            return new InventoryItem
            {
                ItemId = originalItem.ItemId,
                ItemType = originalItem.ItemType,
                Name = originalItem.Name,
                InventoryCount = originalItem.InventoryCount
            };
        }

        public Guid ItemId { get; set; }

        public InventoryItemType ItemType { get; set; }

        public String Name { get; set; }

        public Int32 InventoryCount { get; set; }
    }

    public enum InventoryItemType
    {
        Appliances,
        Electrical,
        Flooring,
        HandTools,
        Hardware,
        HeatingCooling,
        LawnGarden,
        Paint,
        Plumbing,
        PowerTools
    }
}
