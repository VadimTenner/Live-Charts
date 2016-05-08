﻿//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Linq;

namespace LiveCharts.SeriesAlgorithms
{
    public class RowAlgorithm : SeriesAlgorithm, ICartesianSeries
    {
        public RowAlgorithm(ISeriesView view) : base(view)
        {
            XAxisMode = AxisLimitsMode.Separator;
            YAxisMode = AxisLimitsMode.UnitWidth;
            SeriesConfigurationType = SeriesConfigurationType.IndexedY;
        }

        public AxisLimitsMode XAxisMode { get; set; }
        public AxisLimitsMode YAxisMode { get; set; }

        public override void Update()
        {
            var fx = CurrentYAxis.GetFormatter();

            var columnSeries = (IRowSeries) View;

            const double padding = 5;
            
            var totalSpace = ChartFunctions.GetUnitWidth(AxisTags.Y, Chart, View.ScalesYAt) - padding;
            var columnSeriesCount = Chart.View.Series.OfType<IRowSeries>().Count();

            var singleRowHeight = totalSpace/columnSeriesCount;

            double exceed = 0;

            var seriesPosition = Chart.View.Series.IndexOf(View);

            if (singleRowHeight > columnSeries.MaxRowWidth)
            {
                exceed = (singleRowHeight - columnSeries.MaxRowWidth) * columnSeriesCount / 2;
                singleRowHeight = columnSeries.MaxRowWidth;
            }

            var relativeTop = padding + exceed + singleRowHeight * (seriesPosition);

            var startAt = CurrentXAxis.MinLimit >= 0 && CurrentXAxis.MaxLimit > 0   //both positive
                ? CurrentXAxis.MinLimit                                             //then use Min
                : (CurrentXAxis.MinLimit <= 0 && CurrentXAxis.MaxLimit < 0          //both negative
                    ? CurrentXAxis.MaxLimit                                         //then use Max
                    : 0);                                                           //if mixed then use 0

            var zero = ChartFunctions.ToDrawMargin(startAt, AxisTags.X, Chart, View.ScalesXAt);

            var correction = ChartFunctions.GetUnitWidth(AxisTags.Y, Chart, View.ScalesYAt);

            foreach (var chartPoint in View.Values.Points)
            {
                var reference =
                    ChartFunctions.ToDrawMargin(chartPoint, View.ScalesXAt, View.ScalesYAt, Chart);

                chartPoint.View = View.GetPointView(chartPoint.View,
                    View.DataLabels ? fx(chartPoint.X) : null);

                var rectangleView = (IRectangleData) chartPoint.View;

                var w = Math.Abs(reference.X - zero);
                var l = reference.X < zero
                    ? reference.X
                    : zero;

                rectangleView.Data.Height = singleRowHeight - padding;
                rectangleView.Data.Top = reference.Y + relativeTop - correction;

                rectangleView.Data.Left = l;
                rectangleView.Data.Width = w > 0 ? w : 0;

                rectangleView.ZeroReference = zero;

                chartPoint.ChartLocation = new LvcPoint(rectangleView.Data.Left + singleRowHeight / 2 - padding / 2,
                    l);

                chartPoint.View.DrawOrMove(null, chartPoint, 0, Chart);
            }
        }
    }
}
