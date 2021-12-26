using CsvHelper;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using RelationalGit.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace RelationalGit.Calculation
{
    public class AnalyzerOpenReview
    {
        public void AnalyzeOpenReviewWorkload(long actualSimulationId, string analyzeResultPath, string type)
        {
            if (!Directory.Exists(analyzeResultPath))
                Directory.CreateDirectory(analyzeResultPath);
           
            CalculateOpenReviews(actualSimulationId, analyzeResultPath, type);
            
          
        }
        
        private static void CalculateOpenReviews(long actualId, string path,  string type)
            {
            var result = new List<OpenReviewResult>();
            
                var values = new List<int>();

                var AppSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "relationalgit.json");
                
                var builder = new ConfigurationBuilder()
              .AddJsonFile(AppSettingsPath);

                var Configuration = builder.Build();

                string connectionString =  Configuration.GetConnectionString("RelationalGit");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                var queryString = "";
                if (type == "quarter")
                {
                    queryString = $@"SELECT  NormalizedName,
                                   count(distinct(pullRequestId))as openreiews,
                                   DATEPART(QUARTER, DateTime) ,  DATEPART(year, Datetime)
                                   FROM [dbo].[DeveloperReviews] where SimulationId = @simId  Group by DATEPART(QUARTER, Datetime)  , NormalizedName,  DATEPART(year, Datetime)
                                   order by openreiews desc";
                }
                else if (type == "week")
                {
                    queryString = $@"SELECT  NormalizedName,
                                   count(distinct(pullRequestId))as openreiews,
                                    DATEPART(year, Datetime),DATEPART(month, DateTime) ,DATEPART(week, DateTime) 
                                   FROM [dbo].[DeveloperReviews] where SimulationId = @simId  Group by DATEPART(year, Datetime)  , NormalizedName,  DATEPART(month, Datetime), DATEPART(week, Datetime) 
                                   order by openreiews desc";
                }
                else if (type == "day")
                {
                    queryString = $@"SELECT  NormalizedName,
                                   avg(OpenReviews)as openreiews,
                                   DATEPART(MONTH, DateTime) ,  DATEPART(year, Datetime),  DATEPART(day, Datetime)
                                   FROM [dbo].[DeveloperOpenReviews] where SimulationId = @simId  Group by NormalizedName,   DATEPART(MONTH, DateTime) ,  DATEPART(year, Datetime),  DATEPART(day, Datetime)
                                   order by openreiews desc";
                }
                else if (type == "month")
                {
                    queryString = $@"SELECT  NormalizedName,
                                   count(distinct(pullRequestId))as openreiews,
                                    DATEPART(year, Datetime),DATEPART(month, DateTime) 
                                   FROM [dbo].[DeveloperReviews] where SimulationId = @simId  Group by DATEPART(year, Datetime)  , NormalizedName,  DATEPART(month, Datetime), DATEPART(week, Datetime) 
                                   order by openreiews desc";
                }
                else if (type == "year")
                {
                    queryString = $@"SELECT  NormalizedName,
                                   count(distinct(pullRequestId))as openreiews,
                                    DATEPART(year, Datetime)
                                   FROM [dbo].[DeveloperReviews] where SimulationId = @simId  Group by DATEPART(year, Datetime)  , NormalizedName
                                   order by openreiews desc";

                }
                
                SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@simId", actualId);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            values.Add(Convert.ToInt32(reader["openreiews"]));
                        }
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
                using (var dbContext = GetDbContext())
                {
                    var lossSimulation = dbContext.LossSimulations.Single(q => q.Id == actualId);


                    var openRevResult = new OpenReviewResult()
                    {
                        LossSimulation = lossSimulation
                    };

                    openRevResult.Results = values;


                    result.Add(openRevResult);
                }

            var csvName = "OpenreviewPer" + type + ".csv";
            WriteOprnReviws(result, Path.Combine(path, csvName));
        }
        
        private static void WriteOprnReviws(IEnumerable<OpenReviewResult> openReviewResults, string path)
        {
            using (var dt = new DataTable())
            {
               
                foreach (var openRevResult in openReviewResults)
                {
                    dt.Columns.Add(openRevResult.LossSimulation.KnowledgeShareStrategyType + "-" + openRevResult.LossSimulation.Id, typeof(double));
                }

                var rows = openReviewResults.ElementAt(0).Results
                    
                    .OrderBy(q => q)
                    .Select(q =>
                    {
                        var row = dt.NewRow();
                        row[0] = q;
                        return row;
                    }).ToArray();


                for (int j = 0; j < rows.Length - 1; j++)
                {
                    for (int i = 0; i < openReviewResults.Count(); i++)
                    {
                        rows[j][i] = openReviewResults.ElementAt(i).Results[j];
                    }
                    dt.Rows.Add(rows[j]);
                }

                for (int j = dt.Rows.Count - 1; j >= 0; j--)
                {
                    var isRowConstant = IsRowConstant(dt.Rows[j]);

                    if (isRowConstant)
                    {
                        dt.Rows.RemoveAt(j);
                    }
                }

               
               

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
        }
        private static bool IsRowConstant(DataRow row)
        {
            if (row.Table.Columns.Count <= 2)
                return false;

            var lastCheckedValue = row[1];

            for (int i = 2; i < row.Table.Columns.Count; i++)
            {
                if(lastCheckedValue.ToString() != row[i].ToString())
                {
                    return false;
                }
            }

            return true;
        }

        private static GitRepositoryDbContext GetDbContext()
        {
            return new GitRepositoryDbContext(autoDetectChangesEnabled: false);
        }

       
        public class OpenReviewResult
        {
            public LossSimulation LossSimulation { get; set; }

            public List<int> Results { get; set; } = new List<int>();

            
        }

       
    }
}
