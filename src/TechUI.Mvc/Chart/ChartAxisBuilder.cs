namespace TechUI.Mvc.Chart
{
    public sealed class ChartAxisBuilder
    {
        internal bool Visible { get; private set; } = true;

        internal bool ShowLabels { get; private set; } = true;

        internal bool ShowGrid { get; private set; } = true;

        internal string? AxisTitle { get; private set; }

        internal string? ValueFormat { get; private set; }

        internal double? Minimum { get; private set; }

        internal double? Maximum { get; private set; }

        internal bool? StartAtZero { get; private set; }

        internal int RotateLabels { get; private set; }

        public ChartAxisBuilder Show(bool show = true)
        {
            Visible = show;
            return this;
        }

        public ChartAxisBuilder Title(string title)
        {
            AxisTitle = string.IsNullOrWhiteSpace(title)
                ? null
                : title.Trim();
            return this;
        }

        public ChartAxisBuilder Labels(bool show = true)
        {
            ShowLabels = show;
            return this;
        }

        public ChartAxisBuilder Grid(bool show = true)
        {
            ShowGrid = show;
            return this;
        }

        /// <summary>
        /// number, percent, currency or compact.
        /// </summary>
        public ChartAxisBuilder Format(string format)
        {
            ValueFormat = string.IsNullOrWhiteSpace(format)
                ? null
                : format.Trim().ToLowerInvariant();
            return this;
        }

        public ChartAxisBuilder Min(double value)
        {
            Minimum = value;
            return this;
        }

        public ChartAxisBuilder Max(double value)
        {
            Maximum = value;
            return this;
        }

        public ChartAxisBuilder BeginAtZero(bool beginAtZero = true)
        {
            StartAtZero = beginAtZero;
            return this;
        }

        public ChartAxisBuilder Rotate(int degrees)
        {
            RotateLabels = Math.Clamp(degrees, -90, 90);
            return this;
        }

        internal Dictionary<string, object?> ToPayload()
        {
            return new Dictionary<string, object?>
            {
                ["show"] = Visible,
                ["labels"] = ShowLabels,
                ["grid"] = ShowGrid,
                ["title"] = AxisTitle,
                ["format"] = ValueFormat,
                ["min"] = Minimum,
                ["max"] = Maximum,
                ["beginAtZero"] = StartAtZero,
                ["rotate"] = RotateLabels
            };
        }
    }
}
