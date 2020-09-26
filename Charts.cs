using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace AdoptionAlacrityDashboard
{
    public class Charts
    {
        public static void CreateChartByStateYear(ref Chart chart, string query, SeriesChartType chartType, int stateId, int year, string text)
        {
            SqlCommand command = new SqlCommand($"{query} WHERE [Year]=@year AND [StateId]=@stateId", DbConnection.SqlConnection);
            command.Parameters.Add("@StateId", SqlDbType.Int);
            command.Parameters["@StateId"].Value = stateId;
            command.Parameters.Add("@year", SqlDbType.Int);
            command.Parameters["@year"].Value = year;

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            if (table.Rows.Count > 0)
            {
                double total = 0.0;
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (DataColumn column in table.Columns)
                {
                    if (column.ColumnName != "StateId" && column.ColumnName != "Year")
                    {
                        total += Convert.ToDouble(table.Rows[0][column.ColumnName]);
                        dict.Add(column.ColumnName, table.Rows[0][column.ColumnName]);
                    }
                }

                if (chartType == SeriesChartType.Pie ||
                    text.Equals("Race"))
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        double percent = Convert.ToDouble(table.Rows[0][column.ColumnName]) / total;
                        // don't include columns less that 1% in pie charts or in the Race chart
                        if (percent < 0.01)
                        {
                            dict.Remove(column.ColumnName);
                        }
                    }
                }

                string[] x = dict.Keys.ToArray();
                object[] y = dict.Values.ToArray();
                CreateChart(x, y, chartType, ref chart, "Total Children");
            }
        }

        public static void CreateChart(string[]x, object[]y, SeriesChartType chartType,  ref Chart chart, string axisYLable)
        {
            chart.Series[0].ChartType = chartType;
            chart.Series[0].Font = new Font("Arial", 12);
            chart.Series[0].Points.DataBindXY(x, y);
            chart.Series[0].Name = "Adoption Data";
            chart.Legends[0].Enabled = true;
            chart.ChartAreas[0].Area3DStyle.Enable3D = true;
            chart.ChartAreas[0].AxisY.Title = axisYLable;
            chart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12);
            chart.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = 10;
        }

        public static void CreateTitle(string text, Chart chart)
        {
            Title title = new Title(text)
            {
                Font = new Font("Arial", 14, FontStyle.Bold)
            };

            chart.Titles.Add(title);
        }

        public static double CreateRegressionChart(ref Chart regressionChart)
        {
            SqlCommand command = new SqlCommand(DbConnection.RegressionQuery, DbConnection.SqlConnection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);

            double[] x2012 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2012
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("Subsidy")).ToArray();

            double[] y2012 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2012
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("AverageMonths")).ToArray();

            double[] x2013 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2013
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("Subsidy")).ToArray();

            double[] y2013 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2013
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("AverageMonths")).ToArray();

            double[] x2014 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2014
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("Subsidy")).ToArray();

            double[] y2014 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2014
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("AverageMonths")).ToArray();

            double[] x2015 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2015
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("Subsidy")).ToArray();

            double[] y2015 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2015
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("AverageMonths")).ToArray();

            double[] x2016 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2016
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("Subsidy")).ToArray();

            double[] y2016 = (from p in table.AsEnumerable()
                              where Convert.ToInt32(p.Field<object>("Year")) == 2016
                              orderby p.Field<double>("Subsidy") ascending
                              select p.Field<double>("AverageMonths")).ToArray();

            var points = (from DataRow p in table.Rows
                          orderby p.Field<double>("Subsidy") ascending
                          select new System.Windows.Point((double)p["Subsidy"], (double)p["AverageMonths"])).ToArray();

            var regressionLine = Regression.GetRegressionLine(points, out double r);
            double[] rlx = (from p in regressionLine select p.X).ToArray();
            double[] rly = (from p in regressionLine select p.Y).ToArray();

            // regression line
            regressionChart.Series[0].ChartType = SeriesChartType.Line;
            regressionChart.Series[0].Font = new Font("Arial", 12);
            regressionChart.Series[0].Points.DataBindXY(rlx, rly);
            regressionChart.Series[0].Name = "Regresion Line";
            regressionChart.Series[0].MarkerSize = 10;
            regressionChart.Series[0].MarkerBorderWidth = 15;
            regressionChart.Legends[0].Enabled = true;
            regressionChart.ChartAreas[0].Area3DStyle.Enable3D = true;
            regressionChart.ChartAreas[0].AxisX.Title = "% Receiving Adoption Subsidy";
            regressionChart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12);
            regressionChart.ChartAreas[0].AxisY.Title = "TPR to Adoption Avg Months";
            regressionChart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12);
            regressionChart.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = 10;

            // scatter plots
            regressionChart.Series.Add("2012");
            regressionChart.Series[1].Points.DataBindXY(x2012, y2012);
            regressionChart.Series[1].ChartType = SeriesChartType.Point;
            regressionChart.Series[1].MarkerColor = Color.OrangeRed;
            regressionChart.Series[1].MarkerStyle = MarkerStyle.Square;

            regressionChart.Series.Add("2013");
            regressionChart.Series[2].Points.DataBindXY(x2013, y2013);
            regressionChart.Series[2].ChartType = SeriesChartType.Point;
            regressionChart.Series[2].MarkerColor = Color.Orange;
            regressionChart.Series[2].MarkerStyle = MarkerStyle.Square;

            regressionChart.Series.Add("2014");
            regressionChart.Series[3].Points.DataBindXY(x2014, y2014);
            regressionChart.Series[3].ChartType = SeriesChartType.Point;
            regressionChart.Series[3].MarkerColor = Color.Navy;
            regressionChart.Series[3].MarkerStyle = MarkerStyle.Square;

            regressionChart.Series.Add("2015");
            regressionChart.Series[4].Points.DataBindXY(x2015, y2015);
            regressionChart.Series[4].ChartType = SeriesChartType.Point;
            regressionChart.Series[4].MarkerColor = Color.MediumVioletRed;
            regressionChart.Series[4].MarkerStyle = MarkerStyle.Square;

            regressionChart.Series.Add("2016");
            regressionChart.Series[5].Points.DataBindXY(x2016, y2016);
            regressionChart.Series[5].ChartType = SeriesChartType.Point;
            regressionChart.Series[5].MarkerColor = Color.ForestGreen;
            regressionChart.Series[5].MarkerStyle = MarkerStyle.Square;

            return r;
        }
    }
}
