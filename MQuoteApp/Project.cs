using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MQuoteApp
{

    public class ConstructionRecord // 工事記録の一覧を保持
    {
        public DateTime Date { get; set; } // 日付
        public string Content { get; set; } // コンテンツ
    }

    public class CustomerInfo
    {
        public string Name { get; set; } // 顧客名
        public string Address { get; set; } // 住所
        public string PhoneNumber { get; set; } // 電話番号
        public string Email { get; set; }// メールアドレス
        public string LineAccount { get; set; } // Lineアカウント
        public List<Project> Projects { get; set; } // 工事情報

        public CustomerInfo(string name, string address, string phoneNumber, string email, string lineAccount)
        {
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            Email = email;
            LineAccount = lineAccount;
            Projects = new List<Project>();
        }

        public void AddRecord(string name, string projectName, DateTime startDate, DateTime endDate, CustomerInfo customer, DateTime estimateCreationDate, DateTime estimateExpirationDate, List<EstimateItem> estimateItems, Building building)
        {
            Projects.Add(new Project(name, projectName, startDate, endDate, customer, estimateCreationDate, estimateExpirationDate, estimateItems, building));
        }
    }

    public class Building
    {
        public string Address { get; set; } // 住所
        public string FloorPlan { get; set; } // 間取
        public double TotalFloorArea { get; set; } // 延べ床面積
        public int BuildingYear { get; set; } // 建築年
        public string Structure { get; set; } // 構造
        public List<Project> Projects { get; set; } // 工事情報

        public Building(string address, string floorPlan, double totalFloorArea, int buildingYear, string structure)
        {
            Address = address;
            FloorPlan = floorPlan;
            TotalFloorArea = totalFloorArea;
            BuildingYear = buildingYear;
            Structure = structure;
            Projects = new List<Project>();

        }
        public void AddRecord(string name, string projectName, DateTime startDate, DateTime endDate, CustomerInfo customer, DateTime estimateCreationDate, DateTime estimateExpirationDate, List<EstimateItem> estimateItems, Building building)
        {
            Projects.Add(new Project(name, projectName, startDate, endDate, customer, estimateCreationDate, estimateExpirationDate, estimateItems, building));
        }

    }

    public class Project : ProjectItem
    {
        public string ProjectName { get; set; } // 現場名
        public CustomerInfo Customer { get; set; } // 顧客情報
        public List<EstimateItem> EstimateItems { get; set; } 
        // プロジェクトに紐づく見積もりデータを保持するリスト
        public DateTime EstimateCreationDate { get; set; }
        public DateTime EstimateExpirationDate { get; set; }
        public Building Building { get; set; }
        public List<ConstructionRecord> Records { get; set; }
        public ProjectSchedule ProjectSchedule { get; set; } // 工期管理の追加

        public Project(string name, string projectName, DateTime startDate, DateTime finishDate, CustomerInfo customer, DateTime estimateCreationDate, DateTime estimateExpirationDate, List<EstimateItem> estimateItems, Building building)
            : base(startDate, finishDate)
        {
            Name = name;
            ProjectName = projectName;
            Customer = customer;
            EstimateCreationDate = estimateCreationDate;
            EstimateExpirationDate = estimateExpirationDate;
            StartDate = startDate;
            FinishDate = finishDate;
            EstimateItems = estimateItems;
            Building = building;
            Records = new List<ConstructionRecord>();
        }


        public void AddItem(EstimateItem item)
        {
            EstimateItems.Add(item);
        }
        public void AddRecord(DateTime date, string content)
        {
            Records.Add(new ConstructionRecord { Date = date, Content = content });
        }
        // 見積もりデータをDataGridViewに表示するためのデータを取得する
        public DataTable GetQuoteData()
        {
            // DataGridViewに表示するための空のテーブルを作成する
            DataTable table = new DataTable();
            table.Columns.Add("No", typeof(int));
            table.Columns.Add("商品名", typeof(string));
            table.Columns.Add("単価", typeof(int));
            table.Columns.Add("数量", typeof(int));
            table.Columns.Add("見積金額", typeof(int));

            // Estimatesリストからデータを取り出し、テーブルに追加する
            int i = 1;
            foreach (EstimateItem item in EstimateItems)
            {
                table.Rows.Add(i, item.ItemName, item.UnitPrice, item.Amount, item.EstimatedAmount);
                i++;
            }

            return table;
        }
        public decimal GetTotalCost()
        {
            decimal total = 0;
            foreach (var item in EstimateItems)
            {
                total += item.GetTotalCost() * (decimal)GetWorkingDays() / (decimal)item.WorkingDays;
            }
            return total;
        }

        public int GetWorkingDays()
        {
            int days = 0;
            DateTime currentDay = StartDate;
            while (currentDay <= FinishDate)
            {
                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                currentDay = currentDay.AddDays(1);
            }
            return days;
        }

        public string GanttChart()
        {
            StringBuilder chart = new StringBuilder();

            int projectDuration = GetDuration();

            chart.Append("Name\tStart Date\tEnd Date\tDuration (days)\n");

            foreach (var item in EstimateItems)
            {
                int itemDuration = (int)Math.Ceiling((double)item.EstimatedAmount / (double)item.Amount);
                int itemStartDateOffset = GetWorkingDays(item.StartDate, StartDate);

                chart.AppendFormat("{0}\t{1}\t{2}\t{3}\n", item.Name, item.StartDate.ToShortDateString(),
                                     item.StartDate.AddDays(itemDuration).ToShortDateString(), itemDuration);

                if (item.SubItems != null && item.SubItems.Any())
                {
                    foreach (var subItem in item.SubItems)
                    {
                        int subItemDuration = (int)Math.Ceiling((double)subItem.EstimatedAmount / (double)subItem.Amount);
                        int subItemStartDateOffset = GetWorkingDays(subItem.StartDate, StartDate);

                        chart.AppendFormat("- {0}\t{1}\t{2}\t{3}\n", subItem.Name,
                                             subItem.StartDate.ToShortDateString(),
                                             subItem.StartDate.AddDays(subItemDuration).ToShortDateString(), subItemDuration);
                    }
                }
            }

            return chart.ToString();
        }

        private int GetDuration()
        {
            return GetWorkingDays(StartDate, FinishDate);
        }

        private int GetWorkingDays(DateTime startDate, DateTime finishDate)
        {
            int days = 0;
            DateTime currentDay = startDate;
            while (currentDay <= finishDate)
            {
                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
                currentDay = currentDay.AddDays(1);
            }
            return days;
        }
    }
    public class SaveToDatabase
    {
            private string connectionString;

            public SaveToDatabase(string path)
            {
                connectionString = $"Data Source={path}";
            }

            public void SaveProject(Project project)
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Projectテーブルにデータを挿入する
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;

                                command.CommandText = @"
                                INSERT INTO Project (ProjectName, CreatedAt, ValidUntil, CustomerName, CustomerAddress, BuildingAddress, FloorPlan, TotalFloorArea, BuildingYear, Structure)
                                VALUES (@ProjectName, @CreatedAt, @ValidUntil, @CustomerName, @CustomerAddress, @BuildingAddress, @FloorPlan, @TotalFloorArea, @BuildingYear, @Structure);
                            ";

                                command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                                command.Parameters.AddWithValue("@CreatedAt", project.StartDate);
                                command.Parameters.AddWithValue("@ValidUntil", project.FinishDate);
                                command.Parameters.AddWithValue("@CustomerName", project.Customer.Name);
                                command.Parameters.AddWithValue("@CustomerAddress", project.Customer.Address);
                                command.Parameters.AddWithValue("@BuildingAddress", project.Building.Address);
                                command.Parameters.AddWithValue("@FloorPlan", project.Building.FloorPlan);
                                command.Parameters.AddWithValue("@TotalFloorArea", project.Building.TotalFloorArea);
                                command.Parameters.AddWithValue("@BuildingYear", project.Building.BuildingYear);
                                command.Parameters.AddWithValue("@Structure", project.Building.Structure);

                                command.ExecuteNonQuery();
                            }

                            // 2. Itemsテーブルにデータを挿入する
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;

                                command.CommandText = @"
                                INSERT INTO Items (ItemName, Amount, Unit, UnitPrice, Remarks, SubcontractorAmount)
                                VALUES (@ItemName, @Amount, @Unit, @UnitPrice,@Remarks, @SubcontractorAmount);
                            ";

                                foreach (var item in project.EstimateItems)
                                {
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@Name", item.ItemName);
                                    command.Parameters.AddWithValue("@Price", item.Amount);
                                    command.Parameters.AddWithValue("@Quantity", item.Unit);
                                    command.Parameters.AddWithValue("@Quantity", item.UnitPrice);
                                    command.Parameters.AddWithValue("@Quantity", item.Remarks);
                                    command.Parameters.AddWithValue("@Quantity", item.SubcontractorAmount);

                                command.ExecuteNonQuery();
                                }
                            }

                            // 3. Recordsテーブルにデータを挿入する
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;

                                command.CommandText = @"
                                INSERT INTO Records (ProjectName, EstimateItems, EstimateCreationDate)
                                VALUES (@ProjectName, @EstimateItems, @EstimateCreationDate);
                            ";

                                foreach (var record in project.Building.Projects)
                                {
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                                    command.Parameters.AddWithValue("@EstimateItems", record.EstimateItems);
                                    command.Parameters.AddWithValue("@EstimateCreationDate", record.EstimateCreationDate);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }
}

