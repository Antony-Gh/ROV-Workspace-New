using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ROV_GUI_Control.ViewModels
{
    public class Plot : INotifyPropertyChanged
    {
        private readonly LineSeries lineSeries;
        private readonly PlotModel plotModel;
        private readonly ConcurrentQueue<(DateTime time, double value)> dataQueue = new();
        private readonly DispatcherTimer plotTimer;
        private readonly CancellationTokenSource cts = new();
        private readonly int maxPoints = 350
            ;
        private DateTime lastInterval = DateTime.Now;
        private readonly TimeSpan maxInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan windowMinimum = TimeSpan.FromSeconds(5);
        private readonly TimeSpan windowMaximum = TimeSpan.FromSeconds(10);
        public Plot(PlotModel model, string yTitle)
        {
            plotModel = model;
            plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss tt",
                Title = "Time (Sec)",
                IntervalType = DateTimeIntervalType.Seconds,
                Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(15)),
                Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = yTitle,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = 0,
                Maximum = 5,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            lineSeries = new LineSeries
            {
                Title = "seriesTitle",
                LineStyle = LineStyle.Solid,
                Color = OxyColors.DodgerBlue
            };
            plotModel.Series.Add(lineSeries);

            plotTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            plotTimer.Tick += UiTimer_Tick;
        }
        private void UiTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (now - lastInterval >= maxInterval)
            {
                plotModel.Axes[0].Minimum = DateTimeAxis.ToDouble(now - TimeSpan.FromTicks(windowMinimum.Ticks));
                plotModel.Axes[0].Maximum = DateTimeAxis.ToDouble(now + TimeSpan.FromTicks(windowMaximum.Ticks));
                lastInterval = now - TimeSpan.FromTicks(windowMinimum.Ticks);
            }
            while (dataQueue.TryDequeue(out var point))
            {
                if (lineSeries.Points.Count >= maxPoints)
                    lineSeries.Points.RemoveAt(0);
                double x = DateTimeAxis.ToDouble(point.time);
                lineSeries.Points.Add(new DataPoint(x, point.value));
            }
            plotModel.InvalidatePlot(true);
        }
        public void Start(Func<double> dataProducer, int intervalMs = 20)
        {
            plotTimer.Start();

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    double value = dataProducer();
                    dataQueue.Enqueue((DateTime.Now, value));
                    await Task.Delay(intervalMs);
                }
            });
        }
        public void Stop()
        {
            cts.Cancel();
            plotTimer.Stop();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}