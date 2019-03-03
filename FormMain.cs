using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLiteClient
{
    public partial class FormMain : Form
    {
        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
        #endregion

        #region Class Members
        List<DAC> DBs = new List<DAC>();
        DAC currentDB;
        #endregion

        #region Constructor and Setup
        public FormMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Menu and Toolbar Handlers
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                CreateNewFile(sfd.FileName);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                OpenFile(ofd.FileName);
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (currentDB == null)
            {
                if (DBs.Count == 0)
                {
                    Output("No databases connected.");
                    return;
                }

                currentDB = DBs[0];
            }

            string sql;
            if (rtbContent.SelectedText.Length > 0)
            {
                sql = rtbContent.SelectedText;
            }
            else
            {
                sql = rtbContent.Text;
            }

            try
            {
                SetResults(currentDB.Run(sql));
            }
            catch (Exception ex)
            {
                Output(ex.Message);
            }
        }
        #endregion

        #region Primary Business Functions
        private void CreateNewFile(string fileName)
        {
            DAC d = new DAC(fileName);
            d.CreateFile();

            DBs.Add(d);

            ReLoadDBs();
        }
        private void OpenFile(string fileName)
        {
            DAC d = new DAC(fileName);

            DBs.Add(d);

            ReLoadDBs();
        }
        #endregion

        #region UI and admin functions
        private void ReLoadDBs()
        {
            treeView1.Nodes.Clear();

            foreach(DAC d in DBs)
            {
                TreeNode dbNode = new TreeNode(d.FileName);
                treeView1.Nodes.Add(dbNode);

                TreeNode tblHeaderNode = new TreeNode("Tables");
                dbNode.Nodes.Add(tblHeaderNode);

                DataTable tbls = d.GetTables();
                if(tbls == null) { return; }
                foreach(DataRow tblRow in tbls.Rows)
                {
                    TreeNode tblNode = new TreeNode(tblRow["Name"].ToString());
                    tblHeaderNode.Nodes.Add(tblNode);
                }
            }

            treeView1.Refresh();
        }
        private void Output(string msg)
        {
            this.tbOutput.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + ": " + msg);
            this.tbOutput.AppendText(Environment.NewLine);
        }
        private void SetResults(DataTable data)
        {
            dgvResults.DataSource = data;
            dgvResults.Refresh();
        }
        #endregion
    }
}
