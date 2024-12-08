using MaintainHome.Models;
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MaintainHome.Views
{
    public partial class Dashboard : ContentPage
    {
        private ObservableCollection<Tasks> _tasks;
        private Dictionary<int, string> _categories;
        private readonly TasksRepository _tasksRepository;
        private readonly CategoryRepository _categoryRepository;

        public ObservableCollection<Tasks> AllTasks { get; private set; } = new ObservableCollection<Tasks>();
        public ICommand OnTaskDoubleTapped { get; }

        public Dashboard()
        {
            InitializeComponent();
            BindingContext = this;

            try
            {
                _tasksRepository = new TasksRepository();
                _categoryRepository = new CategoryRepository();

                LoadData(); // Load tasks and categories from the database

                OnTaskDoubleTapped = new Command<Tasks>(OnTaskDoubleTappedHandler);
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                ApplyEmphasisToOverdueTasks();
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


        private async void LoadData()
        {
            try
            {
                var tasks = await _tasksRepository.GetTasksAsync();
                _tasks = new ObservableCollection<Tasks>(tasks);

                var categories = await _categoryRepository.GetAllCategoriesAsync();
                _categories = categories.ToDictionary(c => c.CategoryId, c => c.Title);

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
                // Implementation of traffic light icon update
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating traffic light icon: {ex.Message}");
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


        private void OnTaskTapped(object sender, SelectionChangedEventArgs e)
        {
            // Commenting out the navigation logic
            /*
            try
            {
                var selectedTask = e.CurrentSelection.FirstOrDefault() as Tasks;
                if (selectedTask != null)
                {
                    EditTaskDetail(selectedTask);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling task tap: {ex.Message}");
            }
            */
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
    }
}
