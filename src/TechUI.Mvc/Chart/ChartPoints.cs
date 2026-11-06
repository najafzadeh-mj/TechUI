namespace TechUI.Mvc.Chart
{
    public sealed class ChartPoint
    {
        public ChartPoint()
        {
        }

        public ChartPoint(double x, double y, double? r = null, string? label = null)
        {
            X = x;
            Y = y;
            R = r;
            Label = label;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double? R { get; set; }

        public string? Label { get; set; }
    }

    public sealed class ChartCandle
    {
        public ChartCandle()
        {
        }

        public ChartCandle(double open, double high, double low, double close)
        {
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }
    }
}
