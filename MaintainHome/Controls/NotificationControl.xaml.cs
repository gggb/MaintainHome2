//using Android.App;
using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class NotificationControl : ContentView, IListEditBaseControl
    {
        public int TaskId { get; set; }

        public ObservableCollection<string> StatusOptions { get; set; } =
            new ObservableCollection<string> { "Completed", "Pending", "On-Hold", "Canceled" };

        public ObservableCollection<string> ConditionOptions { get; set; } =
            new ObservableCollection<string> { "Excellent", "Good", "Fair", "Poor" };

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(NotificationControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<Notification>), typeof(NotificationControl), new ObservableCollection<Notification>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(Notification), typeof(NotificationControl), default(Notification));

        public static readonly BindableProperty ClassLabelTextProperty =
            BindableProperty.Create(nameof(ClassLabelText), typeof(string), typeof(NotificationControl), "Notifications (0)");

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Heading section change when adding new task activity
        // Bindable property for CollectionView SelectionMode
        public static readonly BindableProperty CollectionSelectionModeProperty =
            BindableProperty.Create(nameof(CollectionSelectionMode), typeof(SelectionMode), typeof(NotificationControl), SelectionMode.Single);

        // Data entry form title-- "SectionTitle" (assigned in the xaml file.
        public static readonly BindableProperty SectionTitleProperty =
            BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(NotificationControl), "View/Edit Notification");

        // IsAddVisible, IsDeleteVisible, IsCancelVisible are references to button, alowing buttons' visibility to be manipulated programatically.
        public static readonly BindableProperty IsAddVisibleProperty =
            BindableProperty.Create(nameof(IsAddVisible), typeof(bool), typeof(NotificationControl), true);

        public static readonly BindableProperty IsDeleteVisibleProperty =
            BindableProperty.Create(nameof(IsDeleteVisible), typeof(bool), typeof(NotificationControl), true);

        public static readonly BindableProperty IsCancelVisibleProperty =
          BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(NotificationControl), false);
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

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public ObservableCollection<Notification> ItemsSource
        {
            get => (ObservableCollection<Notification>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public Notification SelectedItem
        {
            get => (Notification)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public NotificationControl()
        {
            InitializeComponent();
            BindingContext = this;
        }
        public async Task LoadData(int taskId)
        {
            try
            {
                TaskId = taskId; // Store the TaskId

                var repository = new NotificationRepository();
                var notifications = await repository.GetNotificationsAsyncByTaskId(taskId);

                // Log the task ID and number of notifications fetched
                System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Notifications: {notifications.Count}");
                foreach (var notification in notifications)
                {
                    System.Diagnostics.Debug.WriteLine($"Notify ID: {notification.TaskId}, Name: {notification.TargetName}, Email: {notification.TargetEmail}");
                }
                ItemsSource = new ObservableCollection<Notification>(notifications);

                // Select the first item if the list has items
                if (ItemsSource.Count > 0) { SelectedItem = ItemsSource[0]; }

                // Update the ClassLabelText property
                ClassLabelText = $"Notifications ({notifications.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading notifications: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading notifications.", "OK");
            }
        }

        public async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementation here if needed
        }
        public void OnNewButtonClicked(object sender, EventArgs e)
        {
            try
            {
                SelectedItem = new Notification();
                SectionTitle = "Add a New Notification";
                IsAddVisible = false; IsDeleteVisible = false;
                IsCancelVisible = true;

                // Disable selection when adding a new item
                CollectionSelectionMode = SelectionMode.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating new notification: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while creating a new notification.", "OK");
                });
            }
        }

        public void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnSaveButtonClickedAsync(sender, e).ConfigureAwait(false);
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
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to save the notification data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                var repository = new NotificationRepository();
                if (SelectedItem != null)
                {
                    if (SelectedItem.NotificationId == 0) // New notification
                    {
                        SelectedItem.TaskId = TaskId; // Assign the stored TaskId
                        await repository.AddNotificationAsync(SelectedItem);
                        ItemsSource.Add(SelectedItem);
                    }
                    else // Modified notification
                    {
                        await repository.UpdateNotificationAsync(SelectedItem);

                        // Refresh the collection
                        var updatedNotification = SelectedItem;
                        ItemsSource.Remove(SelectedItem);
                        ItemsSource.Add(updatedNotification);
                    }

                    // Refresh the entire notification list
                    await LoadData(TaskId);

                    // Call the OnCancelButtonClicked method
                    OnCancelButtonClicked(sender, e);

                    // Select the first item if the list has items
                    if (ItemsSource.Count > 0) { SelectedItem = ItemsSource[0]; }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving notification: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving the notification.", "OK");
            }
        }


        public void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnDeleteButtonClickedAsync(sender, e).ConfigureAwait(false);
        }
        public async Task OnDeleteButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItem == null)
                {
                    await DisplayAlert("Delete Error", "There is no notification data to Delete", "OK");
                    return;
                }

                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to DELETE the notification data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)
                {
                    // Delete Notification record
                    var repository = new NotificationRepository();
                    await repository.DeleteNotificationAsync(SelectedItem.NotificationId);
                    await parentPage.DisplayAlert("Confirm Update", "The Notification record has been deleted.", "OK");

                    // Remove the selected item and clear the selection
                    ItemsSource.Remove(SelectedItem); 
                    SelectedItem = null;

                    // Refresh the entire notification list
                    await LoadData(TaskId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting notification: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the notification.", "OK");
            }
        }

        public async void OnCancelButtonClicked(object sender, EventArgs e)
        {
            // Call the async method and handle it properly
            OnCancelButtonClickedAsync(sender, e).ConfigureAwait(false);
        }
        public async Task OnCancelButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                SelectedItem = null;
                SectionTitle = "View/Edit Notification";
                IsAddVisible = true; IsDeleteVisible = true;
                IsCancelVisible = false;

                // Re-enable selection when canceling add
                CollectionSelectionMode = SelectionMode.Single;

                // Refresh the entire notification list
                await LoadData(TaskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error canceling notification: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while canceling the notification action.", "OK");
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

        public async Task<bool> ValidateForm()
        {
            if (SelectedItem == null)
            {
                await DisplayAlert("Validation Error", "No notification selected.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(SelectedItem.TargetName) ||
                string.IsNullOrWhiteSpace(SelectedItem.TargetEmail) ||
                string.IsNullOrWhiteSpace(SelectedItem.TargetPhone) ||
                string.IsNullOrWhiteSpace(SelectedItem.Message))
            {
                await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
                return false;
            }

            // Uncomment if needed for additional validation
            // if (!decimal.TryParse(SelectedItem.TimeSpent.ToString(), out decimal timeSpent))
            // {
            //     await DisplayAlert("Validation Error", "Please enter a valid number for time spent.", "OK");
            //     return false;
            // }

            return true;
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



