using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MQuoteApp
{

    public class QuoteDGV : DataGridView
    {
        private DataGridView dataGridView1;
        private int _draggedIndex;

        public QuoteDGV()
        {

            AllowDrop = true; 

        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                _draggedIndex = this.HitTest(e.X, e.Y).RowIndex;
                if (_draggedIndex >= 0)
                {
                    this.DoDragDrop(this.Rows[_draggedIndex], DragDropEffects.Move);
                }
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                var targetIndex = this.HitTest(e.X, e.Y).RowIndex;
                if (targetIndex >= 0 && targetIndex != _draggedIndex)
                {
                    e.Effect = DragDropEffects.Move;
                    this.Rows[_draggedIndex].Selected = false;
                    this.Rows[targetIndex].Selected = true;
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                var targetIndex = this.HitTest(e.X, e.Y).RowIndex;
                if (targetIndex >= 0 && targetIndex != _draggedIndex)
                {
                    var draggedRow = (DataGridViewRow)e.Data.GetData(typeof(DataGridViewRow));
                    this.Rows.RemoveAt(_draggedIndex);
                    this.Rows.Insert(targetIndex, draggedRow);
                    this.CurrentCell = this[0, targetIndex];
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C) // Ctrl + Cでコピー
            {
                CopySelectedRowsToClipboard();
            }
            else if (e.Control && e.KeyCode == Keys.V) // Ctrl + Vで貼り付け
            {
                // 貼り付け処理を実装
                PasteRows();
            }
            else if (e.Control && e.KeyCode == Keys.X) // Ctrl + Xで切り取り
            {
                // 切り取り処理を実装
                CutSelectedRows();
            }
            else if (e.Control && e.KeyCode == Keys.Enter) // Enterキーで次の列に移動
            {
                // Enterキー処理を実装
                e.Handled = true;
                int currentColumnIndex = CurrentCell.ColumnIndex;
                int currentRowIndex = CurrentCell.RowIndex;
                if (currentColumnIndex == Columns.Count - 1)
                {
                    if (currentRowIndex < Rows.Count - 1)
                    {
                        CurrentCell = Rows[currentRowIndex + 1].Cells[0];
                    }
                }
                else
                {
                    CurrentCell = Rows[currentRowIndex].Cells[currentColumnIndex + 1];

                }
            }
        }

        private void CopySelectedRowsToClipboard()
        {
            // 選択された行を取得
            DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

            // 行データを文字列に連結してクリップボードにコピー
            StringBuilder sb = new StringBuilder();
            foreach (DataGridViewRow row in selectedRows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    sb.Append(cell.Value);
                    sb.Append("\t"); // タブ区切りで列を区切る
                }
                sb.Remove(sb.Length - 1, 1); // 最後のタブを削除
                sb.AppendLine(); // 行を区切る
            }
            Clipboard.SetText(sb.ToString());
        }

        private void CutSelectedRows()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                return;
            }

            // 切り取り先のコレクションを作成する
            var cutCollection = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                cutCollection.Add(row);
            }

            // コレクションから削除する
            foreach (var row in cutCollection)
            {
                dataGridView1.Rows.Remove(row);
            }
        }

        private void PasteRows()
        {
            // クリップボードからデータを取得する
            string clipboardData = Clipboard.GetText();

            // クリップボードのデータが空でない場合、行を貼り付ける
            if (!string.IsNullOrEmpty(clipboardData))
            {
                // 改行で分割して、行ごとのデータを取得する
                string[] rowData = clipboardData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // 選択した行の最後の行番号を取得する
                int lastRowIndex = dataGridView1.SelectedRows[dataGridView1.SelectedRows.Count - 1].Index;

                // 選択した行の最後の行の次の行に新しい行を挿入する
                dataGridView1.Rows.Insert(lastRowIndex + 1);

                // 挿入した行にクリップボードから取得したデータを貼り付ける
                for (int i = 0; i < rowData.Length; i++)
                {
                    // タブで分割して、セルごとのデータを取得する
                    string[] cellData = rowData[i].Split('\t');

                    // セルの数が列の数よりも大きい場合は、余分なセルを切り捨てる
                    int cellCount = Math.Min(cellData.Length, dataGridView1.ColumnCount);

                    // セルのデータを設定する
                    for (int j = 0; j < cellCount; j++)
                    {
                        dataGridView1.Rows[lastRowIndex + i + 1].Cells[j].Value = cellData[j];
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(240, 150);
            this.dataGridView1.TabIndex = 0;
            // 
            // QuoteDGV
            // 
            this.RowTemplate.Height = 24;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
