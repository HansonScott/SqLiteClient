using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace SQLiteClient
{
    public class DAC
    {

        // for reference, the sqlite language: https://www.sqlite.org/lang.html

        private string mFileName;
        private SQLiteConnection mCon;

        private string tblTest = "testTable";

        public string FileName
        {
            get { return mFileName; }
        }


        public DAC (string SqlFileName)
        {
            mFileName = SqlFileName;

            string sCon = $"Data Source={SqlFileName}; Version=3";
            mCon = new SQLiteConnection(sCon);
        }

        public void CreateFile()
        {
            SQLiteConnection.CreateFile(this.mFileName);
        }

        public void CreateTable()
        {
            string sql = $"CREATE TABLE {tblTest} (name VARCHAR(20), score INT)";
            RunCommand(sql);
        }

        public void InsertData()
        {
            string sql = $"insert into {tblTest} (name, score) values ('Me', 3000)";
            RunCommand(sql);

            sql = $"insert into {tblTest} (name, score) values ('Myself', 6000)";
            RunCommand(sql);

            sql = $"insert into {tblTest} (name, score) values ('And I', 9001)";
            RunCommand(sql);
        }

        public DataSet SelectData()
        {
            string sql = $"select * from {tblTest} order by score desc";
            return RunSelect(sql);
        }

        private DataSet RunSelect(string sql)
        {
            DataSet ds = new DataSet();
            var da = new SQLiteDataAdapter(sql, mCon);

            try
            {
                mCon.Open();
                da.Fill(ds);

            }
            finally
            {
                mCon.Close();
            }

            return ds;
        }
        private void RunCommand(string sql)
        {
            SQLiteCommand comm = new SQLiteCommand(sql, mCon);
            try
            {
                mCon.Open();
                comm.ExecuteNonQuery();
            }
            finally
            {
                mCon.Close();
            }
        }

        public DataTable GetMaster()
        {
            string sql = "SELECT * FROM sqlite_master";
            DataSet ds = RunSelect(sql);
            if (ds.Tables != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }

            else return null;
        }

        internal DataTable GetTables()
        {
            string sql = "SELECT * FROM sqlite_master where Type='table'";
            return Run(sql);
        }

        internal DataTable Run(string sql)
        {
            return RunSelect(sql).Tables[0];
        }
    }
}
