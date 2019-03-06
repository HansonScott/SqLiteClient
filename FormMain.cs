using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        private enum TreeViewLevel
        {
            DataBase = 0,
            TableFolder = 1,
            Table = 2,
        }

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
            // check for an open database
            if (currentDB == null)
            {
                if (DBs.Count == 0)
                {
                    Output("No databases connected.");
                    return;
                }

                currentDB = DBs[0];
            }

            // capture content
            string sql;
            if (rtbContent.SelectedText.Length > 0)
            {
                sql = rtbContent.SelectedText;
            }
            else
            {
                sql = rtbContent.Text;
            }

            // run query
            try
            {
                SetResults(currentDB.Run(sql));
            }
            catch (Exception ex)
            {
                Output(ex.Message);
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CutAction(sender, e);
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            CopyAction(sender, e);
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            PasteAction(sender, e);
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            // Select * From...
            string sql = "SELECT * FROM ";
            TreeNode n = this.treeView1.SelectedNode;

            TreeViewLevel lvl = (TreeViewLevel)n.Tag;
            switch (lvl)
            {
                case TreeViewLevel.DataBase:
                    sql += "sqlite_master";
                    break;
                case TreeViewLevel.TableFolder:
                    sql += "sqlite_master WHERE type = 'table'";
                    break;
                case TreeViewLevel.Table:
                    sql += n.Text;
                    break;
                default:
                    break;
            }

            // put the query text into the query window
            if (rtbContent.Text.Length > 0)
            {
                rtbContent.AppendText(Environment.NewLine);
                rtbContent.AppendText(Environment.NewLine);
            }
            rtbContent.AppendText(sql);

            // and run it
            SetResults(currentDB.Run(sql));
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;

            string dbName = GetDBFromNode(e.Node);
            SetCurrentBD(dbName);
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
        void CutAction(object sender, EventArgs e)
        {
            if (rtbContent.Focused)
            {
                rtbContent.Cut();
            }
            else if (dgvResults.Focused)
            {
                //int i = dgvResults.SelectedCells.Count;
                // do nothing
                return;
            }
        }
        void CopyAction(object sender, EventArgs e)
        {
            if (rtbContent.Focused)
            {
                Clipboard.SetText(rtbContent.SelectedText);
            }
            else if (dgvResults.Focused)
            {
                string results = string.Empty;
                for (int i = 0; i < dgvResults.SelectedCells.Count; i++)
                {
                    if (results.Length > 0)
                    {
                        results += '|';
                    }

                    results += dgvResults.SelectedCells[i].Value.ToString();
                }

                Clipboard.SetText(results);
            }
        }
        void PasteAction(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText()) { return; }

            string s = Clipboard.GetText(TextDataFormat.Text);
            if (s == null || s.Length == 0) { return; }


            if (rtbContent.Focused)
            {
                int pos = rtbContent.SelectionStart;
                rtbContent.SelectedText = s;
            }
            else if (dgvResults.Focused)
            {
                //int i = dgvResults.SelectedCells.Count;
                // do nothing
                return;
            }
        }
        #endregion

        #region UI and admin functions
        private void ReLoadDBs()
        {
            treeView1.Nodes.Clear();

            foreach(DAC d in DBs)
            {
                string dbName = Path.GetFileName(d.FileName);
                TreeNode dbNode = new TreeNode(dbName);
                dbNode.Tag = TreeViewLevel.DataBase;

                treeView1.Nodes.Add(dbNode);


                TreeNode tblHeaderNode = new TreeNode("Tables");
                tblHeaderNode.Tag = TreeViewLevel.TableFolder;
                dbNode.Nodes.Add(tblHeaderNode);

                DataTable tbls = d.GetTables();
                if(tbls == null) { return; }
                foreach(DataRow tblRow in tbls.Rows)
                {
                    TreeNode tblNode = new TreeNode(tblRow["Name"].ToString());
                    tblNode.Tag = TreeViewLevel.Table;
                    tblHeaderNode.Nodes.Add(tblNode);
                }
            }

            treeView1.Refresh();
        }
        private string GetDBFromNode(TreeNode node)
        {
            TreeNode currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode.Text;
        }
        private void SetCurrentBD(string name)
        {
            foreach (DAC d in DBs)
            {
                if (Path.GetFileName(d.FileName) == name) { currentDB = d; }
            }
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
