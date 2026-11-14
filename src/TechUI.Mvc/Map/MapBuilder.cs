using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TechUI.Mvc.Map
{
    public static class MapTones
    {
        public const string Primary = "primary";
        public const string Success = "success";
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Danger = "danger";
        public const string Neutral = "neutral";

        internal static readonly HashSet<string> All =
            new(StringComparer.OrdinalIgnoreCase)
            {
                Primary, Success, Info, Warning, Danger, Neutral
            };
    }

    /// <summary>
    /// Dependency-free slippy map (OpenStreetMap tiles) with markers,
    /// popups, vector shapes, layers, search and location controls.
    /// </summary>
    public sealed class MapBuilder : IHtmlContent
    {
        private const string DefaultTileUrl =
            "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

        private const string DefaultAttribution =
            "&copy; <a href=\"https://www.openstreetmap.org/copyright\">" +
            "OpenStreetMap</a> contributors";

        private readonly string _id;
        private readonly List<MapLayer> _layers = new();
        private readonly List<MapMarker> _markers = new();
        private readonly List<MapShape> _polylines = new();
        private readonly List<MapShape> _polygons = new();
        private readonly List<MapCircle> _circles = new();
        private readonly List<string> _subdomains = new();
        private readonly Dictionary<string, string> _events =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private double _lat = 35.6892;
        private double _lng = 51.3890;
        private int _zoom = 12;
        private int _minZoom = 2;
        private int _maxZoom = 19;
        private int _height = 420;
        private string _tileUrl = DefaultTileUrl;
        private string _attribution = DefaultAttribution;
        private string? _ariaLabel;
        private string? _searchUrl;
        private string? _searchPlaceholder;
        private bool _showZoom = true;
        private bool _showScale = true;
        private bool _showFullscreen = true;
        private bool _showLocate = true;
        private bool _showCoordinates;
        private bool _showLayerControl = true;
        private bool _showSearch = true;
        private bool _scrollWheelZoom = true;
        private bool _doubleClickZoom = true;
        private bool _dragging = true;
        private bool _keyboard = true;

        public MapBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-map-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string MapId => _id;

        public MapBuilder Center(double lat, double lng)
        {
            _lat = Math.Clamp(lat, -85.05112878, 85.05112878);
            _lng = NormalizeLng(lng);
            return this;
        }

        public MapBuilder Zoom(int zoom)
        {
            _zoom = Math.Clamp(zoom, 1, 22);
            return this;
        }

        public MapBuilder MinZoom(int zoom)
        {
            _minZoom = Math.Clamp(zoom, 1, 22);
            return this;
        }

        public MapBuilder MaxZoom(int zoom)
        {
            _maxZoom = Math.Clamp(zoom, 1, 22);
            return this;
        }

        public MapBuilder Height(int pixels)
        {
            _height = Math.Clamp(pixels, 160, 2000);
            return this;
        }

        public MapBuilder TileUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _tileUrl = url.Trim();
            }

            return this;
        }

        public MapBuilder Attribution(string html)
        {
            _attribution = html ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Uses a built-in tile provider. Its maximum zoom acts as a ceiling;
        /// call <see cref="MaxZoom(int)"/> afterwards to allow more.
        /// </summary>
        public MapBuilder Source(MapSource source, string? attribution = null)
        {
            if (source is null)
            {
                return this;
            }

            _tileUrl = source.Url;
            _attribution = attribution ?? source.Attribution;
            _maxZoom = Math.Min(_maxZoom, source.MaxZoom);
            MergeSubdomains(source.Subdomains);

            return this;
        }

        /// <summary>
        /// Uses a built-in provider by key/name, or any tile URL template
        /// containing <c>{z}</c>.
        /// </summary>
        public MapBuilder Source(string source, string? attribution = null)
        {
            var preset = MapSources.Find(source);

            if (preset is not null)
            {
                return Source(preset, attribution);
            }

            if (!string.IsNullOrWhiteSpace(source) &&
                source.Contains("{z}", StringComparison.Ordinal))
            {
                _tileUrl = source.Trim();

                if (!string.IsNullOrWhiteSpace(attribution))
                {
                    _attribution = attribution;
                }
            }

            return this;
        }

        public MapBuilder Subdomains(params string[] subdomains)
        {
            _subdomains.Clear();

            if (subdomains is not null)
            {
                _subdomains.AddRange(subdomains
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item.Trim()));
            }

            return this;
        }

        /// <summary>
        /// Adds a selectable base layer. The first layer is active by default.
        /// </summary>
        public MapBuilder Layer(
            string name,
            string url,
            string? attribution = null,
            bool selected = false,
            int? maxZoom = null)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(url))
            {
                return this;
            }

            _layers.Add(new MapLayer
            {
                Name = name.Trim(),
                Url = url.Trim(),
                Attribution = attribution,
                Selected = selected || _layers.Count == 0,
                MaxZoom = maxZoom.HasValue
                    ? Math.Clamp(maxZoom.Value, 1, 22)
                    : null
            });

            return this;
        }

        /// <summary>
        /// Adds a built-in tile provider as a base layer.
        /// </summary>
        public MapBuilder Layer(
            MapSource source,
            string? name = null,
            bool selected = false)
        {
            if (source is null)
            {
                return this;
            }

            _layers.Add(new MapLayer
            {
                Name = string.IsNullOrWhiteSpace(name)
                    ? source.Name
                    : name.Trim(),
                Url = source.Url,
                Attribution = source.Attribution,
                Selected = selected || _layers.Count == 0,
                MaxZoom = source.MaxZoom,
                Subdomains = source.Subdomains
            });

            return this;
        }

        public MapBuilder Marker(
            double lat,
            double lng,
            Action<MapMarkerBuilder>? configure = null)
        {
            var builder = new MapMarkerBuilder(lat, lng);

            configure?.Invoke(builder);

            _markers.Add(builder.Build());
            return this;
        }

        public MapBuilder Marker(
            double lat,
            double lng,
            string? title = null,
            string? popup = null,
            string? icon = null,
            string? tone = null)
        {
            var builder = new MapMarkerBuilder(lat, lng)
                .Title(title)
                .Popup(popup)
                .Icon(icon)
                .Tone(tone);

            _markers.Add(builder.Build());
            return this;
        }

        public MapBuilder Polyline(
            IEnumerable<(double Lat, double Lng)> points,
            string? color = null,
            int weight = 3,
            double opacity = 0.9)
        {
            AddShape(_polylines, points, color, weight, opacity, null);
            return this;
        }

        public MapBuilder Polygon(
            IEnumerable<(double Lat, double Lng)> points,
            string? color = null,
            int weight = 2,
            double opacity = 0.9,
            double fillOpacity = 0.18)
        {
            AddShape(_polygons, points, color, weight, opacity, fillOpacity);
            return this;
        }

        public MapBuilder Circle(
            double lat,
            double lng,
            double radiusMeters,
            string? color = null,
            double fillOpacity = 0.15)
        {
            if (radiusMeters <= 0)
            {
                return this;
            }

            _circles.Add(new MapCircle
            {
                Lat = Math.Clamp(lat, -85.05112878, 85.05112878),
                Lng = NormalizeLng(lng),
                Radius = radiusMeters,
                Color = Text(color),
                FillOpacity = Math.Clamp(fillOpacity, 0, 1)
            });

            return this;
        }

        public MapBuilder Search(
            string url = "https://nominatim.openstreetmap.org/search",
            string? placeholder = null)
        {
            _searchUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
            _searchPlaceholder = Text(placeholder);
            return this;
        }

        public MapBuilder ZoomControl(bool show = true)
        {
            _showZoom = show;
            return this;
        }

        public MapBuilder ScaleControl(bool show = true)
        {
            _showScale = show;
            return this;
        }

        public MapBuilder FullscreenControl(bool show = true)
        {
            _showFullscreen = show;
            return this;
        }

        public MapBuilder LocateControl(bool show = true)
        {
            _showLocate = show;
            return this;
        }

        public MapBuilder CoordinatesControl(bool show = true)
        {
            _showCoordinates = show;
            return this;
        }

        public MapBuilder LayerControl(bool show = true)
        {
            _showLayerControl = show;
            return this;
        }

        public MapBuilder SearchControl(bool show = true)
        {
            _showSearch = show;
            return this;
        }

        public MapBuilder ScrollWheelZoom(bool enabled = true)
        {
            _scrollWheelZoom = enabled;
            return this;
        }

        public MapBuilder DoubleClickZoom(bool enabled = true)
        {
            _doubleClickZoom = enabled;
            return this;
        }

        public MapBuilder Dragging(bool enabled = true)
        {
            _dragging = enabled;
            return this;
        }

        public MapBuilder Keyboard(bool enabled = true)
        {
            _keyboard = enabled;
            return this;
        }

        public MapBuilder Events(Action<MapEventBuilder> configure)
        {
            if (configure is null)
            {
                return this;
            }

            var builder = new MapEventBuilder(_events);

            configure(builder);

            return this;
        }

        public MapBuilder AriaLabel(string label)
        {
            _ariaLabel = Text(label);
            return this;
        }

        public MapBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public MapBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public MapBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-map");
            root.Attributes["data-tech-component"] = "map";
            root.Attributes["style"] =
                FormattableString.Invariant($"height:{_height}px");
            root.Attributes["aria-label"] =
                _ariaLabel ?? "نقشه تعاملی";
            root.Attributes["data-map-options"] =
                JsonSerializer.Serialize(BuildOptions());

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    root.AddCssClass(attribute.Value);
                }
                else if (!root.Attributes.ContainsKey(attribute.Key))
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            root.WriteTo(writer, encoder);
        }

        private Dictionary<string, object?> BuildOptions()
        {
            var activeLayer = _layers.FirstOrDefault(layer => layer.Selected)
                ?? _layers.FirstOrDefault();

            var options = new Dictionary<string, object?>
            {
                ["center"] = new Dictionary<string, object?>
                {
                    ["lat"] = _lat,
                    ["lng"] = _lng
                },
                ["zoom"] = Math.Clamp(_zoom, _minZoom, _maxZoom),
                ["minZoom"] = _minZoom,
                ["maxZoom"] = _maxZoom,
                ["tileUrl"] = activeLayer?.Url ?? _tileUrl,
                ["attribution"] = activeLayer?.Attribution ?? _attribution,
                ["subdomains"] = _subdomains,
                ["scrollWheelZoom"] = _scrollWheelZoom,
                ["doubleClickZoom"] = _doubleClickZoom,
                ["dragging"] = _dragging,
                ["keyboard"] = _keyboard,
                ["controls"] = new Dictionary<string, object?>
                {
                    ["zoom"] = _showZoom,
                    ["scale"] = _showScale,
                    ["fullscreen"] = _showFullscreen,
                    ["locate"] = _showLocate,
                    ["coordinates"] = _showCoordinates,
                    ["layers"] = _showLayerControl && _layers.Count > 1,
                    ["search"] = _showSearch &&
                        !string.IsNullOrWhiteSpace(_searchUrl)
                },
                ["searchUrl"] = _searchUrl,
                ["searchPlaceholder"] = _searchPlaceholder,
                ["layers"] = _layers.Select(layer => new Dictionary<string, object?>
                {
                    ["name"] = layer.Name,
                    ["url"] = layer.Url,
                    ["attribution"] = layer.Attribution,
                    ["selected"] = layer.Selected,
                    ["maxZoom"] = layer.MaxZoom,
                    ["subdomains"] = layer.Subdomains
                }).ToArray(),
                ["markers"] = _markers.Select(marker => new Dictionary<string, object?>
                {
                    ["id"] = marker.Id,
                    ["lat"] = marker.Lat,
                    ["lng"] = marker.Lng,
                    ["title"] = marker.Title,
                    ["popup"] = marker.Popup,
                    ["icon"] = marker.Icon,
                    ["tone"] = marker.Tone,
                    ["draggable"] = marker.Draggable,
                    ["open"] = marker.Open
                }).ToArray(),
                ["polylines"] = _polylines.Select(ShapeOptions).ToArray(),
                ["polygons"] = _polygons.Select(ShapeOptions).ToArray(),
                ["circles"] = _circles.Select(circle => new Dictionary<string, object?>
                {
                    ["lat"] = circle.Lat,
                    ["lng"] = circle.Lng,
                    ["radius"] = circle.Radius,
                    ["color"] = circle.Color,
                    ["fillOpacity"] = circle.FillOpacity
                }).ToArray(),
                ["events"] = new Dictionary<string, string>(_events)
            };

            return options;
        }

        private void MergeSubdomains(IReadOnlyList<string> subdomains)
        {
            foreach (var subdomain in subdomains)
            {
                if (!_subdomains.Contains(subdomain,
                    StringComparer.OrdinalIgnoreCase))
                {
                    _subdomains.Add(subdomain);
                }
            }
        }

        private static Dictionary<string, object?> ShapeOptions(MapShape shape)
        {
            return new Dictionary<string, object?>
            {
                ["points"] = shape.Points
                    .Select(point => new[] { point.Lat, point.Lng })
                    .ToArray(),
                ["color"] = shape.Color,
                ["weight"] = shape.Weight,
                ["opacity"] = shape.Opacity,
                ["fillOpacity"] = shape.FillOpacity
            };
        }

        private void AddShape(
            List<MapShape> target,
            IEnumerable<(double Lat, double Lng)> points,
            string? color,
            int weight,
            double opacity,
            double? fillOpacity)
        {
            if (points is null)
            {
                return;
            }

            var normalized = points
                .Where(point => !double.IsNaN(point.Lat) &&
                    !double.IsNaN(point.Lng))
                .Select(point => new MapPoint
                {
                    Lat = Math.Clamp(point.Lat, -85.05112878, 85.05112878),
                    Lng = NormalizeLng(point.Lng)
                })
                .ToList();

            if (normalized.Count < 2)
            {
                return;
            }

            target.Add(new MapShape
            {
                Points = normalized,
                Color = Text(color),
                Weight = Math.Clamp(weight, 1, 20),
                Opacity = Math.Clamp(opacity, 0, 1),
                FillOpacity = fillOpacity.HasValue
                    ? Math.Clamp(fillOpacity.Value, 0, 1)
                    : null
            });
        }

        private static double NormalizeLng(double lng)
        {
            if (double.IsNaN(lng) || double.IsInfinity(lng))
            {
                return 0;
            }

            var normalized = (lng + 180) % 360;

            if (normalized < 0)
            {
                normalized += 360;
            }

            return normalized - 180;
        }

        private static string? Text(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class MapLayer
        {
            public string Name { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string? Attribution { get; set; }
            public bool Selected { get; set; }
            public int? MaxZoom { get; set; }
            public IReadOnlyList<string> Subdomains { get; set; } =
                Array.Empty<string>();
        }

        private sealed class MapPoint
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        private sealed class MapShape
        {
            public List<MapPoint> Points { get; set; } = new();
            public string? Color { get; set; }
            public int Weight { get; set; }
            public double Opacity { get; set; }
            public double? FillOpacity { get; set; }
        }

        private sealed class MapCircle
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public double Radius { get; set; }
            public string? Color { get; set; }
            public double FillOpacity { get; set; }
        }

        internal sealed class MapMarker
        {
            public string Id { get; set; } = string.Empty;
            public double Lat { get; set; }
            public double Lng { get; set; }
            public string? Title { get; set; }
            public string? Popup { get; set; }
            public string Icon { get; set; } = "geo-alt-fill";
            public string Tone { get; set; } = MapTones.Primary;
            public bool Draggable { get; set; }
            public bool Open { get; set; }
        }
    }

    /// <summary>
    /// Fluent options for a single map marker.
    /// </summary>
    public sealed class MapMarkerBuilder
    {
        private readonly double _lat;
        private readonly double _lng;
        private string? _id;
        private string? _title;
        private string? _popup;
        private string _icon = "geo-alt-fill";
        private string _tone = MapTones.Primary;
        private bool _draggable;
        private bool _open;

        internal MapMarkerBuilder(double lat, double lng)
        {
            _lat = Math.Clamp(lat, -85.05112878, 85.05112878);
            _lng = lng;
        }

        public MapMarkerBuilder Id(string id)
        {
            _id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
            return this;
        }

        public MapMarkerBuilder Title(string? title)
        {
            _title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
            return this;
        }

        public MapMarkerBuilder Popup(string? html)
        {
            _popup = string.IsNullOrWhiteSpace(html) ? null : html;
            return this;
        }

        public MapMarkerBuilder Icon(string? icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                return this;
            }

            var value = icon.Trim();

            _icon = value.StartsWith("bi-", StringComparison.OrdinalIgnoreCase)
                ? value[3..]
                : value;

            return this;
        }

        public MapMarkerBuilder Tone(string? tone)
        {
            if (!string.IsNullOrWhiteSpace(tone) &&
                MapTones.All.Contains(tone.Trim()))
            {
                _tone = tone.Trim().ToLowerInvariant();
            }

            return this;
        }

        public MapMarkerBuilder Draggable(bool draggable = true)
        {
            _draggable = draggable;
            return this;
        }

        public MapMarkerBuilder Open(bool open = true)
        {
            _open = open;
            return this;
        }

        internal MapBuilder.MapMarker Build()
        {
            return new MapBuilder.MapMarker
            {
                Id = _id ?? "marker-" + Guid.NewGuid().ToString("N")[..8],
                Lat = _lat,
                Lng = _lng,
                Title = _title,
                Popup = _popup,
                Icon = _icon,
                Tone = _tone,
                Draggable = _draggable,
                Open = _open
            };
        }
    }

    /// <summary>
    /// Client event handler names for a map.
    /// </summary>
    public sealed class MapEventBuilder
    {
        private readonly Dictionary<string, string> _events;

        internal MapEventBuilder(Dictionary<string, string> events)
        {
            _events = events;
        }

        public MapEventBuilder Click(string handler)
        {
            return Set("click", handler);
        }

        public MapEventBuilder MoveEnd(string handler)
        {
            return Set("moveEnd", handler);
        }

        public MapEventBuilder ZoomEnd(string handler)
        {
            return Set("zoomEnd", handler);
        }

        public MapEventBuilder MarkerClick(string handler)
        {
            return Set("markerClick", handler);
        }

        public MapEventBuilder MarkerDragEnd(string handler)
        {
            return Set("markerDragEnd", handler);
        }

        public MapEventBuilder Locate(string handler)
        {
            return Set("locate", handler);
        }

        public MapEventBuilder Search(string handler)
        {
            return Set("search", handler);
        }

        private MapEventBuilder Set(string name, string? handler)
        {
            if (!string.IsNullOrWhiteSpace(handler))
            {
                _events[name] = handler.Trim();
            }

            return this;
        }
    }
}
