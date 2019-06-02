using System;
using Xunit;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
//using DataGridView.DataSource;

namespace TheGame.IntegrationTests
{
    public class UnitTest1
    {
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
            while (!sr.EndOfStream)
            {
                string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        [Fact]
        public void Test1()
        {
            string filepath = @"C:\Users\julia\source\repos\theprojectgame\TheGame\TheGame\Configfile\reportlog.csv";
            DataTable dt = ConvertCSVtoDataTable(filepath);


        }

        

    }
}
