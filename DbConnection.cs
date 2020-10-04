using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AdoptionAlacrityDashboard
{
    public static class DbConnection
    {
        public const string GenderQuery = @"SELECT * FROM [dbo].[GenderRawDataView]";

        public const string SpecialNeedsQuery = @"SELECT * FROM [dbo].[SpecialNeedsRawDataView]";

        public const string AdoptionSubsidyQuery = @"SELECT * FROM [dbo].[AdoptionSubsidyRawDataView]";

        public const string PriorRelationshipQuery = @"SELECT * FROM [dbo].[PriorRelationshipRawDataView]";

        public const string FamilyStructureQuery = @"SELECT * FROM [dbo].[FamilyStructureRawDataView]";

        public const string RaceQuery = @"SELECT * FROM [dbo].[RaceRawDataView]";

        public const string TprToAdoptQuery = @"SELECT * FROM [dbo].[TprToAdoptRawDataView]";

        public const string FinalAgeQuery = @"SELECT * FROM [dbo].[FinalAgeRawDataView]";

        public const string Observations2016 = @"SELECT * FROM [dbo].[ObservationsByStateYear] WHERE [YEAR]=2016 ";

        public const string RegressionQuery = @"SELECT * FROM [dbo].[ObservationsByStateYear] ORDER BY Subsidy ASC";

        internal static SqlConnection SqlConnection
        {
            get
            {
                if (sqlConnection == null)
                {
                    sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AdoptionStatistics"].ConnectionString);
                    sqlConnection.Open();
                }

                return sqlConnection;
            }
        }

        public static void CloseConnection()
        {
            Logger.Log.WriteLog("closing SQL connection");
            if (sqlConnection != null &&
                sqlConnection.State != ConnectionState.Closed)
            {
                sqlConnection.Close();
            }

            sqlConnection = null;
        }

        private static SqlConnection sqlConnection;
    }
}
