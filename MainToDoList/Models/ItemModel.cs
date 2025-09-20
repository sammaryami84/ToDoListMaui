using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;

namespace MainToDoList.Models
{
    [Table("Items")]
    public class ItemModel : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private DateTime _taskDate;
        private long _taskTimeTicks;
        private bool _isCompleted;
        private string _category;

        private string _categoryIconText;
        [Ignore]
        public string CategoryIconText
        {
            get => _categoryIconText;
            set => SetField(ref _categoryIconText, value);
        }
        public long ScheduledAtTicks { get; set; }

        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public DateTime TaskDate
        {
            get => _taskDate;
            set => SetField(ref _taskDate, value);
        }

        public long TaskTimeTicks
        {
            get => _taskTimeTicks;
            set
            {
                if (SetField(ref _taskTimeTicks, value))
                {
                    OnPropertyChanged(nameof(TaskTime));
                }
            }
        }
        [Ignore]
        public DateTime ScheduledAt
        {
            get
            {
                try
                {
                    return ScheduledAtTicks > 0
                        ? new DateTime(ScheduledAtTicks)
                        : DateTime.MinValue;
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
            set => ScheduledAtTicks = value.Ticks;
        }

        [Ignore]
        public TimeSpan TaskTime
        {
            get => TimeSpan.FromTicks(TaskTimeTicks);
            set
            {
                if (TaskTimeTicks != value.Ticks)
                {
                    TaskTimeTicks = value.Ticks;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetField(ref _isCompleted, value);
        }

        public string Category
        {
            get => _category;
            set => SetField(ref _category, value);
        }

        [Ignore]
        public ObservableCollection<SubTaskModel> SubTasks { get; set; } = new();

        

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    [Table("SubTasks")]
    public class SubTaskModel : INotifyPropertyChanged
    {
        private int _id;
        private int _itemId;
        private string _name;
        private bool _isCompleted;
        private int _order;

        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [Indexed]
        public int ItemId
        {
            get => _itemId;
            set => SetField(ref _itemId, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetField(ref _isCompleted, value);
        }

        public int Order
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // مدل دسته‌بندی برای کار با ColorfulPicker
    public class CategoryModel
    {
        public string Name { get; set; }
    }
}