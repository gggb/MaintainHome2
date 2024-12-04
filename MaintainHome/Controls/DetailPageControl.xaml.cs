using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class DetailPageControl : ContentView
    {
        public int TaskId { get; set; }

        public ObservableCollection<string> StatusOptions { get; set; } = 
            new ObservableCollection<string> { "Completed", "Pending", "On-Hold", "Canceled" }; 
        
        public ObservableCollection<string> ConditionOptions { get; set; } = 
            new ObservableCollection<string> { "Excellent", "Good", "Fair", "Poor" };

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailPageControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<TaskActivity>), typeof(DetailPageControl), new ObservableCollection<TaskActivity>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(TaskActivity), typeof(DetailPageControl), default(TaskActivity));

        public static readonly BindableProperty TaskActivityLabelTextProperty =
            BindableProperty.Create(nameof(TaskActivityLabelText), typeof(string), typeof(DetailPageControl), "Task Activities (0)");
        //++++++++++++++++++++++++++++++++++++++
        // Heading section change when adding new task activity
        // Bindable property for CollectionView SelectionMode
        public static readonly BindableProperty CollectionSelectionModeProperty = 
            BindableProperty.Create(nameof(CollectionSelectionMode), typeof(SelectionMode), typeof(DetailPageControl), SelectionMode.Single); 


        public static readonly BindableProperty SectionTitleProperty =
            BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(DetailPageControl), "View/Edit Task Activity");

        public static readonly BindableProperty IsAddVisibleProperty =
            BindableProperty.Create(nameof(IsAddVisible), typeof(bool), typeof(DetailPageControl), true);

        public static readonly BindableProperty IsDeleteVisibleProperty =
            BindableProperty.Create(nameof(IsDeleteVisible), typeof(bool), typeof(DetailPageControl), true);

        public static readonly BindableProperty IsCancelVisibleProperty =
          BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(DetailPageControl), false);

        public string SectionTitle
        {
            get => (string)GetValue(SectionTitleProperty);
            set => SetValue(SectionTitleProperty, value);
        }

        public bool IsAddVisible
        {
            get => (bool)GetValue(IsAddVisibleProperty);
            set => SetValue(IsAddVisibleProperty, value);
        }

        public bool IsDeleteVisible
        {
            get => (bool)GetValue(IsDeleteVisibleProperty);
            set => SetValue(IsDeleteVisibleProperty, value);
        }

        public bool IsCancelVisible
        {
            get => (bool)GetValue(IsCancelVisibleProperty);
            set => SetValue(IsCancelVisibleProperty, value);
        }

        public SelectionMode CollectionSelectionMode 
        { 
            get => (SelectionMode)GetValue(CollectionSelectionModeProperty);
            set => SetValue(CollectionSelectionModeProperty, value); 
        }

        public string TaskActivityLabelText
        {
            get => (string)GetValue(TaskActivityLabelTextProperty);
            set => SetValue(TaskActivityLabelTextProperty, value);
        }
        //++++++++++++++++++++++++++++++++
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
            BindingContext = this;
        }

        public async Task LoadTaskActivities(int taskId)
        {
            TaskId = taskId; // Store the TaskId

            var repository = new TaskActivityRepository();
            var taskActivities = await repository.GetTaskActivitiesAsyncByTaskId(taskId);

            // Log the task ID and number of activities fetched
            System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Activities: {taskActivities.Count}");
            foreach (var activity in taskActivities)
            {
                System.Diagnostics.Debug.WriteLine($"Activity ID: {activity.Id}, Status: {activity.Status}, Action: {activity.Action}");
            }
            ItemsSource = new ObservableCollection<TaskActivity>(taskActivities);

            if (ItemsSource.Count > 0) 
            { 
                SelectedItem = ItemsSource[0];
                SelectedItem.Status = taskActivities[0].Status;
                
            }

            // Update the TaskActivityLabelText property
            TaskActivityLabelText = $"Task Activities ({taskActivities.Count})";
        }
        private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (!await ValidateTaskActivityForm())
            {
                return; // Stop if validation fails
            }

            // Confirm the user wants to update the task activity
                var parentPage = GetParentPage(); 
            if (parentPage != null) 
            { 
                bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to save the Task Activity data?", "Yes", "No"); 
                if (!confirm) 
                { 
                    return; 
                } 
            }

            if (SelectedItem != null)                                          // SelectedItem should never be null, right. In fact
                                                                               // this method should start by 
            {
                var repository = new TaskActivityRepository();
                if (SelectedItem.Id == 0)                                      // New task activity-- Add
                {
                    await repository.AddTaskActivity(SelectedItem);       
                    ItemsSource.Add(SelectedItem);
                }
                else
                {
                    await repository.UpdateTaskActivityAsync(SelectedItem);   //Modified task activity-- Update
                }

                // Refresh the entire notification list
                await LoadTaskActivities(TaskId);

                // Call the OnCancelButtonClicked method
                OnCancelButtonClicked(sender, e);
            }
        }
        private void OnNewButtonClicked(object sender, EventArgs e)
        {
            SelectedItem = new TaskActivity();
            SectionTitle = "Add a New Task Activity";
            IsAddVisible = false; IsDeleteVisible = false;
            IsCancelVisible = true;

            // Disable selection when adding a new item
            CollectionSelectionMode = SelectionMode.None;
        }
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var parentPage = GetParentPage();
            if (parentPage != null)
            {
                bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to DELETE the Task Activity data?", "Yes", "No");
                if (!confirm)
                {
                    return;
                }
            }

            if (SelectedItem != null)
            {
                var repository = new TaskActivityRepository();
                await repository.DeleteTaskActivityAsync(SelectedItem.Id);
                ItemsSource.Remove(SelectedItem); SelectedItem = null;
                await parentPage.DisplayAlert("Confirm Update", "The Task Activity record has been deleted.", "OK");

                ItemsSource.Remove(SelectedItem); SelectedItem = null;
                // Refresh the entire notification list
                await LoadTaskActivities(TaskId);
            }
        }
        private async void OnCancelButtonClicked(object sender, EventArgs e)
        {
            SelectedItem = null;
            SectionTitle = "View/Edit Task Activity";
            IsAddVisible = true; IsDeleteVisible = true;
            IsCancelVisible = false;

            // Re-enable selection when canceling add
            CollectionSelectionMode = SelectionMode.Single;

            // Refresh the entire notification list
            await LoadTaskActivities(TaskId);
        }
        private Page GetParentPage() 
        { 
            Element element = this; 
            while (element != null) 
            { 
                if (element is Page page) 
                { 
                    return page; 
                } 
                element = element.Parent; 
            } 
            return null; 
        }
        private async Task<bool> ValidateTaskActivityForm()
        {
            if (string.IsNullOrWhiteSpace(SelectedItem.Action) ||
                string.IsNullOrWhiteSpace(SelectedItem.Status) ||
                string.IsNullOrWhiteSpace(SelectedItem.Condition) ||
                string.IsNullOrWhiteSpace(SelectedItem.Notes))
            {
                await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
                return false;
            }

            if (!decimal.TryParse(SelectedItem.TimeSpent.ToString(), out decimal timeSpent))
            {
                await DisplayAlert("Validation Error", "Please enter a valid number for Time Spent.", "OK");
                return false;
            }
            return true;
        }

        private async Task DisplayAlert(string title, string message, string cancel)
        {
            var parentPage = GetParentPage();
            if (parentPage != null)
            {
                await parentPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}
