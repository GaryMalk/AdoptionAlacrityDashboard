using System;
using System.Linq;
using System.Windows;

namespace AdoptionAlacrityDashboard
{
    public class Regression
    {
        public static Point[] GetRegressionLine(Point[] points, out double r)
        {
            Point[] regression = new Point[2];
            double x_mean = points.Average(p => p.X);
            double y_mean = points.Average(p => p.Y);

            // regression line always goes through the mean
            regression[0] = new Point(x_mean, y_mean);

            // calculate the standard deviation
            double x_sd = Math.Sqrt(points.Sum(p => (p.X - x_mean) * (p.X - x_mean)) / (points.Length - 1));
            double y_sd = Math.Sqrt(points.Sum(p => (p.Y - y_mean) * (p.Y - y_mean)) / (points.Length - 1));

            // calculate correlation coefficient r
            r = (1.0 / (points.Length - 1)) * points.Sum(p => ((p.X - x_mean) / x_sd) * ((p.Y - y_mean) / y_sd));

            // get the slope
            double m = r * (x_mean / y_mean);

            // use the definition of a linear equation y = mx + b
            // combined with the fact that the line must pass through the mean to get the x intercept
            double b = y_mean - m * x_mean;

            // add intercept to the line, all we need are two points
            regression[1] = new Point(0, b);
            return regression.OrderBy(p => p.X).ToArray();
        }
    }
}
