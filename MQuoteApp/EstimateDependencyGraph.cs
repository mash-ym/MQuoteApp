using System.Collections.Generic;

namespace MQuoteApp
{
    public class EstimateDependencyGraph
    {
        private Dictionary<string, List<string>> _dependencies; // キー：見積項目の名前、値：依存する見積項目の名前のリスト

        public EstimateDependencyGraph()
        {
            _dependencies = new Dictionary<string, List<string>>();
        }

        // 新しい見積項目を追加する
        public void AddEstimateItem(string itemName)
        {
            if (!_dependencies.ContainsKey(itemName))
            {
                _dependencies.Add(itemName, new List<string>());
            }
        }

        // 見積項目Aが見積項目Bに依存していることを表現するエッジを追加する
        public void AddDependency(string fromItem, string toItem)
        {
            if (_dependencies.ContainsKey(fromItem))
            {
                if (!_dependencies[fromItem].Contains(toItem))
                {
                    _dependencies[fromItem].Add(toItem);
                }
            }
            else
            {
                _dependencies.Add(fromItem, new List<string> { toItem });
            }
        }

        // 見積項目の名前を指定して、その見積項目が依存している見積項目の名前のリストを取得する
        public List<string> GetDependencies(string itemName)
        {
            return _dependencies.ContainsKey(itemName) ? _dependencies[itemName] : new List<string>();
        }
        private void CalculateFinishDate(EstimateItem item)
        {
            if (item.StartDate.HasValue && item.Duration.HasValue)
            {
                item.FinishDate = item.StartDate.Value.AddDays(item.Duration.Value);

                // 子ノードの終了日を再帰的に計算
                foreach (var childItem in item.Children)
                {
                    CalculateFinishDate(childItem);
                }
            }
        }

    }

}
