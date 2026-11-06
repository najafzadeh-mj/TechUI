using System.Globalization;

namespace TechUI.Mvc.Chart
{
    public sealed class ChartSeriesBuilder
    {
        private readonly List<double?> _data = new();
        private readonly List<ChartPoint> _points = new();
        private readonly List<ChartCandle> _candles = new();
        private readonly List<double?> _targets = new();

        internal ChartSeriesBuilder()
        {
        }

        internal string? Name { get; private set; }

        internal string? SeriesType { get; private set; }

        internal string? SeriesColor { get; private set; }

        internal bool IsDashed { get; private set; }

        internal double? FillOpacity { get; private set; }

        internal bool IsHidden { get; private set; }

        internal string? StackName { get; private set; }

        public ChartSeriesBuilder Title(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            return this;
        }

        public ChartSeriesBuilder Type(string type)
        {
            SeriesType = string.IsNullOrWhiteSpace(type)
                ? null
                : type.Trim().ToLowerInvariant();
            return this;
        }

        public ChartSeriesBuilder Values(params double?[] values)
        {
            _data.Clear();

            if (values is not null)
            {
                _data.AddRange(values);
            }

            return this;
        }

        public ChartSeriesBuilder Values(IEnumerable<double?> values)
        {
            _data.Clear();

            if (values is not null)
            {
                _data.AddRange(values);
            }

            return this;
        }

        public ChartSeriesBuilder Values(IEnumerable<double> values)
        {
            _data.Clear();

            if (values is not null)
            {
                _data.AddRange(values.Select(value => (double?)value));
            }

            return this;
        }

        public ChartSeriesBuilder Points(params ChartPoint[] points)
        {
            _points.Clear();

            if (points is not null)
            {
                _points.AddRange(points);
            }

            return this;
        }

        public ChartSeriesBuilder Points(IEnumerable<ChartPoint> points)
        {
            _points.Clear();

            if (points is not null)
            {
                _points.AddRange(points);
            }

            return this;
        }

        public ChartSeriesBuilder Points(
            params (double X, double Y)[] points)
        {
            _points.Clear();

            if (points is not null)
            {
                _points.AddRange(points.Select(
                    point => new ChartPoint(point.X, point.Y)));
            }

            return this;
        }

        public ChartSeriesBuilder Bubbles(
            params (double X, double Y, double R)[] points)
        {
            _points.Clear();

            if (points is not null)
            {
                _points.AddRange(points.Select(
                    point => new ChartPoint(point.X, point.Y, point.R)));
            }

            return this;
        }

        public ChartSeriesBuilder Candles(params ChartCandle[] candles)
        {
            _candles.Clear();

            if (candles is not null)
            {
                _candles.AddRange(candles);
            }

            return this;
        }

        public ChartSeriesBuilder Candles(
            IEnumerable<ChartCandle> candles)
        {
            _candles.Clear();

            if (candles is not null)
            {
                _candles.AddRange(candles);
            }

            return this;
        }

        public ChartSeriesBuilder Targets(params double?[] targets)
        {
            _targets.Clear();

            if (targets is not null)
            {
                _targets.AddRange(targets);
            }

            return this;
        }

        public ChartSeriesBuilder Color(string color)
        {
            SeriesColor = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
            return this;
        }

        public ChartSeriesBuilder Dashed(bool dashed = true)
        {
            IsDashed = dashed;
            return this;
        }

        public ChartSeriesBuilder Opacity(double opacity)
        {
            FillOpacity = Math.Clamp(opacity, 0d, 1d);
            return this;
        }

        public ChartSeriesBuilder Hidden(bool hidden = true)
        {
            IsHidden = hidden;
            return this;
        }

        public ChartSeriesBuilder Stack(string name)
        {
            StackName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            return this;
        }

        internal Dictionary<string, object?> ToPayload()
        {
            var payload = new Dictionary<string, object?>
            {
                ["name"] = Name,
                ["type"] = SeriesType,
                ["color"] = SeriesColor
            };

            if (_data.Count > 0)
            {
                payload["data"] = _data.ToArray();
            }

            if (_points.Count > 0)
            {
                payload["points"] = _points.Select(point =>
                    new Dictionary<string, object?>
                    {
                        ["x"] = point.X,
                        ["y"] = point.Y,
                        ["r"] = point.R,
                        ["label"] = point.Label
                    }).ToList();
            }

            if (_candles.Count > 0)
            {
                payload["candles"] = _candles.Select(candle =>
                    new Dictionary<string, object?>
                    {
                        ["open"] = candle.Open,
                        ["high"] = candle.High,
                        ["low"] = candle.Low,
                        ["close"] = candle.Close
                    }).ToList();
            }

            if (_targets.Count > 0)
            {
                payload["targets"] = _targets.ToArray();
            }

            if (IsDashed)
            {
                payload["dash"] = true;
            }

            if (FillOpacity.HasValue)
            {
                payload["opacity"] = FillOpacity.Value;
            }

            if (IsHidden)
            {
                payload["hidden"] = true;
            }

            if (StackName is not null)
            {
                payload["stack"] = StackName;
            }

            return payload;
        }

        internal static string DefaultName(int index) =>
            "سری " + (index + 1).ToString(CultureInfo.InvariantCulture);
    }
}
