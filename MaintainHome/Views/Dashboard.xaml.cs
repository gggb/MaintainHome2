using MaintainHome.Models;   
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace MaintainHome.Views
{
    public partial class Dashboard : ContentPage
    {
        private ObservableCollection<Tasks> _tasks;
        private Dictionary<int, string> _categories;
        private readonly TasksRepository _tasksRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly UserRepository _userRepository;
        private int? _selectedUserId;
        private bool _showUnscheduled = false; // Declare _showUnscheduled variable here

        public ObservableCollection<Tasks> AllTasks { get; private set; } = new ObservableCollection<Tasks>();
        public ObservableCollection<Tasks> FilteredTasks { get; private set; } = new ObservableCollection<Tasks>();
        public ObservableCollection<User> Users { get; private set; } = new ObservableCollection<User>();
        public ICommand OnTaskDoubleTapped { get; }

        public Dashboard()
        {
            InitializeComponent();
            BindingContext = this;

            // Check if the user is logged in
            if (App.CurrentUser == null) 
            { 
                MainThread.BeginInvokeOnMainThread(async () => 
                { 
                    await DisplayAlert("Alert", "You must login first", "OK"); 
                    Application.Current.MainPage = new Login(); 
                }); 
            }
            else
            {
                try
                {
                    // Clear existing collections
                    AllTasks.Clear(); 
                    FilteredTasks.Clear(); 
                    Users.Clear();

                    _tasksRepository = new TasksRepository();
                    _categoryRepository = new CategoryRepository();
                    _userRepository = new UserRepository();

                    //await LoadDataAsync(); // Load tasks and categories from the database
                    LoadDataAsync().ConfigureAwait(false);

                    OnTaskDoubleTapped = new Command<Tasks>(OnTaskDoubleTappedHandler);
                    UserIdPicker.SelectedIndexChanged += UserIdPicker_SelectedIndexChanged;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error initializing Dashboard: {ex.Message}");
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Error", "An error occurred while initializing the dashboard.", "OK");
                    });
                }
            }
        }
        public async Task LoadDataAsync()
        {
            try
            {
                var tasks = await _tasksRepository.GetTasksAsync();
                _tasks = new ObservableCollection<Tasks>(tasks);

                var categories = await _categoryRepository.GetAllCategoriesAsync();
                _categories = categories.ToDictionary(c => c.CategoryId, c => c.Title);

                var users = await _userRepository.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                var pickerItems = Users.Select(u => new UserIdPickerItem { UserId = u.UserId, UserName = u.UserName }).ToList();
                UserIdPicker.ItemsSource = pickerItems; // Populate picker
                UserIdPicker.ItemDisplayBinding = new Binding("UserName"); // Display UserName in picker

                CategoryPicker.ItemsSource = _categories.Values.ToList(); // Populate CategoryPicker with category titles

                FilterAndSortTasks();
                ApplyEmphasisToOverdueTasks();
                UpdateTrafficLightIcon();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while loading data.", "OK");
                });
            }
        }
        private void OnSearchIconClicked(object sender, EventArgs e)
        {
            // Toggle the visibility of the search box
            SearchBox.IsVisible = !SearchBox.IsVisible;
        }
        private void UserIdPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedItem = UserIdPicker.SelectedItem as UserIdPickerItem;
            _selectedUserId = selectedItem?.UserId;
            Debug.WriteLine($"Selected UserId: {_selectedUserId}");
        }
        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            // Get the search criteria
            var status = StatusPicker.SelectedItem?.ToString();
            var priority = PriorityPicker.SelectedItem?.ToString();

            var userId = UserIdPicker.SelectedItem != null && ((UserIdPickerItem)UserIdPicker.SelectedItem).UserName != "None" 
                ? ((UserIdPickerItem)UserIdPicker.SelectedItem).UserId 
                : (int?)null;

            var categoryTitle = CategoryPicker.SelectedItem?.ToString();
            var categoryId = categoryTitle != "None" && CategoryPicker.SelectedItem != null
                ? _categories.FirstOrDefault(x => x.Value == categoryTitle).Key
                : (int?)null;

            var dueDateStart = DueDateStartPicker.Date != DateTime.Today ? DueDateStartPicker.Date : (DateTime?)null;
            var dueDateEnd = DueDateEndPicker.Date != DateTime.Today ? DueDateEndPicker.Date : (DateTime?)null;

            // Prepare the search criteria message
            var criteriaMessage = $"Status: {status ?? "Any"}\n" +
                                  $"Priority: {priority ?? "Any"}\n" +
                                  $"User ID: {userId?.ToString() ?? "Any"}\n" + 
                                  $"Category ID: {categoryId?.ToString() ?? "Any"}\n" +
                                  $"Due Date Start: {dueDateStart?.ToString("MM/dd/yyyy") ?? "Any"}\n" +
                                  $"Due Date End: {dueDateEnd?.ToString("MM/dd/yyyy") ?? "Any"}";

            // Show the dialog box
            bool confirm = await DisplayAlert("Confirm Search Criteria", criteriaMessage, "Confirm", "Cancel");

            if (!confirm)
            {
                // User cancelled the search
                return;
            }

            // Add debugging output
            Debug.WriteLine($"Search Criteria - Status: {status}, Priority: {priority}, CategoryId: {categoryId}, DueDateStart: {dueDateStart}, DueDateEnd: {dueDateEnd}");

            // Perform the search
            var filteredTasks = await _tasksRepository.SearchTasksAsync(status, priority, _selectedUserId, categoryId, dueDateStart, dueDateEnd);

            // Update the AllTasks collection with the filtered tasks
            AllTasks.Clear();
            foreach (var task in filteredTasks)
            {
                AllTasks.Add(task);
            }

            // Add debugging output for the final task count
            Debug.WriteLine($"Filtered Tasks Count: {filteredTasks.Count}");

            // Update the traffic light icon after search
            UpdateTrafficLightIcon();
        }
        private void OnResetDatesButtonClicked(object sender, EventArgs e)
        {
            DueDateStartPicker.Date = DateTime.Today; // Use DateTime.MinValue as the "unset" indicator
            DueDateEndPicker.Date = DateTime.Today; // Use DateTime.MinValue as the "unset" indicator
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                LoadDataAsync().ConfigureAwait(false);
                //LoadData();
                //FilterAndSortTasks();
                //ApplyEmphasisToOverdueTasks();
                //UpdateTrafficLightIcon();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while updating overdue tasks.", "OK");
                });
            }
        }
        private void FilterAndSortTasks()
        {
            try
            {
                var sortedTasks = _tasks.OrderBy(t => t.DueDate).ToList();

                AllTasks.Clear();
                foreach (var task in sortedTasks)
                {
                    AllTasks.Add(task);
                }

                // Filter based on _showUnscheduled
                //var filteredTasks = _showUnscheduled ? sortedTasks : sortedTasks.Where(t => t.Status != "Unscheduled").ToList();
                var filteredTasks = sortedTasks;
                FilteredTasks.Clear();
                foreach (var task in filteredTasks)
                {
                    FilteredTasks.Add(task); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering and sorting tasks: {ex.Message}");
            }
        }
        private void ApplyEmphasisToOverdueTasks()
        {
            try
            {
                 foreach (var task in AllTasks)
                {
                    if (task.DueDate <= DateTime.Now && !task.Title.Contains("⚠️"))
                    {
                        task.Title = $"⚠️ {task.Title}";
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying emphasis to overdue tasks: {ex.Message}");
            }
        }
        private void UpdateTrafficLightIcon()
        {
            try
            {
                // Default to green
                string status = "green";
                DateTime now = DateTime.Now;
                DateTime threeDaysFromNow = now.AddDays(3);

                // Check for overdue tasks
                if (AllTasks.Any(t => t.DueDate <= now))
                {
                    status = "red"; // Set to red if any tasks are overdue
                }
                else if (AllTasks.Any(t => t.DueDate <= threeDaysFromNow))
                {
                    status = "yellow"; // Set to yellow if any tasks are due within the next three days
                }

                // Update the traffic light indicator
                UpdateTrafficLightIndicator(status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating traffic light icon: {ex.Message}");
            }
        }
        private void UpdateTrafficLightIndicator(string status) 
        {
            OverdueAlertExplanation.Text = "";
            RedLight.IsVisible = false; 
            YellowLight.IsVisible = false; 
            GreenLight.IsVisible = false; 
            switch (status) 
            { 
                case "red": 
                    RedLight.IsVisible = true;
                    OverdueAlertExplanation.Text = "Red light & ⚠️ indicates overdue tasks";
                    break; 
                case "yellow": 
                    YellowLight.IsVisible = true;
                    OverdueAlertExplanation.Text = "Yellow light indicates task(s) due within 3 days";
                    break; 
                case "green": 
                    GreenLight.IsVisible = true; 
                    break; 
            } 
        }
        private async void OnAddTaskButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await DisplayAlert("New Task", "Are you sure you want to add a new task?", "Yes", "No");
                if (result)
                {
                    var collectionView = this.FindByName<CollectionView>("TaskCollectionView");
                    if (collectionView.SelectedItem != null)
                    {
                        collectionView.SelectedItem = null;
                    }

                    EditTaskDetail(new Tasks());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding new task: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while adding a new task.", "OK");
            }
        }
        private void EditTaskDetail(Tasks task)
        {
            try
            {
                Debug.WriteLine("Navigating to TaskDetail page with task: " + task.Title);
                Navigation.PushAsync(new TaskDetail(task));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to TaskDetail: {ex.Message}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while navigating to the task details.", "OK");
                });
            }
        }
        private void OnEditTaskSwiped(object sender, EventArgs e)
        {
            try
            {
                var swipeItem = sender as SwipeItem;
                var task = swipeItem?.BindingContext as Tasks;
                if (task != null)
                {
                    EditTaskDetail(task);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling task swipe: {ex.Message}");
            }
        }
        private void OnTaskDoubleTappedHandler(Tasks task)
        {
            try
            {
                if (task != null)
                {
                    EditTaskDetail(task);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling task double tap: {ex.Message}");
            }
        }
        private async void OnToggleUnscheduledClicked(object sender, EventArgs e) 
        { 
            _showUnscheduled = !_showUnscheduled; 
            FilterAndSortTasks(); 
        }
    }
}
