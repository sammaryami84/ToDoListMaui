using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using MainToDoList.Models;

namespace MainToDoList.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitializeAsync()
        {
            await _db.ExecuteAsync("PRAGMA foreign_keys = ON;");
            await _db.CreateTableAsync<ItemModel>();
            await _db.CreateTableAsync<SubTaskModel>();
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IDX_SubTasks_ItemId ON SubTasks(ItemId);");
        }

        // ---------- Items ----------

        public Task<List<ItemModel>> GetItemsAsync()
            => _db.Table<ItemModel>().ToListAsync();

        public async Task<List<ItemModel>> GetItemsWithSubTasksAsync()
        {
            var items = await _db.Table<ItemModel>().ToListAsync();
            if (items.Count == 0) return items;

            var ids = items.Select(i => i.Id).ToList();
            var placeholders = string.Join(",", ids.Select(_ => "?"));

            var subtasks = await _db.QueryAsync<SubTaskModel>(
                $"SELECT * FROM SubTasks WHERE ItemId IN ({placeholders}) ORDER BY ItemId, [Order];",
                ids.Cast<object>().ToArray()
            );

            var lookup = subtasks
                .GroupBy(s => s.ItemId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in items)
            {
                var list = lookup.TryGetValue(item.Id, out var l) ? l : new List<SubTaskModel>();
                item.SubTasks = new ObservableCollection<SubTaskModel>(list);
            }

            return items;
        }

        public Task<int> InsertItemAsync(ItemModel item)
            => _db.InsertAsync(item);

        public Task<int> UpdateItemAsync(ItemModel item)
            => _db.UpdateAsync(item);

        public Task<int> DeleteItemAsync(ItemModel item)
            => _db.DeleteAsync(item);

        public async Task<int> DeleteItemWithSubTasksAsync(ItemModel item)
        {
            int affected = 0;
            await _db.RunInTransactionAsync(conn =>
            {
                affected += conn.Execute("DELETE FROM SubTasks WHERE ItemId = ?", item.Id);
                affected += conn.Delete(item);
            });
            return affected;
        }

        public async Task SaveItemWithSubTasksAsync(ItemModel item, IEnumerable<SubTaskModel> subTasks)
        {
            await _db.RunInTransactionAsync(conn =>
            {
                conn.Insert(item);
                foreach (var sub in subTasks)
                {
                    sub.ItemId = item.Id;
                    conn.Insert(sub);
                }
            });
        }

        // ---------- SubTasks ----------

        public Task<List<SubTaskModel>> GetSubTasksByItemIdAsync(int itemId)
            => _db.Table<SubTaskModel>()
                  .Where(s => s.ItemId == itemId)
                  .OrderBy(s => s.Order)
                  .ToListAsync();

        public Task<int> InsertSubTaskAsync(SubTaskModel subTask)
            => _db.InsertAsync(subTask);

        public Task<int> UpdateSubTaskAsync(SubTaskModel subTask)
            => _db.UpdateAsync(subTask);

        public Task<int> DeleteSubTaskAsync(SubTaskModel subTask)
            => _db.DeleteAsync(subTask);

        public Task<int> DeleteSubTasksByItemIdAsync(int itemId)
            => _db.ExecuteAsync("DELETE FROM SubTasks WHERE ItemId = ?", itemId);

        public async Task UpdateSubTaskOrdersAsync(IList<SubTaskModel> subTasks)
        {
            await _db.RunInTransactionAsync(conn =>
            {
                for (int i = 0; i < subTasks.Count; i++)
                {
                    subTasks[i].Order = i;
                    conn.Update(subTasks[i]);
                }
            });
        }
    }
}