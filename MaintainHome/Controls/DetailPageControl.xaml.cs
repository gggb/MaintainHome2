using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class DetailPageControl : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailPageControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<TaskActivity>), typeof(DetailPageControl), new ObservableCollection<TaskActivity>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(TaskActivity), typeof(DetailPageControl), default(TaskActivity));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public ObservableCollection<TaskActivity> ItemsSource
        {
            get => (ObservableCollection<TaskActivity>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public TaskActivity SelectedItem
        {
            get => (TaskActivity)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DetailPageControl()
        {
            InitializeComponent();
        }

        public async Task LoadTaskActivities(int taskId)
        {
            var repository = new TaskActivityRepository();
            var taskActivities = await repository.GetTaskActivitiesAsyncByTaskId(taskId);

            // Log the task ID and number of activities fetched
            System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Activities: {taskActivities.Count}");
            foreach (var activity in taskActivities)
            {
                System.Diagnostics.Debug.WriteLine($"Activity ID: {activity.Id}, Status: {activity.Status}, Action: {activity.Action}"); 
            }
            ItemsSource = new ObservableCollection<TaskActivity>(taskActivities);
        }

        private void OnNewButtonClicked(object sender, EventArgs e) 
        { 
            SelectedItem = new TaskActivity(); 
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e) 
        { 
            if (SelectedItem != null) 
            { 
                var repository = new TaskActivityRepository(); 
                if (SelectedItem.Id == 0) 
                { 
                    await repository.AddTaskActivity(SelectedItem); 
                    ItemsSource.Add(SelectedItem); 
                } 
                else 
                { await repository.UpdateTaskActivityAsync(SelectedItem); 
                } 
            } 
        }
        private async void OnDeleteButtonClicked(object sender, EventArgs e) 
        { 
            if (SelectedItem != null) 
            { 
                var repository = new TaskActivityRepository(); 
                await repository.DeleteTaskActivityAsync(SelectedItem.Id); 
                ItemsSource.Remove(SelectedItem); SelectedItem = null; 
            } 
        }
    }
}
