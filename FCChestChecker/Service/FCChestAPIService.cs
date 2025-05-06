using ECommons.DalamudServices;
using FCChestChecker.Models;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FCChestChecker.Service
{
    internal static class FCChestAPIService
    {
        internal static async void UploadFCChestEvents(string fcTag, string characterName, string server, FCChestInventory inventory)
        {
            if (String.IsNullOrWhiteSpace(P.Config.APIKey)) return;

            Svc.Log.Debug($"[{characterName}@{server} <<{fcTag}>>] Uploading inventory {inventory.inventoryType} ({inventory.items.Where(x => x.ItemId != 0).Count()} non-empty slots).");

            using var httpClient = new HttpClient() { BaseAddress = new Uri(P.Config.APIUrl) };
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var data = JsonSerializer.Serialize(inventory);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", P.Config.APIKey);

            //Svc.Log.Debug($"Sending POST Data to: {httpClient.BaseAddress}FCChest");
            //Svc.Log.Debug(data);

            try
            {
                var result = await httpClient.PostAsync($"FCChest/{server}/{fcTag}", content);
            }
            catch (Exception ex) 
            { 
                Svc.Log.Error(ex.Message);
                if (ex.InnerException != null) 
                    Svc.Log.Error(ex.InnerException.Message);

                Svc.Chat.Print("[FCChestChecker] An error has occurred with communication with the server.");
            }
        }

        internal static async Task<List<FCChestInventory>?> GetFCChestInventories(string fcTag, string server)
        {
            if (String.IsNullOrWhiteSpace(P.Config.APIKey)) return null;

            Svc.Log.Debug($"[<<{fcTag}>>@{server}] Fetching FC inventories.");

            using var httpClient = new HttpClient() { BaseAddress = new Uri(P.Config.APIUrl) };
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", P.Config.APIKey);

            try
            {
                var result = await httpClient.GetAsync($"FCChest/{server}/{fcTag}");
                var resultString = await result.Content.ReadAsStringAsync();
                Svc.Log.Debug($"{resultString}");
                return JsonSerializer.Deserialize<List<FCChestInventory>>(resultString);
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex.Message);
                Svc.Chat.Print("[FCChestChecker] An error has occurred with communication with the server.");

                return null;
            }
        }
    }
}
