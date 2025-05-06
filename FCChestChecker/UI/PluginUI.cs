using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCChestChecker.UI
{
    internal class PluginUI : Window, IDisposable
    {
        private bool visible = false;

        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        public PluginUI() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}###FCChestChecker")
        {
            this.RespectCloseHotkey = false;
            this.SizeConstraints = new()
            {
                MinimumSize = new(400, 200),
            };
            P.ws.AddWindow(this);
        }

        public void Dispose() { }

        public override void PreDraw() { }

        public override void Draw()
        {
            ImGui.TextWrapped($"Setup your API to recieve the data.");

            string API_Url = P.Config.APIUrl;
            string API_Key = P.Config.APIKey;

            ImGui.Separator();

            ImGui.Text("API Url");
            if (ImGui.InputText("###FCCcApiUrl", ref API_Url, 150))
            {
                P.Config.APIUrl = API_Url;
                P.Config.Save();
            }

            ImGui.Text("API Key");
            if (ImGui.InputText("###FCCcApiKey", ref API_Key, 150))
            {
                P.Config.APIKey = API_Key;
                P.Config.Save();
            }
        }
    }
}
