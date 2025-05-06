using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using FCChestChecker.Service;
using System.Collections.Generic;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using System.Linq;
using Dalamud.Game.Inventory;
using FCChestChecker.Models;
using FCChestChecker.UI;
using Dalamud.Interface.Style;
using System.Threading.Tasks;
using System.Net;
namespace FCChestChecker;

public sealed class FCChestChecker : IDalamudPlugin
{
    public string Name => "FCChestChecker";
    private const string FCSetupCommand = "/fccsetup";
    private const string FCTestCommand = "/fcctest";

    internal static FCChestChecker P = null!;
    internal PluginUI PluginUi;
    internal WindowSystem ws;
    internal Configuration Config;
    //internal NativeController NativeController;

    internal ItemSearchController ItemSearchController;

    internal StyleModel Style;

    public FCChestChecker(IDalamudPluginInterface pluginInterface)
    {
        ECommonsMain.Init(pluginInterface, this, Module.All);

        P = this;
        P.Config = Configuration.Load();

        LuminaSheets.Init();

        ws = new();
        Config = P.Config;
        ItemSearchController = new();
        PluginUi = new();

        Svc.Commands.AddHandler(FCSetupCommand, new CommandInfo(DrawSettingsUICmd)
        {
            HelpMessage = "Opens the FCChestChecker settings.\n",
            ShowInHelp = true,
        });

        //Svc.Commands.AddHandler(FCTestCommand, new CommandInfo(TestCommand)
        //{
        //    HelpMessage = "Test command.\n"
        //});

        Svc.GameInventory.InventoryChangedRaw += InventoryChanged;
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawSettingsUI;
        //Svc.PluginInterface.UiBuilder.OpenMainUi += DrawSettingsUI;

        Style = StyleModel.GetFromCurrent()!;
    }

    private void InventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        if (events.Any(x => 
            x.Item.ContainerType == GameInventoryType.FreeCompanyPage1 ||
            x.Item.ContainerType == GameInventoryType.FreeCompanyPage2 ||
            x.Item.ContainerType == GameInventoryType.FreeCompanyPage3 ||
            x.Item.ContainerType == GameInventoryType.FreeCompanyPage4 ||
            x.Item.ContainerType == GameInventoryType.FreeCompanyPage5))
        {
            var fcName = Svc.ClientState.LocalPlayer?.CompanyTag.GetText() ?? "";
            var charaName = Svc.ClientState.LocalPlayer?.Name.GetText() ?? "";
            var homeWorld = Svc.ClientState.LocalPlayer?.HomeWorld.Value.Name.GetText() ?? "";

            var inventoriesUpdated = events.Where(x => 
                x.Item.ContainerType == GameInventoryType.FreeCompanyPage1 ||
                x.Item.ContainerType == GameInventoryType.FreeCompanyPage2 ||
                x.Item.ContainerType == GameInventoryType.FreeCompanyPage3 ||
                x.Item.ContainerType == GameInventoryType.FreeCompanyPage4 ||
                x.Item.ContainerType == GameInventoryType.FreeCompanyPage5)
                .Select(x => x.Item.ContainerType).Distinct();

            Svc.Log.Debug($"{inventoriesUpdated.Count()} inventories.");

            foreach (var e in inventoriesUpdated.ToList()) {
                var inv = InventoryService.GetInventoryState(e);
                if (inv == null) continue;

                Task.Run(() => FCChestAPIService.UploadFCChestEvents(fcName, charaName, homeWorld, inv));
            }
        }
    }

    public void Dispose()
    {
        PluginUi.Dispose();

        GenericHelpers.Safe(() => Svc.Commands.RemoveHandler(FCSetupCommand));

        Svc.PluginInterface.UiBuilder.OpenConfigUi -= DrawSettingsUI;
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
        //Svc.PluginInterface.UiBuilder.OpenMainUi -= DrawSettingsUI;

        //GenericHelpers.Safe(NativeController.Dispose);

        ws?.RemoveAllWindows();
        ws = null!;

        ECommonsMain.Dispose();
        P = null!;
    }

    private void DrawSettingsUICmd(string command, string args) => DrawSettingsUI();

    //private async void TestCommand(string command, string args)
    //{
    //    var result = await FCChestAPIService.GetFCChestInventories(Svc.ClientState.LocalPlayer?.CompanyTag.GetText() ?? "", Svc.ClientState.LocalPlayer?.HomeWorld.Value.Name.GetText() ?? "");
    //    if (result == null) return;

    //    Svc.Log.Debug($"Fetched: {result.Count} inventories.");
    //}

    private void DrawSettingsUI()
    {
        PluginUi.IsOpen = true;
    }
}
