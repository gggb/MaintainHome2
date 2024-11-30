using MaintainHome.Models;
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
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

        public ObservableCollection<Tasks> AllTasks { get; private set; } = new ObservableCollection<Tasks>();
        public ICommand OnTaskDoubleTapped { get; }

        public Dashboard()
        {
            InitializeComponent();
            BindingContext = this; // Set the BindingContext

            _tasksRepository = new TasksRepository();  // Initialize the tasks repository
            _categoryRepository = new CategoryRepository();  // Initialize the categories repository

            LoadData();  // Load tasks and categories from the database

            OnTaskDoubleTapped = new Command<Tasks>(OnTaskDoubleTappedHandler);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ApplyEmphasisToOverdueTasks();
        }

        private async void LoadData()
        {
            try
            {
                // Load tasks from the database where status is not "closed"
                var tasks = await _tasksRepository.GetAllOpenTasksAsync();
                _tasks = new ObservableCollection<Tasks>(tasks);

                // Load categories from the database
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                _categories = categories.ToDictionary(c => c.CategoryId, c => c.Title);

                // Process tasks for display
                FilterAndSortTasks();

                // Emphasize overdue tasks right after loading
                ApplyEmphasisToOverdueTasks();

                // Update traffic light icon after loading data
                UpdateTrafficLightIcon();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void FilterAndSortTasks()
        {
            var sortedTasks = _tasks.OrderBy(t => t.DueDate).ToList();

            AllTasks.Clear();
            foreach (var task in sortedTasks)
            {
                AllTasks.Add(task);
            }
        }

        private void ApplyEmphasisToOverdueTasks()
        {
            foreach (var task in AllTasks)
            {
                if (task.DueDate <= DateTime.Now)
                {
                    // Apply emphasis - replace exclamation mark with warning triangle
                    task.Title = $"⚠️ {task.Title}";
                    // Additional styling can be applied here if needed
                }
            }
        }

        private void UpdateTrafficLightIcon()
        {
            // Implementation of traffic light icon update
        }

        private async void OnAddTaskButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("New Task", "Are you sure you want to add a new task?", "Yes", "No");
            if (result)
            {
                // Deselect any selected item
                var collectionView = this.FindByName<CollectionView>("TaskCollectionView");
                if (collectionView.SelectedItem != null)
                {
                    collectionView.SelectedItem = null;
                }

                // Navigate to Task Detail page with a new empty task
                EditTaskDetail(new Tasks());
            }
        }


        private void EditTaskDetail(Tasks task)
        {
            // Navigate to Task Detail page with the given task
            Navigation.PushAsync(new TaskDetail(task));
        }

        private void OnTaskTapped(object sender, SelectionChangedEventArgs e)
        {
            // Commenting out the navigation logic
            /*
            var selectedTask = e.CurrentSelection.FirstOrDefault() as Tasks;
            if (selectedTask != null)
            {
                // Handle task selection - navigate to task detail page
                EditTaskDetail(selectedTask);
            }
            */
        }

        private void OnEditTaskSwiped(object sender, EventArgs e)
        {
            var swipeItem = sender as SwipeItem;
            var task = swipeItem?.BindingContext as Tasks;
            if (task != null)
            {
                EditTaskDetail(task);
            }
        }

        private void OnTaskDoubleTappedHandler(Tasks task)
        {
            if (task != null)
            {
                EditTaskDetail(task);
            }
        }
    }
}
