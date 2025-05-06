using System.Collections.Generic;
using System.Linq;
using ECommons.DalamudServices;
using Lumina.Excel.Sheets;

namespace FCChestChecker.Service
{
    internal class LuminaSheets
    {
        public static Dictionary<uint, Item>? ItemSheet;

        public static void Init()
        {
            ItemSheet = Svc.Data?.GetExcelSheet<Item>()?
                       .ToDictionary(i => i.RowId, i => i);
        }

        public static uint GetItemIdByItemName(string itemName)
        {
            var item = ItemSheet?.FirstOrDefault(x => x.Value.Name.ExtractText().ToLower() == itemName.ToLower());
            return item.HasValue ? item.Value.Key : 0;
        }
    }
}
