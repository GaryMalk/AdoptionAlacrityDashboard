using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AdoptionAlacrityDashboard
{
    public class Charts
    {
        private static DataTable regressionTable;

        public static void CreateChartByStateYear(ref Chart chart, string query, SeriesChartType chartType, int stateId, int year, string text)
        {
            SqlCommand command = new SqlCommand($"{query} WHERE [Year]=@year AND [StateId]=@stateId", DbConnection.SqlConnection);
            command.Parameters.Add("@StateId", SqlDbType.Int);
            command.Parameters["@StateId"].Value = stateId;
            command.Parameters.Add("@year", SqlDbType.Int);
            command.Parameters["@year"].Value = year;
            DataTable table = new DataTable();

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }
            catch (SqlException exp)
            {
                MessageBox.Show("SQL Error, see log file for details, closing");
                Logger.Log.WriteLog($"Error executing CreateChartByStateYear for StateId {stateId} {year}.  Closing.");
                Logger.Log.WriteLog("SqlException: " + exp.Message);
                Environment.Exit(1);
            }

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

        public static void InitializeRegressionChart(ref Chart regressionChart, int[] years)
        {
            try
            {
                // table only needs to be created and filled once, since we are grabbing all the columns
                SqlCommand command = new SqlCommand(DbConnection.RegressionQuery, DbConnection.SqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                regressionTable = new DataTable();
                adapter.Fill(regressionTable);
            }
            catch (SqlException exp)
            {
                MessageBox.Show("SQL Error, see log file for details. Closing.");
                Logger.Log.WriteLog("Error retrieving regression data.  Closing.");
                Logger.Log.WriteLog("SqlException: " + exp.Message);
                Environment.Exit(1);
            }

            // name the regression line
            regressionChart.Series[0].Name= "Regresion Line";

            // add charts for each of the scatter plots
            foreach (int year in years)
            {
                regressionChart.Series.Add(year.ToString());
            }
        }

        public static double CreateRegressionChart(ref Chart regressionChart, IndependentVariable variable, string axisTitle, int[] years)
        {
            if (regressionTable == null)
            {
                InitializeRegressionChart(ref regressionChart, years);
            }

            string variableName = variable.ToString();

            var points = (from DataRow p in regressionTable.Rows
                          orderby p.Field<double>(variableName) ascending
                          select new System.Windows.Point((double)p[variableName], (double)p["AverageMonths"])).ToArray();

            var regressionLine = Regression.GetRegressionLine(points, out double r);
            double[] rlx = (from p in regressionLine select p.X).ToArray();
            double[] rly = (from p in regressionLine select p.Y).ToArray();

            // regression line
            regressionChart.Series[0].ChartType = SeriesChartType.Line;
            regressionChart.Series[0].Font = new Font("Arial", 12);
            regressionChart.Series[0].Points.DataBindXY(rlx, rly);
            regressionChart.Series[0].MarkerSize = 10;
            regressionChart.Series[0].MarkerBorderWidth = 15;
            regressionChart.Legends[0].Enabled = true;
            regressionChart.ChartAreas[0].Area3DStyle.Enable3D = true;
            regressionChart.ChartAreas[0].AxisX.Title = axisTitle;
            regressionChart.ChartAreas[0].AxisX.TitleFont = new Font("Arial", 12);
            regressionChart.ChartAreas[0].AxisY.Title = "TPR to Adoption Avg Months";
            regressionChart.ChartAreas[0].AxisY.TitleFont = new Font("Arial", 12);
            regressionChart.ChartAreas[0].AxisX.LabelAutoFitMinFontSize = 10;

            // scatter plots
            foreach (int year in years)
            {
                double[] plotX = (from p in regressionTable.AsEnumerable()
                                  where Convert.ToInt32(p.Field<object>("Year")) == year
                                  orderby p.Field<double>(variableName) ascending
                                  select p.Field<double>(variableName)).ToArray();

                double[] plotY = (from p in regressionTable.AsEnumerable()
                                  where Convert.ToInt32(p.Field<object>("Year")) == year
                                  orderby p.Field<double>(variableName) ascending
                                  select p.Field<double>("AverageMonths")).ToArray();

                var series = regressionChart.Series.Where(c => c.Name.Equals(year.ToString())).First();
                series.Points.DataBindXY(plotX, plotY);
                series.ChartType = SeriesChartType.Point;
                series.MarkerStyle = MarkerStyle.Square;
            }

            return r;
        }
    }
}
