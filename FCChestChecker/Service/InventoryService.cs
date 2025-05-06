using Dalamud.Game.Inventory;
using FCChestChecker.Models;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCChestChecker.Service
{
    internal static unsafe class InventoryService
    {
        internal static unsafe FCChestInventory? GetInventoryState(GameInventoryType gameInventoryType)
        {
            uint invTypeVal = (uint)gameInventoryType;
            var inventoryPointer = InventoryManager.Instance()->GetInventoryContainer((InventoryType)invTypeVal);

            FCChestInventory fcChestInv = new(gameInventoryType);

            for (int i = 0; i < inventoryPointer->Size; i++)
            {
                var item = inventoryPointer->Items[i];
                fcChestInv.AddItemToInventory(item);
            }

            return fcChestInv;
        }
    }
}
