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
            //string filepath = @"C:\Users\julia\source\repos\theprojectgame\TheGame\TheGame\Configfile\reportlog.csv";
            string filepath = @"C:\Users\julia\source\repos\theprojectgame\TheGame\TheGame.IntegrationTests\reportlog_blue.csv";
            DataTable dt = ConvertCSVtoDataTable(filepath);

            /* SCENARIO when only blue players connect, checking for connectivity and players' color*/
            Object cellValue0 = dt.Rows[0][0];
            Object cellValue1 = dt.Rows[1][0];
            string val0 = cellValue0.ToString();
            string val1 = cellValue1.ToString();
            string ex = "Connect";

            string ex_bl = "blue";

            int c = dt.Rows.Count;
            for (int i = 0; i < c - 1; i++)
            {
                Object cellValue_blue = dt.Rows[i][3];
                string val_blue = cellValue_blue.ToString();
                Assert.Contains(ex_bl, val_blue);
            }
            
            Assert.Contains(ex, val0);
            Assert.Contains(ex, val1);
            
            //double otherNumber = dt.Rows[1].Field<string>("Type");

        }

        

    }
}
