using SharpLearning.Containers.Matrices;
using SharpLearning.Neural.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AdoptionAlacrityDashboard
{
    public partial class DashboardForm : Form
    {
        string[] models = new string[] { "adoptModel_black.xml", "adoptModel_hispanic.xml", "adoptModel_nativeAmerican.xml", "adoptModel_white.xml" };
        decimal whitePercent;
        decimal blackPercent;
        decimal hispanicPercent;
        decimal nativeAmericanPercent;

        public DashboardForm()
        {
            Logger.Log.WriteLog("starting application");
            InitializeComponent();
            Logger.Log.WriteLog("unzipping models");
            foreach (string model in models)
            {
                FileInfo fileInfo = new FileInfo(model);
                if (!fileInfo.Exists)
                {
                    Logger.Log.WriteLog($"unzipping {model}");
                    ZipFile.ExtractToDirectory(model.Replace(".xml", ".zip"), ".");
                }
            }

            try
            {
                SqlCommand sqlCommand = new SqlCommand(DbConnection.YearsQuery, DbConnection.SqlConnection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    yearComboBox.Items.Add(reader.GetInt32(0));
                }

                reader.Close();
            }
            catch (SqlException exp)
            {
                MessageBox.Show("SQL Error, see log file for details.  Closing.");
                Logger.Log.WriteLog("Error loading State Data.  Closing.");
                Logger.Log.WriteLog("SqlException: " + exp.Message);
                Environment.Exit(1);
            }

            yearComboBox.SelectedIndex = 0;
            stateComboBox.SelectedIndex = 0;
            errorLabel.ForeColor = Color.Red;
            errorLabel.Text = "";

            UpdateStateCharts();
            Logger.Log.WriteLog("Adding Chart Titles");
            Charts.CreateTitle("Gender", genderChart);
            Charts.CreateTitle("Race", raceChart);
            Charts.CreateTitle("Family Structure", familyStructureChart);
            Charts.CreateTitle("Final Age at Adoption", finalAgeChart);
            Charts.CreateTitle("Prior Relationship", priorRelationshipChart);
            Charts.CreateTitle("Time Between TPR and Adoption", tprToAdoptChart);
            Charts.CreateTitle("Special Needs", specialNeedsChart);
            Charts.CreateTitle("Adoption Subsidy", adoptionSubsidyChart);
            CreateRegressionChart();
            ResizeComponents();
        }

        private void CreateRegressionChart()
        {
            IndependentVariable independentVariable = (IndependentVariable)variableComboBox.SelectedIndex;
            string axisTitle = string.Empty;
            switch (independentVariable)
            {
                case IndependentVariable.AverageAge:
                    axisTitle = "Average Age in Years";
                    break;
                case IndependentVariable.Subsidy:
                    axisTitle = "% Receiving Adoption Subsidy";
                    break;
                case IndependentVariable.Black:
                    axisTitle = "% Black or African American";
                    break;
                case IndependentVariable.Hispanic:
                    axisTitle = "% Hispanic";
                    break;
                case IndependentVariable.NativeAmerican:
                    axisTitle = "% Native American";
                    break;
                case IndependentVariable.White:
                    axisTitle = "% White Non-Hispanic";
                    break;
                case IndependentVariable.NonRelative:
                    axisTitle = "% Non Relative";
                    break;
                case IndependentVariable.Male:
                    axisTitle = "% Male";
                    break;
                case IndependentVariable.Married:
                    axisTitle = "% Married";
                    break;
            }

            Logger.Log.WriteLog("Creating Regression Chart");
            var years = yearComboBox.Items.Cast<int>().ToArray();
            double r = Charts.CreateRegressionChart(ref regressionChart, independentVariable, axisTitle, years);
            rLabel.Text = $"Correlation Coefficient r = {Math.Round(r, 3)}";
            string relation = "";

            if (Math.Abs(r) < 0.1)
            {
                relation = "No";
            }
            else
            {
                if (Math.Abs(r) < 0.4)
                {
                    relation = "Weak";
                }
                else if (Math.Abs(r) < 0.7)
                {
                    relation = "Moderate";
                }
                else
                {
                    relation = "Strong";
                }
                if (r < 0)
                {
                    relation += " Negative";
                }
            }

            relationLabel.Text = $"Relationship Between Independent and Dependent Variables: {relation} Correlation";
        }

        private void UpdateStateCharts()
        {
            int stateId = stateComboBox.SelectedIndex + 1;
            int year = int.Parse(yearComboBox.Text);

            // check if there is data first
            // no data for Puerto Rico 2016
            DataTable table = new DataTable();
            try
            {
                SqlCommand command = new SqlCommand($"{DbConnection.Observations} WHERE [YEAR]={year} AND StateId={stateId}", DbConnection.SqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }
            catch (SqlException exp)
            {
                MessageBox.Show("SQL Error, see log file for details.  Closing.");
                Logger.Log.WriteLog("Error loading State Data.  Closing.");
                Logger.Log.WriteLog("SqlException: " + exp.Message);
            }

            if (table.Rows.Count == 0)
            {
                noDatalabel.Text = $"No Data for {stateComboBox.Text} in {yearComboBox.Text}";
                raceChart.Visible = false;
                genderChart.Visible = false;
                finalAgeChart.Visible = false;
                priorRelationshipChart.Visible = false;
                tprToAdoptChart.Visible = false;
                specialNeedsChart.Visible = false;
                familyStructureChart.Visible = false;
                adoptionSubsidyChart.Visible = false;
            }
            else
            {
                noDatalabel.Text = "";
                Logger.Log.WriteLog($"Creating Charts for {stateComboBox.Text} {yearComboBox.Text}");

                Charts.CreateChartByStateYear(ref raceChart, DbConnection.RaceQuery, SeriesChartType.Column, stateId, year, "Race");
                Charts.CreateChartByStateYear(ref genderChart, DbConnection.GenderQuery, SeriesChartType.Pie, stateId, year, "Gender");
                Charts.CreateChartByStateYear(ref finalAgeChart, DbConnection.FinalAgeQuery, SeriesChartType.StackedArea, stateId, year, "Final Age at Adoption");
                Charts.CreateChartByStateYear(ref priorRelationshipChart, DbConnection.PriorRelationshipQuery, SeriesChartType.Pie, stateId, year, "Prior Relationship");
                Charts.CreateChartByStateYear(ref tprToAdoptChart, DbConnection.TprToAdoptQuery, SeriesChartType.StackedArea, stateId, year, "Time between TPR and Adoption");
                Charts.CreateChartByStateYear(ref specialNeedsChart, DbConnection.SpecialNeedsQuery, SeriesChartType.Pie, stateId, year, "Special Needs");
                Charts.CreateChartByStateYear(ref familyStructureChart, DbConnection.FamilyStructureQuery, SeriesChartType.Pie, stateId, year, "Adopting Family Structure");
                Charts.CreateChartByStateYear(ref adoptionSubsidyChart, DbConnection.AdoptionSubsidyQuery, SeriesChartType.Pie, stateId, year, "Adoption Subsidy");

                raceChart.Visible = true;
                genderChart.Visible = true;
                finalAgeChart.Visible = true;
                priorRelationshipChart.Visible = true;
                tprToAdoptChart.Visible = true;
                specialNeedsChart.Visible = true;
                familyStructureChart.Visible = true;
                adoptionSubsidyChart.Visible = true;
            }
        }

        private void ResizeComponents()
        {
            // tab 1
            int baseMargin = 15;
            int top = 115;
            int bottom = this.Bottom - baseMargin;

            tabControl1.Width = this.Width - 5;
            tabControl1.Height = this.Height - 5;

            /*
            First Row, Pie Charts: Gender, Prior Relationship, Family Structure, Special Needs, Adopt Subsidy
            Second Row, Column Bar Charts: Race, Final Age at Adoption, Months between TPR and Adoption
            divide available width space by the number of charts, allowing for margins
            */

            int width = this.Width - 2 * baseMargin;
            int firstRowWidth = (width - 5 * baseMargin) / 5;
            int secondRowWidth = (width - 4 * baseMargin) / 3;

            int height = this.Height - top - 2 * baseMargin;
            int rowHeight = (height - 3 * baseMargin) / 2;

            int x = baseMargin;
            genderChart.Location = new Point(x, top);
            genderChart.Width = firstRowWidth;
            genderChart.Height = rowHeight;

            x = x + firstRowWidth + baseMargin;
            priorRelationshipChart.Location = new Point(x, top);
            priorRelationshipChart.Width = firstRowWidth;
            priorRelationshipChart.Height = rowHeight;

            x = x + firstRowWidth + baseMargin;
            familyStructureChart.Location = new Point(x, top);
            familyStructureChart.Width = firstRowWidth;
            familyStructureChart.Height = rowHeight;

            x = x + firstRowWidth + baseMargin;
            specialNeedsChart.Location = new Point(x, top);
            specialNeedsChart.Width = firstRowWidth;
            specialNeedsChart.Height = rowHeight;

            x = x + firstRowWidth + baseMargin;
            adoptionSubsidyChart.Location = new Point(x, top);
            adoptionSubsidyChart.Width = firstRowWidth;
            adoptionSubsidyChart.Height = rowHeight;

            // second row
            top = top + rowHeight + baseMargin;
            x = baseMargin;
            raceChart.Location = new Point(x, top);
            raceChart.Width = secondRowWidth;
            raceChart.Height = rowHeight;

            x = x + secondRowWidth + baseMargin;
            finalAgeChart.Location = new Point(x, top);
            finalAgeChart.Width = secondRowWidth;
            finalAgeChart.Height = rowHeight;

            x = x + secondRowWidth + baseMargin;
            tprToAdoptChart.Location = new Point(x, top);
            tprToAdoptChart.Width = secondRowWidth;
            tprToAdoptChart.Height = rowHeight;

            // resize tab 2
            foreach(Control control in regressionTabPage.Controls)
            {
                int moveControl = this.Width / 2 - (control.Right + control.Left) / 2;
                control.Left = control.Left + moveControl;
            }

            variableLabel.Left = regressionChart.Left + 100;
            variableComboBox.Left = variableLabel.Right + 15;

            // resize tab 3
            // need to set the initial center of tab 3 controls
            int tab3ContrlsCenter = (loadLabel.Left + moLabel1.Right) / 2;
            int difference = this.Width / 2 - tab3ContrlsCenter;
            foreach (Control control in predictiveTabPage.Controls)
            {
                control.Left = control.Left + difference;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DbConnection.CloseConnection();
            Logger.Log.WriteLog("closing application");
        }

        private void stateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStateCharts();
        }

        private void yearComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStateCharts();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizeComponents();
        }

        private void raceModelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedRaceLabel.Text = raceModelComboBox.Text + ":";
            otherRacesLabel.Text = $"% / Non-{selectedRaceLabel.Text} {Math.Round(100 - raceUpDown.Value,2)}%";

            // if data has been loaded from the state, the racial percentages have been cached
            if (!string.IsNullOrWhiteSpace(stateLoadComboBox.Text))
            {
                switch(raceModelComboBox.Text)
                {
                    case "Black":
                        raceUpDown.Value = blackPercent;
                        break;
                    case "Hispanic":
                        raceUpDown.Value = hispanicPercent;
                        break;
                    case "Native American":
                        raceUpDown.Value = nativeAmericanPercent;
                        break;
                    case "White":
                        raceUpDown.Value = whitePercent;
                        break;
                }
            }
        }

        private void raceUpDown_ValueChanged(object sender, EventArgs e)
        {
            otherRacesLabel.Text = $"% / Non-{selectedRaceLabel.Text} {Math.Round(100 - raceUpDown.Value,2)}%";
        }

        private void marriedNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            notMarriedLabel.Text = $"% / NonMarried: {Math.Round(100 - marriedNumericUpDown.Value,2)}%";
        }

        private void maleUpDown_ValueChanged(object sender, EventArgs e)
        {
            femaleLabel.Text = $"% / Female: {Math.Round(100 - maleUpDown.Value,2)}%";
        }

        private void relativeUpDown_ValueChanged(object sender, EventArgs e)
        {
            nonRelativeLabel.Text = $"% / NonRelative: {Math.Round(100 - relativeUpDown.Value,2)}%";
        }

        private async void predictButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(raceModelComboBox.Text) ||
                string.IsNullOrWhiteSpace(stateModelComboBox.Text))
            {
                errorLabel.Text = "ERROR: You must at a minimum select a state model and a racial model.";
            }
            else
            {
                errorLabel.Text = "";
                int length = 59;
                var observations = new F64Matrix(1, length);
                for (int n = 0; n < length; n++)
                {
                    // initialize to zero
                    observations[0, n] = 0.0;
                }

                // states are in alphabetical order and the select list is zero indexed, and the
                // OneHot encoded StateId columns come first, so the index is the same
                observations[0, stateModelComboBox.SelectedIndex] = 1.0;
                observations[0, 52] = 2016.0;
                observations[0, 53] = Convert.ToDouble(subsidyUpDown.Value);
                observations[0, 54] = Convert.ToDouble(marriedNumericUpDown.Value);
                observations[0, 55] = Convert.ToDouble(ageUpDown.Value);
                observations[0, 56] = Convert.ToDouble(maleUpDown.Value);
                observations[0, 57] = Convert.ToDouble(raceUpDown.Value);
                observations[0, 58] = Convert.ToDouble(relativeUpDown.Value);

                //progressBar1.Visible = true;
                //progressBar1.Style = ProgressBarStyle.Continuous;
                comparisonChart.Visible = false;
                pictureBox1.Visible = true;
                predictButton.Enabled = false;

                // cache state and actual in case the user changes it while the prediction model is running
                string stateName = stateLoadComboBox.Text;
                double actual = string.IsNullOrWhiteSpace(actualTextBox.Text) ? 0.0 : double.Parse(actualTextBox.Text);

                double prediction = 0.0;
                predictionProgressBar.Visible = true;
                predictionProgressBar.Style = ProgressBarStyle.Marquee;
                predictionProgressBar.MarqueeAnimationSpeed = 30;

                Logger.Log.WriteLog($"Running model prediction for {stateName}");
                string modelFile = $"adoptModel_{raceModelComboBox.Text.Replace(" ", "")}.xml";
                await Task.Run(() =>
                {
                    prediction = Predict(observations, modelFile);
                });

                predictionProgressBar.Visible = false;
                predictionProgressBar.Style = ProgressBarStyle.Continuous;
                predictionProgressBar.MarqueeAnimationSpeed = 0;
                predictionTextBox.Text = Math.Round(prediction, 3).ToString();
                pictureBox1.Visible = false;
                predictButton.Enabled = true;

                string[] x = { "Actual " + stateName, "Projected " + stateModelComboBox.Text };
                object[] y = { actual, prediction };

                Charts.CreateChart(x, y, SeriesChartType.Column, ref comparisonChart, "Months");
                comparisonChart.Visible = true;
            }
        }

        private double Predict(F64Matrix observations, string modelFile)
        {
            var model = RegressionNeuralNetModel.Load(() => new StreamReader(modelFile));
            return model.Predict(observations.Row(0));
        }

        private void stateLoadComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Logger.Log.WriteLog($"Loading data from {stateLoadComboBox.Text} for model");
            try
            {
                SqlCommand command = new SqlCommand($"{DbConnection.Observations} WHERE [YEAR]=2016 AND StateId=@StateId", DbConnection.SqlConnection);
                command.Parameters.Add("@StateId", SqlDbType.Int);
                command.Parameters["@StateId"].Value = stateLoadComboBox.SelectedIndex + 1;
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                // cache the race percentages so we can swap when the racial model is selected/changed
                blackPercent = 100.00m * Convert.ToDecimal(table.Rows[0]["Black"]);
                hispanicPercent = 100.00m * Convert.ToDecimal(table.Rows[0]["Hispanic"]);
                nativeAmericanPercent = 100.00m * Convert.ToDecimal(table.Rows[0]["NativeAmerican"]);
                whitePercent = 100.00m * Convert.ToDecimal(table.Rows[0]["White"]);

                raceUpDown.Value = string.IsNullOrWhiteSpace(raceModelComboBox.Text) ? whitePercent : 100.00m * Convert.ToDecimal(table.Rows[0][raceModelComboBox.Text.Replace(" ", "")]);

                stateModelComboBox.SelectedItem = stateLoadComboBox.SelectedItem;
                subsidyUpDown.Value = 100.00m * Convert.ToDecimal(table.Rows[0]["Subsidy"]);
                marriedNumericUpDown.Value = 100.00m * Convert.ToDecimal(table.Rows[0]["Married"]);
                maleUpDown.Value = 100.00m * Convert.ToDecimal(table.Rows[0]["Male"]);
                ageUpDown.Value = Convert.ToDecimal(table.Rows[0]["AverageAge"]);
                relativeUpDown.Value = 100.00m - 100.00m * Convert.ToDecimal(table.Rows[0]["NonRelative"]);
                actualTextBox.Text = table.Rows[0]["AverageMonths"].ToString();
            }
            catch (SqlException exp)
            {
                MessageBox.Show("SQL Error, see log file for details.  Closing.");
                Logger.Log.WriteLog("Error loading State Data.  Closing.");
                Logger.Log.WriteLog("SqlException: " + exp.Message);
                Environment.Exit(1);
            }
        }

        private void variableComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CreateRegressionChart();
        }
    }
}
