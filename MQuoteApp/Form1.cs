using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace MQuoteApp
{
    public partial class MainForm : Form
    {
        // データベースファイルのパス
        private const string dbFilePath = @"C:\path\to\database.db";
        TreeView treeView1 = new TreeView();
        public MainForm()
        {
            InitializeComponent();
            // 新しいコンテキストメニューを作成する
            ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();
            // 新しいメニュー項目を作成する
            ToolStripMenuItem menuItem1 = new ToolStripMenuItem();
            menuItem1.Text = "Copy";
            menuItem1.Click += new EventHandler(copyToolStripMenuItem_Click);

            ToolStripMenuItem menuItem2 = new ToolStripMenuItem();
            menuItem2.Text = "Delete";
            menuItem2.Click += new EventHandler(delete_Click);
            ToolStripMenuItem menuItem3 = new ToolStripMenuItem();
            menuItem3.Text = "Paste";
            menuItem3.Click += new EventHandler(pasteToolStripMenuItem_Click);

            // コンテキストメニューにメニュー項目を追加する
            contextMenuStrip1.Items.Add(menuItem1);
            contextMenuStrip1.Items.Add(menuItem2);
            contextMenuStrip1.Items.Add(menuItem3);
            // 削除メニュー項目がクリックされたときの処理

            // TreeViewのContextMenuStripプロパティに新しいコンテキストメニューを割り当てる
            treeView1.ContextMenuStrip = contextMenuStrip1;

        }
      
        private void delete_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                treeView1.SelectedNode.Remove();
            }
        }
        private TreeNode copiedNode;
        private void ShowProjectData(Project project)
        {
            dataGridView1.DataSource = project.GetQuoteData();
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                // 選択されたノードをコピー
                copiedNode = (TreeNode)treeView1.SelectedNode.Clone();
            }
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (copiedNode != null && treeView1.SelectedNode != null)
            {
                // 貼り付け先のノードにコピーしたノードを追加
                treeView1.SelectedNode.Nodes.Add((TreeNode)copiedNode.Clone());
                treeView1.SelectedNode.Expand();
            }
        }



        // 新規プロジェクト作成ボタンがクリックされたときの処理
        private void NewProjectButton_Click(object sender, EventArgs e)
        {
            // 新しいプロジェクトを作成し、TreeViewに追加
            var newProject = new Project("新しいプロジェクト");
            var projectNode = new TreeNode(newProject.Name);
            projectNode.Tag = newProject;
            treeView1.Nodes.Add(projectNode);
            List<EstimateItem> estimateItems = new List<EstimateItem>();

            // DataGridViewに空のデータを表示
            dataGridView1.DataSource = new DataTable();
            // ルートノードを追加する
            TreeNode rootNode = new TreeNode("見積");
            treeView1.Nodes.Add(rootNode);

            // EstimateItemクラスのリストからTreeNodeを生成する
            foreach (EstimateItem item in estimateItems)
            {
                TreeNode itemNode = new TreeNode(item.ItemName);
                itemNode.Tag = item;
                rootNode.Nodes.Add(itemNode);
            }
        }

        // ノードが選択されたときの処理
        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 選択されたノードがProjectノードである場合
            if (e.Node.Tag is Project)
            {
                // DataGridViewにそのProjectの見積データを表示
                var project = (Project)e.Node.Tag;
                ShowQuoteData(project.GetQuoteData);
            }
            // 選択されたノードがQuoteノードである場合
            else if (e.Node.Tag is QuoteData)
            {
                // DataGridViewにその見積データを表示
                var quote = (QuoteData)e.Node.Tag;
                ShowQuoteData(new QuoteData[] { quote });
            }
        }
        

    // EstimateItemクラスからTreeNodeクラスに変換する
    private TreeNode ConvertToTreeNode(EstimateItem item)
        {
            TreeNode node = new TreeNode();
            node.Text = item.Name;
            node.Tag = item;
            return node;
        }

        // EstimateItemリストからTreeViewに表示するためのTreeNodeリストに変換する
        private List<TreeNode> ConvertToTreeNodes(List<EstimateItem> items)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (EstimateItem item in items)
            {
                TreeNode node = ConvertToTreeNode(item);
                nodes.Add(node);
            }
            return nodes;
        }
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // ノードをドラッグする
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            // ノードのみを受け入れる
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップされたノードをTreeViewに追加する
            TreeNode newNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            Point pt = treeView1.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = treeView1.GetNodeAt(pt);
            targetNode.Nodes.Add((TreeNode)newNode.Clone());
            newNode.Remove();
            targetNode.Expand();
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

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
