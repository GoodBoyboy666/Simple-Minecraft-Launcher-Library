using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Minecraft_Launcher_Library
{
    public class Asset()
    {
        public static List<AssetItem> GetAssetItem(string asset_json)
        {
            JObject? asset = Json.DeserializeObject<JObject>(asset_json);
            List<AssetItem> assets = new List<AssetItem>();
            foreach (var item in asset?["objects"] ?? new JArray())
            {
                AssetItem assetitem = new AssetItem();
                var property = item as JProperty;
                JObject? obj = Json.DeserializeObject<JObject>(property?.Value?.ToString() ?? "");
                assetitem.hash = obj?["hash"]?.ToString();
                assetitem.size = obj?["size"]?.ToString();
                assets.Add(assetitem);
            }
            return assets;
        }
    }

    public class AssetItem
    {
        public string? hash { get; set; }
        public string? size { get; set; }
    }
}
