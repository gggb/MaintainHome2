using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public partial class NoteControl : ContentView, IListEditBaseControl
    {
        public int TaskId { get; set; }

        public ObservableCollection<string> TypeOptions { get; set; } =
            new ObservableCollection<string> { "Complaint", "Improvement", "Observation", "Other" };

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(NoteControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<TaskNote>), typeof(NoteControl), new ObservableCollection<TaskNote>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(TaskNote), typeof(NoteControl), default(TaskNote));

        public static readonly BindableProperty ClassLabelTextProperty =
            BindableProperty.Create(nameof(ClassLabelText), typeof(string), typeof(NoteControl), "Task Notes (0)");
        //++++++++++++++++++++++++++++++++++++++
        // Heading section change when adding new task Notes
        // Bindable property for CollectionView SelectionMode
        public static readonly BindableProperty CollectionSelectionModeProperty =
            BindableProperty.Create(nameof(CollectionSelectionMode), typeof(SelectionMode), typeof(NoteControl), SelectionMode.Single);


        public static readonly BindableProperty SectionTitleProperty =
            BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(NoteControl), "View/Edit Task Notes");

        public static readonly BindableProperty IsAddVisibleProperty =
            BindableProperty.Create(nameof(IsAddVisible), typeof(bool), typeof(NoteControl), true);

        public static readonly BindableProperty IsDeleteVisibleProperty =
            BindableProperty.Create(nameof(IsDeleteVisible), typeof(bool), typeof(NoteControl), true);

        public static readonly BindableProperty IsCancelVisibleProperty =
          BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(NoteControl), false);

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
        public ObservableCollection<TaskNote> ItemsSource
        {
            get => (ObservableCollection<TaskNote>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public TaskNote SelectedItem
        {
            get => (TaskNote)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public NoteControl()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public async Task LoadData(int taskId)
        {
            try
            {
                TaskId = taskId; // Store the TaskId

                var repository = new TaskNoteRepository();
                var taskNotes = await repository.GetAllTaskNotesAsyncByTaskId(taskId);

                // Log the task ID and number of activities fetched
                System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Notes: {taskNotes.Count}");
                foreach (var taskNote in taskNotes)
                {
                    System.Diagnostics.Debug.WriteLine($"Task Note ID: {taskNote.TaskId}, Type: {taskNote.Type}, NoteContent: {taskNote.NoteContent}");
                }
                ItemsSource = new ObservableCollection<TaskNote>(taskNotes);

                if (ItemsSource.Count > 0)
                {
                    SelectedItem = ItemsSource[0];
                    //SelectedItem.Type = taskNotes[0].Type;
                }

                // Update the ClassLabelText property
                ClassLabelText = $"Notes ({taskNotes.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading task Notes: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task Notes.", "OK");
            }
        }

        public async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Placeholder for selection changed logic
        }
        public void OnNewButtonClicked(object sender, EventArgs e)
        {
            try
            {
                //SelectedItem = new TaskNote();
                SelectedItem = new TaskNote { TaskId = TaskId };
                SectionTitle = "Add a New Task Note";
                IsAddVisible = false; IsDeleteVisible = false;
                IsCancelVisible = true;

                // Disable selection when adding a new item
                CollectionSelectionMode = SelectionMode.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating new task note: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while creating a new task note.", "OK");
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

                // Confirm the user wants to update the task Notes
                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to save the Task note data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)  // SelectedItem should never be null
                {
                    var repository = new TaskNoteRepository();
                    if (SelectedItem.NoteId == 0) // New task note - Add
                    {
                        await repository.AddTaskNoteAsync(SelectedItem);
                        ItemsSource.Add(SelectedItem);
                    }
                    else // Modified task note - Update
                    {
                        await repository.UpdateTaskNoteAsync(SelectedItem);
                    }

                    // Refresh the entire task note list
                    await LoadData(TaskId);

                    // Call the OnCancelButtonClicked method
                    OnCancelButtonClicked(sender, e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving task note: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving the task note.", "OK");
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
                    await DisplayAlert("Delete Error", "There is no note to Delete", "OK");
                    return;
                }

                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to DELETE the Task note data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)
                {
                    var repository = new TaskNoteRepository();
                    await repository.DeleteTaskNoteAsync(SelectedItem.NoteId);
                    ItemsSource.Remove(SelectedItem);
                    await parentPage.DisplayAlert("Confirm Update", "The Task note record has been deleted.", "OK");

                    // Clear the binding context or set it to a new note ESPECIALLY when the last note is deleted.
                    NoteType.SelectedItem = null;
                    NoteDescrEdit.Text = string.Empty;
                    SelectedItem = null;
                    BindingContext = new TaskNote();

                    // Refresh the entire task note list
                    await LoadData(TaskId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting task note: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the task note.", "OK");
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
                SectionTitle = "View/Edit Task Note";
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
            if (string.IsNullOrWhiteSpace(SelectedItem.Type) || string.IsNullOrWhiteSpace(SelectedItem.NoteContent))
            {
                await DisplayAlert("Validation Error", "Please fill in all required fields.", "OK");
                return false;
            }

            //check for TimeStamp

            //if (!decimal.TryParse(SelectedItem.TimeSpent.ToString(), out decimal timeSpent))
            //{
            //    await DisplayAlert("Validation Error", "Please enter a valid number for Time Spent.", "OK");
            //    return false;
            //}
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