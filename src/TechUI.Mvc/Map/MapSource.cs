namespace TechUI.Mvc.Map
{
    /// <summary>
    /// A ready-made tile source (URL, attribution, subdomains and zoom limit).
    /// </summary>
    public sealed class MapSource
    {
        internal MapSource(
            string key,
            string name,
            string url,
            string attribution,
            int maxZoom,
            string[]? subdomains = null)
        {
            Key = key;
            Name = name;
            Url = url;
            Attribution = attribution;
            MaxZoom = maxZoom;
            Subdomains = subdomains ?? Array.Empty<string>();
        }

        /// <summary>Short lookup key, for example <c>osm</c> or <c>satellite</c>.</summary>
        public string Key { get; }

        /// <summary>Display name used by the layer control.</summary>
        public string Name { get; }

        public string Url { get; }

        public string Attribution { get; }

        public int MaxZoom { get; }

        public IReadOnlyList<string> Subdomains { get; }
    }

    /// <summary>
    /// Built-in tile providers for <see cref="MapBuilder.Source(MapSource, string?)"/>
    /// and <see cref="MapBuilder.Layer(MapSource, string?, bool)"/>.
    /// Check each provider's usage policy before production traffic.
    /// </summary>
    public static class MapSources
    {
        public static readonly MapSource OpenStreetMap = new(
            "osm",
            "نقشه استاندارد",
            "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
            "&copy; <a href=\"https://www.openstreetmap.org/copyright\">" +
                "OpenStreetMap</a> contributors",
            19);

        public static readonly MapSource CartoLight = new(
            "light",
            "نقشه روشن",
            "https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png",
            "&copy; OpenStreetMap contributors &copy; " +
                "<a href=\"https://carto.com/attributions\">CARTO</a>",
            20,
            new[] { "a", "b", "c", "d" });

        public static readonly MapSource CartoDark = new(
            "dark",
            "نقشه تاریک",
            "https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png",
            "&copy; OpenStreetMap contributors &copy; " +
                "<a href=\"https://carto.com/attributions\">CARTO</a>",
            20,
            new[] { "a", "b", "c", "d" });

        public static readonly MapSource OpenTopoMap = new(
            "topo",
            "توپوگرافی",
            "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png",
            "Map data &copy; OpenStreetMap contributors, SRTM | " +
                "Map style &copy; <a href=\"https://opentopomap.org\">" +
                "OpenTopoMap</a> (CC-BY-SA)",
            17,
            new[] { "a", "b", "c" });

        public static readonly MapSource EsriSatellite = new(
            "satellite",
            "تصاویر ماهواره‌ای",
            "https://server.arcgisonline.com/ArcGIS/rest/services/" +
                "World_Imagery/MapServer/tile/{z}/{y}/{x}",
            "Tiles &copy; Esri &mdash; Source: Esri, Maxar, " +
                "Earthstar Geographics",
            19);

        public static IReadOnlyList<MapSource> All { get; } = new[]
        {
            OpenStreetMap,
            CartoLight,
            CartoDark,
            OpenTopoMap,
            EsriSatellite
        };

        /// <summary>
        /// Resolves a source by key, display name or tile URL template.
        /// </summary>
        public static MapSource? Find(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var text = value.Trim();

            return All.FirstOrDefault(source =>
                source.Key.Equals(text, StringComparison.OrdinalIgnoreCase) ||
                source.Name.Equals(text, StringComparison.OrdinalIgnoreCase) ||
                source.Url.Equals(text, StringComparison.OrdinalIgnoreCase));
        }
    }
}
