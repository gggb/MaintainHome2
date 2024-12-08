using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class DetailPageControl : ContentView, IListEditBaseControl
    {
        public int TaskId { get; set; }

        //========================
        // for Task Activity only
        public ObservableCollection<string> StatusOptions { get; set; } =
            new ObservableCollection<string> { "Completed", "Pending", "On-Hold", "Canceled" };

        public ObservableCollection<string> ConditionOptions { get; set; } =
            new ObservableCollection<string> { "Excellent", "Good", "Fair", "Poor" };
        //=======================

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(DetailPageControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<TaskActivity>), typeof(DetailPageControl), new ObservableCollection<TaskActivity>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(TaskActivity), typeof(DetailPageControl), default(TaskActivity));

        public static readonly BindableProperty ClassLabelTextProperty = 
            BindableProperty.Create(nameof(ClassLabelText), typeof(string), typeof(DetailPageControl), "Activities (0)");
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

        public string ClassLabelText
        {
            get => (string)GetValue(ClassLabelTextProperty);
            set => SetValue(ClassLabelTextProperty, value);
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

        public async Task LoadData(int taskId)
        {
            try
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
                ClassLabelText = $"Activities ({taskActivities.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading task activities: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task activities.", "OK");
            }
        }

        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)            // need to delete. Not needed.
        {
            // Placeholder for selection changed logic
        }

        public void OnNewButtonClicked(object sender, EventArgs e)                          // may want to consider changing to "Async Task" due to "await"
        {
            try
            {
                SelectedItem = new TaskActivity();
                SectionTitle = "Add a New Task Activity";
                IsAddVisible = false; IsDeleteVisible = false;
                IsCancelVisible = true;

                // Disable selection when adding a new item
                CollectionSelectionMode = SelectionMode.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating new task activity: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while creating a new task activity.", "OK");
                });
            }
        }

        public void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnSaveButtonClickedAsync(sender, e).ConfigureAwait(false);
        }

        public void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnDeleteButtonClickedAsync(sender, e).ConfigureAwait(false);
        }

        public async Task OnSaveButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (!await ValidateForm())
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

                if (SelectedItem != null)  // SelectedItem should never be null
                {
                    var repository = new TaskActivityRepository();
                    if (SelectedItem.Id == 0) // New task activity - Add
                    {
                        await repository.AddTaskActivity(SelectedItem);
                        ItemsSource.Add(SelectedItem);
                    }
                    else // Modified task activity - Update
                    {
                        await repository.UpdateTaskActivityAsync(SelectedItem);
                    }

                    // Refresh the entire task activity list
                    await LoadData(TaskId);

                    // Call the OnCancelButtonClicked method which will focus back on the list/edit.
                    await OnCancelButtonClickedAsync(sender, e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving task activity: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving the task activity.", "OK");
            }
        }


        public async Task OnDeleteButtonClickedAsync(object sender, EventArgs e)
        {
            try
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
                    ItemsSource.Remove(SelectedItem);
                    await parentPage.DisplayAlert("Confirm Update", "The Task Activity record has been deleted.", "OK");

                    // Refresh the entire task activity list
                    await LoadData(TaskId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting task activity: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the task activity.", "OK");
            }
        }

        public void OnCancelButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnCancelButtonClickedAsync(sender, e).ConfigureAwait(false);
        }

        public async Task OnCancelButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                SelectedItem = null;
                SectionTitle = "View/Edit Task Activity";
                IsAddVisible = true; IsDeleteVisible = true;
                IsCancelVisible = false;

                // Re-enable selection when canceling add
                CollectionSelectionMode = SelectionMode.Single;

                // Refresh the entire task activity list
                await LoadData(TaskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error canceling task activity: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while canceling the task activity.", "OK");
            }
        }

        public Page GetParentPage()
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

        //public async Task<bool> ValidateForm()
        //{
        //    if (string.IsNullOrWhiteSpace(SelectedItem.Action) ||
        //        string.IsNullOrWhiteSpace(SelectedItem.Status) ||
        //        string.IsNullOrWhiteSpace(SelectedItem.Condition) ||
        //        string.IsNullOrWhiteSpace(SelectedItem.Notes))
        //    {
        //        await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
        //        return false;
        //    }

        //    if (!decimal.TryParse(SelectedItem.TimeSpent.ToString(), out decimal timeSpent))
        //    {
        //        await DisplayAlert("Validation Error", "Please enter a valid number for Time Spent.", "OK");
        //        return false;
        //    }
        //    return true;
        //}

        public async Task<bool> ValidateForm()
        {
            try
            {
                // Perform your validation logic here
                // For example, check if all required fields are filled

                bool isValid = true; // Set this based on your validation logic

                // Example validation (replace with actual logic)
                if (string.IsNullOrEmpty(SelectedItem?.Action) ||
                    string.IsNullOrEmpty(SelectedItem?.Status) ||
                    string.IsNullOrEmpty(SelectedItem?.Condition) ||
                    string.IsNullOrEmpty(SelectedItem?.Notes))
                {
                    await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
                    isValid = false;
                }

                if (!decimal.TryParse(SelectedItem?.TimeSpent.ToString(), out decimal timeSpent))
                {
                    await DisplayAlert("Validation Error", "Please enter a valid number for Time Spent.", "OK");
                    isValid = false;
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error validating task form: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while validating the task form.", "OK");
                return false;
            }
        }


        public async Task DisplayAlert(string title, string message, string cancel)
        {
            var parentPage = GetParentPage();
            if (parentPage != null)
            {
                await parentPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}