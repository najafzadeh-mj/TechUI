using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechUI.Mvc.Chart
{
    /// <summary>
    /// Fluent builder for dependency-free SVG charts.
    /// </summary>
    public sealed class ChartBuilder : IHtmlContent
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly HashSet<string> KnownTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ChartTypes.Column, ChartTypes.Bar, ChartTypes.Line,
                ChartTypes.Spline, ChartTypes.Step, ChartTypes.Area,
                ChartTypes.AreaSpline, ChartTypes.Pie, ChartTypes.Donut,
                ChartTypes.Polar, ChartTypes.Scatter, ChartTypes.Bubble,
                ChartTypes.Radar, ChartTypes.Gauge, ChartTypes.RadialBar,
                ChartTypes.Heatmap, ChartTypes.Funnel, ChartTypes.Treemap,
                ChartTypes.Waterfall, ChartTypes.Candlestick,
                ChartTypes.Bullet, ChartTypes.Sparkline,
                ChartTypes.SparkBar
            };

        private readonly string _id;
        private readonly List<string> _categories = new();
        private readonly List<string> _colors = new();
        private readonly List<ChartSeriesBuilder> _series = new();
        private readonly ChartAxisBuilder _xAxis = new();
        private readonly ChartAxisBuilder _yAxis = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _type = ChartTypes.Column;
        private string? _title;
        private string? _subtitle;
        private int? _height;
        private bool _legend = true;
        private string _legendPosition = "bottom";
        private bool _tooltip = true;
        private string? _tooltipFormat;
        private bool _dataLabels;
        private bool? _stacked;
        private bool? _horizontal;
        private bool? _smooth;
        private bool? _area;
        private bool? _rtl;
        private double? _donutHole;
        private string? _centerText;
        private double? _gaugeMin;
        private double? _gaugeMax;
        private double? _gaugeTarget;
        private string? _valueFormat;
        private string? _locale;
        private string? _emptyText;
        private string? _ariaLabel;
        private bool _animation = true;
        private bool _waterfallTotal = true;

        public ChartBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-chart-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string ChartId => _id;

        public ChartBuilder Type(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return this;
            }

            var normalized = type.Trim().ToLowerInvariant();

            normalized = normalized switch
            {
                "area-spline" => ChartTypes.AreaSpline,
                "areaspline" => ChartTypes.AreaSpline,
                "horizontalbar" => ChartTypes.Bar,
                "horizontal-bar" => ChartTypes.Bar,
                "hbar" => ChartTypes.Bar,
                "radial" => ChartTypes.RadialBar,
                "radial-gauge" => ChartTypes.Gauge,
                "spark" => ChartTypes.Sparkline,
                _ => normalized
            };

            _type = KnownTypes.Contains(normalized)
                ? normalized
                : ChartTypes.Column;

            return this;
        }

        public ChartBuilder Title(string title)
        {
            _title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
            return this;
        }

        public ChartBuilder Subtitle(string subtitle)
        {
            _subtitle = string.IsNullOrWhiteSpace(subtitle)
                ? null
                : subtitle.Trim();
            return this;
        }

        public ChartBuilder Height(int pixels)
        {
            _height = Math.Clamp(pixels, 120, 1600);
            return this;
        }

        public ChartBuilder Categories(params string[] categories)
        {
            _categories.Clear();

            if (categories is not null)
            {
                _categories.AddRange(categories.Where(
                    category => category is not null));
            }

            return this;
        }

        public ChartBuilder Categories(IEnumerable<string> categories)
        {
            return Categories(categories?.ToArray() ?? Array.Empty<string>());
        }

        public ChartBuilder Series(Action<ChartSeriesBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            var series = new ChartSeriesBuilder();

            configure(series);

            _series.Add(series);

            return this;
        }

        public ChartBuilder Series(string name, params double?[] values)
        {
            return Series(series => series.Title(name).Values(values));
        }

        public ChartBuilder Series(
            string name,
            IEnumerable<double?> values)
        {
            return Series(series => series.Title(name).Values(values));
        }

        public ChartBuilder Series(
            string name,
            IEnumerable<double> values)
        {
            return Series(series => series.Title(name).Values(values));
        }

        public ChartBuilder Series(
            string type,
            string name,
            params double?[] values)
        {
            return Series(series => series
                .Title(name)
                .Type(type)
                .Values(values));
        }

        public ChartBuilder Colors(params string[] colors)
        {
            _colors.Clear();

            if (colors is not null)
            {
                _colors.AddRange(colors.Where(
                    color => !string.IsNullOrWhiteSpace(color)));
            }

            return this;
        }

        public ChartBuilder Legend(bool show = true, string? position = null)
        {
            _legend = show;

            if (!string.IsNullOrWhiteSpace(position))
            {
                var normalized = position.Trim().ToLowerInvariant();

                _legendPosition = normalized == "top" ? "top" : "bottom";
            }

            return this;
        }

        public ChartBuilder Tooltip(bool show = true, string? format = null)
        {
            _tooltip = show;
            _tooltipFormat = string.IsNullOrWhiteSpace(format)
                ? null
                : format.Trim().ToLowerInvariant();

            return this;
        }

        public ChartBuilder DataLabels(bool show = true)
        {
            _dataLabels = show;
            return this;
        }

        public ChartBuilder Stacked(bool stacked = true)
        {
            _stacked = stacked;
            return this;
        }

        public ChartBuilder Horizontal(bool horizontal = true)
        {
            _horizontal = horizontal;
            return this;
        }

        public ChartBuilder Smooth(bool smooth = true)
        {
            _smooth = smooth;
            return this;
        }

        public ChartBuilder Area(bool area = true)
        {
            _area = area;
            return this;
        }

        public ChartBuilder Rtl(bool rtl = true)
        {
            _rtl = rtl;
            return this;
        }

        public ChartBuilder Donut(double hole = 0.62, string? centerText = null)
        {
            _donutHole = Math.Clamp(hole, 0.1, 0.9);

            if (!string.IsNullOrWhiteSpace(centerText))
            {
                _centerText = centerText.Trim();
            }

            return this;
        }

        public ChartBuilder Gauge(
            double min = 0,
            double max = 100,
            double? target = null)
        {
            _gaugeMin = min;
            _gaugeMax = max > min ? max : min + 1;
            _gaugeTarget = target;
            return this;
        }

        public ChartBuilder GaugeTarget(double target)
        {
            _gaugeTarget = target;
            return this;
        }

        /// <summary>
        /// number, percent, currency or compact.
        /// </summary>
        public ChartBuilder ValueFormat(string format)
        {
            _valueFormat = string.IsNullOrWhiteSpace(format)
                ? null
                : format.Trim().ToLowerInvariant();
            return this;
        }

        public ChartBuilder Locale(string locale)
        {
            _locale = string.IsNullOrWhiteSpace(locale)
                ? null
                : locale.Trim();
            return this;
        }

        public ChartBuilder EmptyText(string text)
        {
            _emptyText = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            return this;
        }

        public ChartBuilder AriaLabel(string label)
        {
            _ariaLabel = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
            return this;
        }

        public ChartBuilder Animation(bool enabled)
        {
            _animation = enabled;
            return this;
        }

        public ChartBuilder WaterfallTotal(bool showTotal = true)
        {
            _waterfallTotal = showTotal;
            return this;
        }

        public ChartBuilder XAxis(Action<ChartAxisBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            configure(_xAxis);

            return this;
        }

        public ChartBuilder YAxis(Action<ChartAxisBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            configure(_yAxis);

            return this;
        }

        public ChartBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public ChartBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-chart");
            root.Attributes["data-tech-component"] = "chart";

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            var options = BuildOptions();

            root.Attributes["data-chart-options"] =
                JsonSerializer.Serialize(options, JsonOptions);

            if (_height.HasValue)
            {
                root.Attributes["style"] =
                    $"--tech-chart-height:{_height.Value}px";
            }

            var accessibleLabel = _ariaLabel ?? _title ?? "نمودار";

            root.Attributes["aria-label"] = accessibleLabel;

            foreach (var attribute in _attributes)
            {
                root.Attributes[attribute.Key] = attribute.Value;
            }

            if (_title is not null || _subtitle is not null)
            {
                var header = new TagBuilder("div");

                header.AddCssClass("tech-chart-header");

                if (_title is not null)
                {
                    var title = new TagBuilder("div");

                    title.AddCssClass("tech-chart-title");
                    title.InnerHtml.Append(_title);

                    header.InnerHtml.AppendHtml(title);
                }

                if (_subtitle is not null)
                {
                    var subtitle = new TagBuilder("div");

                    subtitle.AddCssClass("tech-chart-subtitle");
                    subtitle.InnerHtml.Append(_subtitle);

                    header.InnerHtml.AppendHtml(subtitle);
                }

                root.InnerHtml.AppendHtml(header);
            }

            var canvas = new TagBuilder("div");

            canvas.AddCssClass("tech-chart-canvas");
            canvas.Attributes["role"] = "img";
            canvas.Attributes["aria-label"] = accessibleLabel;

            root.InnerHtml.AppendHtml(canvas);

            root.WriteTo(writer, encoder);
        }

        private Dictionary<string, object?> BuildOptions()
        {
            var options = new Dictionary<string, object?>
            {
                ["type"] = _type,
                ["legend"] = new Dictionary<string, object?>
                {
                    ["show"] = _legend,
                    ["position"] = _legendPosition
                },
                ["tooltip"] = new Dictionary<string, object?>
                {
                    ["show"] = _tooltip,
                    ["format"] = _tooltipFormat
                },
                ["dataLabels"] = _dataLabels,
                ["xAxis"] = _xAxis.ToPayload(),
                ["yAxis"] = _yAxis.ToPayload(),
                ["series"] = _series
                    .Select(series => series.ToPayload())
                    .ToList()
            };

            if (_categories.Count > 0)
            {
                options["categories"] = _categories.ToArray();
            }

            if (_colors.Count > 0)
            {
                options["colors"] = _colors.ToArray();
            }

            if (_height.HasValue)
            {
                options["height"] = _height.Value;
            }

            if (_stacked.HasValue)
            {
                options["stacked"] = _stacked.Value;
            }

            if (_horizontal.HasValue)
            {
                options["horizontal"] = _horizontal.Value;
            }

            if (_smooth.HasValue)
            {
                options["smooth"] = _smooth.Value;
            }

            if (_area.HasValue)
            {
                options["area"] = _area.Value;
            }

            if (_rtl.HasValue)
            {
                options["rtl"] = _rtl.Value;
            }

            if (_donutHole.HasValue)
            {
                options["donut"] = new Dictionary<string, object?>
                {
                    ["hole"] = _donutHole.Value,
                    ["centerText"] = _centerText
                };
            }

            if (_gaugeMin.HasValue || _gaugeMax.HasValue)
            {
                options["gauge"] = new Dictionary<string, object?>
                {
                    ["min"] = _gaugeMin ?? 0,
                    ["max"] = _gaugeMax ?? 100,
                    ["target"] = _gaugeTarget
                };
            }

            if (_valueFormat is not null)
            {
                options["valueFormat"] = _valueFormat;
            }

            if (_locale is not null)
            {
                options["locale"] = _locale;
            }

            if (_emptyText is not null)
            {
                options["emptyText"] = _emptyText;
            }

            if (_ariaLabel is not null)
            {
                options["ariaLabel"] = _ariaLabel;
            }

            if (!_animation)
            {
                options["animation"] = false;
            }

            if (_type == ChartTypes.Waterfall && !_waterfallTotal)
            {
                options["waterfall"] = new Dictionary<string, object?>
                {
                    ["showTotal"] = false
                };
            }

            return options;
        }
    }
}
