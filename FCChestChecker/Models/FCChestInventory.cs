using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCChestChecker.Models
{
    public class FCChestInventory
    {
        public GameInventoryType inventoryType { get; set; }
        public List<FCChestInventoryItem> items { get; set; }

        public FCChestInventory() { }

        public FCChestInventory(GameInventoryType invType) 
        {
            inventoryType = invType;
            items = [];
        }

        public void AddItemToInventory(InventoryItem item)
        {
            items ??= [];
            items.Add(new(item));
        }
    }

    public class FCChestInventoryItem
    {
        public short Slot { get; set; }
        public uint ItemId { get; set; }
        public int Count { get; set; }
        public bool IsHQ { get; set; }

        public FCChestInventoryItem() { }

        public FCChestInventoryItem(InventoryItem item)
        {
            Slot = item.Slot;
            ItemId = item.ItemId;
            Count = item.Quantity;
            IsHQ = item.IsHighQuality();
        }
    }
}
