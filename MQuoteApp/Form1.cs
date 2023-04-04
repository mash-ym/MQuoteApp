using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace MQuoteApp
{
    public partial class MainForm : Form
    {
        // データベースファイルのパス
        private const string dbFilePath = @"C:\path\to\database.db";

        public MainForm()
        {
            InitializeComponent();
        }

        // 新規プロジェクト作成ボタンがクリックされたときの処理
        private void NewProjectButton_Click(object sender, EventArgs e)
        {
            // 新しいプロジェクトを作成し、TreeViewに追加
            var newProject = new Project("新しいプロジェクト");
            var projectNode = new TreeNode(newProject.Name);
            projectNode.Tag = newProject;
            treeView1.Nodes.Add(projectNode);

            // DataGridViewに空のデータを表示
            dataGridView1.DataSource = new DataTable();
        }

        // ノードが選択されたときの処理
        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 選択されたノードがProjectノードである場合
            if (e.Node.Tag is Project)
            {
                // DataGridViewにそのProjectの見積データを表示
                var project = (Project)e.Node.Tag;
                ShowQuoteData(project.Quotes);
            }
            // 選択されたノードがQuoteノードである場合
            else if (e.Node.Tag is QuoteData)
            {
                // DataGridViewにその見積データを表示
                var quote = (QuoteData)e.Node.Tag;
                ShowQuoteData(new QuoteData[] { quote });
            }
        }

        // DataGridViewに見積データを表示するメソッド
        private void ShowQuoteData(QuoteData[] quotes)
        {
            // 接続文字列
            string connectionString = $"Data Source={dbFilePath}";

            // データベース接続オブジェクトを作成
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 見積データを取得するSQL文
                string sql = "SELECT * FROM Quotes WHERE ";
                for (int i = 0; i < quotes.Length; i++)
                {
                    if (i != 0)
                    {
                        sql += " OR ";
                    }
                    sql += $"(ProjectName = '{quotes[i].ProjectName}' AND SubcontractorName = '{quotes[i].SubcontractorName}')";
                }
                sql += ";";

                // SELECT文を実行してデータリーダーを取得
                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        // データリーダーから取得したデータをDataGridViewに表示
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        dataGridView1.DataSource = dataTable;
                    }
                }

                connection.Close();
            }
        }

        // DataGridViewのセルがダブルクリックされたときの処理
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        { // 選択された行の見積データを取得
            var selectedRow = dataGridView1.Rows[e.RowIndex];
            var projectName = selectedRow.Cells["ProjectName"].Value.ToString();
            var subcontractorName = selectedRow.Cells["SubcontractorName"].Value.ToString();
            var amount = selectedRow.Cells["Amount"].Value.ToString();
            var date = selectedRow.Cells["Date"].Value.ToString();
            // 見積データを編集するダイアログを表示
            using (var dialog = new EditQuoteDialog())
            {
                dialog.ProjectName = projectName;
                dialog.SubcontractorName = subcontractorName;
                dialog.Amount = amount;
                dialog.Date = date;

                var result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // 編集された見積データを保存
                    var editedQuote = new QuoteData
                    {
                        ProjectName = dialog.ProjectName,
                        SubcontractorName = dialog.SubcontractorName,
                        Amount = dialog.Amount,
                        Date = dialog.Date
                    };

                    quoteManager.UpdateQuoteData(selectedRow.Index, editedQuote);

                    // DataGridViewに表示されている見積データを更新
                    dataGridView1.Rows[selectedRow.Index].Cells["ProjectName"].Value = editedQuote.ProjectName;
                    dataGridView1.Rows[selectedRow.Index].Cells["SubcontractorName"].Value = editedQuote.SubcontractorName;
                    dataGridView1.Rows[selectedRow.Index].Cells["Amount"].Value = editedQuote.Amount;
                    dataGridView1.Rows[selectedRow.Index].Cells["Date"].Value = editedQuote.Date;
                }
            }
        }

    }
}
