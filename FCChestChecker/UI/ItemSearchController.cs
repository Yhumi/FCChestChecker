using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Inventory;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FCChestChecker.Models;
using FCChestChecker.Service;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Lumina.Data.Parsing.Uld.NodeData;

namespace FCChestChecker.UI
{
    internal class ItemSearchController : Window, IDisposable
    {
        private nint addon = nint.Zero;
        public List<FCChestInventory> Inventories = new List<FCChestInventory>();

        private uint ItemId = 0;

        public ItemSearchController() : base("FCChestItemSearch", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoNavInputs | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoFocusOnAppearing, true)
        {
            P.ws.AddWindow(this);

            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ItemFinder", OnAddonSetup);
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "ItemFinder", OnAddonPreFinalise);
            Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "ItemFinder", OnAddonRefresh);
        }

        public void Dispose()
        {
            Svc.AddonLifecycle.UnregisterListener(OnAddonSetup);
        }

        private unsafe void OnAddonSetup(AddonEvent type, AddonArgs args)
        {
            IsOpen = true;
            addon = args.Addon;
            
            ItemId = LuminaSheets.GetItemIdByItemName(new ReadOnlySeStringSpan(((AtkUnitBase*)addon)->AtkValues[0].String.Value).ExtractText());
            Svc.Log.Info(ItemId.ToString());

            Svc.Framework.RunOnFrameworkThread(() => FetchFCInventories());
        }

        private void OnAddonPreFinalise(AddonEvent type, AddonArgs args)
        {
            IsOpen = false;
            ItemId = 0;
            addon = nint.Zero;
            Inventories.Clear();
        }

        private unsafe void OnAddonRefresh(AddonEvent type, AddonArgs args)
        {
            if (addon == nint.Zero) return;

            ItemId = LuminaSheets.GetItemIdByItemName(new ReadOnlySeStringSpan(((AtkUnitBase*)addon)->AtkValues[0].String.Value).ExtractText());
            Svc.Log.Debug($"Refreshed ItemFinder, new Item: {ItemId}");

            Svc.Framework.RunOnFrameworkThread(() => FetchFCInventories());
        }

        private async void FetchFCInventories()
        {
            var result = await FCChestAPIService.GetFCChestInventories(Svc.ClientState.LocalPlayer?.CompanyTag.GetText() ?? "", Svc.ClientState.LocalPlayer?.HomeWorld.Value.Name.GetText() ?? "");
            if (result == null) return;

            Inventories = result;
        }

        public override unsafe void Draw()
        {
            if (addon == nint.Zero || ItemId == 0) return;

            var addonPtr = (AtkUnitBase*)addon;
            if (addonPtr == null)
                return;

            var componentNode = addonPtr->UldManager.SearchNodeById(1);
            if (!componentNode->IsVisible())
                return;

            DrawWindow(addonPtr);
        }

        private unsafe void DrawWindow(AtkUnitBase* addonPtr)
        {
            var windowFlags = SetupWindowPosition(addonPtr);

            ImGui.Begin($"###FCChestItemSearch", windowFlags);

            ImGui.Text($"Search results for {new ReadOnlySeStringSpan(addonPtr->AtkValues[0].String.Value).ExtractText()} in your FC Chest.");
            ImGui.Separator();

            foreach (var inv in Inventories)
            {
                if (inv.items.Any(x => x.ItemId == ItemId))
                {
                    var NQSum = inv.items.Where(x => x.ItemId == ItemId && !x.IsHQ).Sum(x => x.Count);
                    var HQSum = inv.items.Where(x => x.ItemId == ItemId && x.IsHQ).Sum(x => x.Count);

                    ImGui.Text($"{InventoryNameCleanup(inv.inventoryType)} | {NQSum} NQ/{HQSum} HQ");
                }
            }

            ImGui.End();
        }

        private string InventoryNameCleanup(GameInventoryType inventoryType)
        {
            switch (inventoryType)
            {
                case GameInventoryType.FreeCompanyPage1:
                    return "FC Chest (Page 1)";
                case GameInventoryType.FreeCompanyPage2:
                    return "FC Chest (Page 2)";
                case GameInventoryType.FreeCompanyPage3:
                    return "FC Chest (Page 3)";
                case GameInventoryType.FreeCompanyPage4:
                    return "FC Chest (Page 4)";
                case GameInventoryType.FreeCompanyPage5:
                    return "FC Chest (Page 5)";
                default:
                    return inventoryType.ToString();
            }
        }

        private unsafe ImGuiWindowFlags SetupWindowPosition(AtkUnitBase* addonPtr)
        {
            var baseX = addonPtr->X;
            var baseY = addonPtr->Y;

            var componentNode = addonPtr->UldManager.SearchNodeById(1);

            var position = AtkResNodeFunctions.GetNodePosition(componentNode);
            var scale = AtkResNodeFunctions.GetNodeScale(componentNode);
            var size = new Vector2(componentNode->Width, componentNode->Height) * scale;

            var center = new Vector2((position.X + size.X) / 2, (position.Y - size.Y) / 2);

            ImGuiHelpers.ForceNextWindowMainViewport();

            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(position.X + size.X + 7, position.Y), ImGuiCond.Always);
            AtkResNodeFunctions.ResetPosition = false;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(7f, 7f));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(0f, 0f));

            return ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove;
        }
    }
}
