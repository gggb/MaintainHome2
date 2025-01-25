//using Android.Views;
//using Android.Widget;
using MaintainHome.Database;
using MaintainHome.Models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;


namespace MaintainHome.Controls
{
    public partial class PartControl : ContentView, IListEditBaseControl
    {
        public int TaskId { get; set; }

        public ObservableCollection<string> TypeOptions { get; set; } =
            new ObservableCollection<string> { "Video", "Pictorial", "Textual", "Other" };

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(PartControl), default(string));

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<PartInfo>), typeof(PartControl), new ObservableCollection<PartInfo>());

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(PartInfo), typeof(PartControl), default(PartInfo));

        public static readonly BindableProperty ClassLabelTextProperty =
            BindableProperty.Create(nameof(ClassLabelText), typeof(string), typeof(PartControl), "Parts (0)");
        //++++++++++++++++++++++++++++++++++++++
        // Heading section change when adding new task Parts
        // Bindable property for CollectionView SelectionMode
        public static readonly BindableProperty CollectionSelectionModeProperty =
            BindableProperty.Create(nameof(CollectionSelectionMode), typeof(SelectionMode), typeof(PartControl), SelectionMode.Single);


        public static readonly BindableProperty SectionTitleProperty =
            BindableProperty.Create(nameof(SectionTitle), typeof(string), typeof(PartControl), "View/Edit Task Parts");

        public static readonly BindableProperty IsAddVisibleProperty =
            BindableProperty.Create(nameof(IsAddVisible), typeof(bool), typeof(PartControl), true);

        public static readonly BindableProperty IsDeleteVisibleProperty =
            BindableProperty.Create(nameof(IsDeleteVisible), typeof(bool), typeof(PartControl), true);

        public static readonly BindableProperty IsCancelVisibleProperty =
          BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(PartControl), false);

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
        public ObservableCollection<PartInfo> ItemsSource
        {
            get => (ObservableCollection<PartInfo>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public PartInfo SelectedItem
        {
            get => (PartInfo)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public PartControl()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public async Task LoadData(int taskId)
        {
            try
            {
                TaskId = taskId; // Store the TaskId

                var repository = new PartInfoRepository();
                var taskParts = await repository.GetAllPartsInfoAsyncByTaskId(taskId);

                // Log the task ID and number of activities fetched
                System.Diagnostics.Debug.WriteLine($"Task ID: {taskId}, Fetched Parts: {taskParts.Count}");
                foreach (var PartInfo in taskParts)
                {
                    System.Diagnostics.Debug.WriteLine($"Task Part ID: {PartInfo.TaskId}, Type: {PartInfo.Name}, PartContent: {PartInfo.Description}");
                }
                ItemsSource = new ObservableCollection<PartInfo>(taskParts);

                if (ItemsSource.Count > 0)
                {
                    SelectedItem = ItemsSource[0];
                    //SelectedItem.Type = PartInfos[0].Type;
                }

                // Update the ClassLabelText property
                ClassLabelText = $"Parts ({taskParts.Count})";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading task Parts: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task parts.", "OK");
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
                SelectedItem = new PartInfo{TaskId = TaskId};
                SectionTitle = "Add a New Task Part";
                IsAddVisible = false; IsDeleteVisible = false;
                IsCancelVisible = true;

                // Disable selection when adding a new item
                CollectionSelectionMode = SelectionMode.None;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating new task Part: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while creating a new task part.", "OK");
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

                // Confirm the user wants to update the task Parts
                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to save the task part data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)  // SelectedItem should never be null
                {
                    var repository = new PartInfoRepository();
                    if (SelectedItem.PartInfoId == 0) // New task Part - Add
                    {
                        await repository.AddPartsInfoAsync(SelectedItem);
                        ItemsSource.Add(SelectedItem);
                    }
                    else // Modified task Part - Update
                    {
                        await repository.UpdatePartsInfoAsync(SelectedItem);
                    }

                    // Refresh the entire task Part list
                    await LoadData(TaskId);

                    // Call the OnCancelButtonClicked method
                    OnCancelButtonClicked(sender, e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving task Part: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while saving the task part.", "OK");
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
                    await DisplayAlert("Delete Error", "There is no part to Delete", "OK");
                    return;
                }

                var parentPage = GetParentPage();
                if (parentPage != null)
                {
                    bool confirm = await parentPage.DisplayAlert("Confirm Update", "Are you sure you want to DELETE the task part data?", "Yes", "No");
                    if (!confirm)
                    {
                        return;
                    }
                }

                if (SelectedItem != null)
                {
                    var repository = new PartInfoRepository();

                    
                    await repository.DeletePartsInfoAsync(SelectedItem.PartInfoId);
                    ItemsSource.Remove(SelectedItem);
                    await parentPage.DisplayAlert("Confirm Update", "The task part record has been deleted.", "OK");

                    // Clear the binding context or set it to a new part
                    NameEntry.Text = string.Empty; 
                    PriceEntry.Text = string.Empty; 
                    SourceEntry.Text = string.Empty; 
                    DescriptionEditor.Text = string.Empty;

                    SelectedItem = null;
                    BindingContext = new PartInfo();

                    // Refresh the entire task part list
                    await LoadData(TaskId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting task part: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while deleting the task part.", "OK");
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
                SectionTitle = "Add a New Task Part";
                IsAddVisible = true; IsDeleteVisible = true;
                IsCancelVisible = false;

                // Re-enable selection when canceling add
                CollectionSelectionMode = SelectionMode.Single;

                // Refresh the entire task part list
                await LoadData(TaskId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error canceling task part: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while canceling the task Part.", "OK");
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
            if (string.IsNullOrWhiteSpace(SelectedItem.Name) ||
                string.IsNullOrWhiteSpace(SelectedItem.Description) ||
                string.IsNullOrWhiteSpace(SelectedItem.Source) ||
                SelectedItem.Price <= 0)
            {
                await DisplayAlert("Validation Error", "Please fill in all required fields and ensure that Price is a valid number.", "OK");
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