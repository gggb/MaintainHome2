//using Java.Nio.FileNio.Attributes;
using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class HelpControl : ContentView, IListEditBaseControl
    {
        public int TaskId { get; set; }

        public ObservableCollection<string> TypeOptions { get; set; } =
            new ObservableCollection<string> { "Video", "Pictorial", "Textual", "Other" };

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(HelpControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<TaskHelp>), typeof(HelpControl), new ObservableCollection<TaskHelp>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(TaskHelp), typeof(HelpControl), default(TaskHelp));

        public static readonly BindableProperty ClassLabelTextProperty =
            BindableProperty.Create(nameof(ClassLabelText), typeof(string), typeof(HelpControl), "Helps (0)");
        //++++++++++++++++++++++++++++++++++++++
        // Heading section change when adding new task Helps
        // Bindable property for CollectionView SelectionMode
        public static readonly BindableProperty CollectionSelectionModeProperty =
            BindableProperty.Create(nameof(CollectionSelectionMode), typeof(SelectionMode), typeof(HelpControl), SelectionMode.Single);


        public static readonly BindableProperty SectionTitleProperty =
            BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(HelpControl), "View/Edit Task Helps");

        public static readonly BindableProperty IsAddVisibleProperty =
            BindableProperty.Create(nameof(IsAddVisible), typeof(bool), typeof(HelpControl), true);

        public static readonly BindableProperty IsDeleteVisibleProperty =
            BindableProperty.Create(nameof(IsDeleteVisible), typeof(bool), typeof(HelpControl), true);

        public static readonly BindableProperty IsCancelVisibleProperty =
          BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(HelpControl), false);

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
        public ObservableCollection<TaskHelp> ItemsSource
        {
            get => (ObservableCollection<TaskHelp>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public TaskHelp SelectedItem
        {
            get => (TaskHelp)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public HelpControl()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public async Task LoadData(int taskId)
        {
            try
            {
                TaskId = taskId; // Store the TaskId

                var repository = new TaskHelpRepository();
                var taskHelps = await repository.GetAllTaskHelpsAsyncByTaskId(taskId);

                // Log the task ID and number of activities fetched
                System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Helps: {taskHelps.Count}");
                foreach (var TaskHelp in taskHelps)
                {
                    System.Diagnostics.Debug.WriteLine($"Task Help ID: {TaskHelp.TaskId}, Type: {TaskHelp.Type}, HelpContent: {TaskHelp.Description}");
                }
                ItemsSource = new ObservableCollection<TaskHelp>(taskHelps);

                if (ItemsSource.Count > 0)
                {
                    SelectedItem = ItemsSource[0];
                    //SelectedItem.Type = TaskHelps[0].Type;
                }

                // Update the ClassLabelText property
                ClassLabelText = $"Helps ({taskHelps.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading task Helps: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task Helps.", "OK");
            }
        }

        public async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Placeholder for selection changed logic
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

                // Confirm the user wants to update the task Helps
                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to save the Task Help data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)  // SelectedItem should never be null
                {
                    var repository = new TaskHelpRepository();
                    if (SelectedItem.TaskHelpsId == 0) // New task Help - Add
                    {
                        await repository.AddTaskHelpAsync(SelectedItem);
                        ItemsSource.Add(SelectedItem);
                    }
                    else // Modified task Help - Update
                    {
                        await repository.UpdateTaskHelpAsync(SelectedItem);
                    }

                    // Refresh the entire task Help list
                    await LoadData(TaskId);

                    // Call the OnCancelButtonClicked method
                    OnCancelButtonClicked(sender, e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving task Help: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving the task Help.", "OK");
            }
        }
        public void OnNewButtonClicked(object sender, EventArgs e)
        {
            try
            {
                SelectedItem = new TaskHelp { TaskId = TaskId }; 
                SectionTitle = "Add a New Task Help";
                IsAddVisible = false; IsDeleteVisible = false;
                IsCancelVisible = true;

                // Disable selection when adding a new item
                CollectionSelectionMode = SelectionMode.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating new task Help: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while creating a new task Help.", "OK");
                });
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
                    await DisplayAlert("Delete Error", "There is no help record to Delete", "OK");
                    return;
                }

                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to DELETE the Task Help data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)
                {
                    var repository = new TaskHelpRepository();
                    await repository.DeleteTaskHelpAsync(SelectedItem.TaskHelpsId);
                    ItemsSource.Remove(SelectedItem);
                    await parentPage.DisplayAlert("Confirm Update", "The Task Help record has been deleted.", "OK");

                    // Clear the binding context or set it to a new Help when the last help tip is deleted.
                    TypePicker.SelectedItem = null;
                    URLEntry.Text = string.Empty;
                    DescrEdit.Text = string.Empty; 

                    SelectedItem = null;
                    BindingContext = new TaskHelp();


                    // Refresh the entire task Help list
                    await LoadData(TaskId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting task Help: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the task Help.", "OK");
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
                SectionTitle = "View/Edit Task Help";
                IsAddVisible = true; IsDeleteVisible = true;
                IsCancelVisible = false;

                // Re-enable selection when canceling add
                CollectionSelectionMode = SelectionMode.Single;

                // Refresh the entire task note list
                await LoadData(TaskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error canceling task note: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while canceling the task note.", "OK");
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
            if (string.IsNullOrWhiteSpace(SelectedItem.Description) ||
                string.IsNullOrWhiteSpace(SelectedItem.URL))
            {
                await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
                return false;
            }
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