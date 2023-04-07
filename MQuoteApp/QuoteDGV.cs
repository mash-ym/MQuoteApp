using System.Text;
using System.Windows.Forms;

namespace MQuoteApp
{

    public class QuoteDGV : DataGridView
    {
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
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left && _draggedIndex >= 0)
            {
                this.DoDragDrop(this.Rows[_draggedIndex], DragDropEffects.Move);
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            this.ClearSelection();
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(DataGridViewRow)))
            {
                var targetIndex = this.HitTest(e.X, e.Y).RowIndex;
                if (targetIndex >= 0 && targetIndex != _draggedIndex)
                {
                    e.Effect = DragDropEffects.Move;
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
                CopySelectedRowsToClipboard(this);
            }
            else if (e.Control && e.KeyCode == Keys.V) // Ctrl + Vで貼り付け
            {
                // 貼り付け処理を実装
                PasteRows(this);
            }
            else if (e.Control && e.KeyCode == Keys.X) // Ctrl + Xで切り取り
            {
                // 切り取り処理を実装
                CutSelectedRows(this);
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

        private void CopySelectedRowsToClipboard(DataGridView dgv)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    sb.Append(cell.Value);
                    sb.Append("\t");
                }

                sb.Remove(sb.Length - 1, 1); // Remove the last tab
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }

        private void CutSelectedRows(DataGridView dgv)
        {
            CopySelectedRowsToClipboard(dgv);

            foreach (DataGridViewRow row in dgv.SelectedRows)
            {
                dgv.Rows.Remove(row);
            }
        }

        private void PasteRows(DataGridView dgv)
        {
            string[] rows = Clipboard.GetText().Split('\n');

            foreach (string row in rows)
            {
                if (row.Length > 0)
                {
                    dgv.Rows.Add(row.Split('\t'));
                }
            }
        }
    }

}
