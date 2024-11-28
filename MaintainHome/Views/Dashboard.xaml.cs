using MaintainHome.Models;
using MaintainHome.Database;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaintainHome.Views
{
    public partial class Dashboard : ContentPage
    {
        private ObservableCollection<Tasks> _tasks;
        private Dictionary<int, string> _categories;
        private readonly TasksRepository _tasksRepository;
        private readonly CategoryRepository _categoryRepository;

        public Dashboard()
        {
            InitializeComponent();
            _tasksRepository = new TasksRepository();  // Initialize the tasks repository
            _categoryRepository = new CategoryRepository();  // Initialize the categories repository

            LoadData();  // Load tasks and categories from the database
            //UpdateTrafficLightIcon();
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

                FilterTasks();  // Process tasks for display

                // Update traffic light icon after loading data
                UpdateTrafficLightIcon();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }


        private void FilterTasks()
        {
            var overdueTasks = _tasks.Where(t => t.DueDate < DateTime.Now).ToList();
            var scheduledTasks = _tasks.Where(t => t.DueDate >= DateTime.Now)
                                       .GroupBy(t => t.CategoryId)
                                       .Select(g => new
                                       {
                                           Category = _categories.ContainsKey(g.Key) ? _categories[g.Key] : "Unknown",
                                           Tasks = g.ToList()
                                       })
                                       .ToList();

            overdueTasksListView.ItemsSource = overdueTasks;
            scheduledTasksListView.ItemsSource = scheduledTasks;
        }

        private void UpdateTrafficLightIcon()
        {
            var now = DateTime.Now;
            var overdueTasks = _tasks.Any(t => t.DueDate < now);
            var upcomingTasks = _tasks.Any(t => t.DueDate >= now && t.DueDate <= now.AddDays(5));

            if (overdueTasks)
            {
                trafficLightIcon.Source = "red_light.png";
            }
            else if (upcomingTasks)
            {
                trafficLightIcon.Source = "yellow_light.png";
            }
            else
            {
                trafficLightIcon.Source = "green_light.png";
            }
        }

        private void OnFilterButtonClicked(object sender, EventArgs e) 
        { 
            var button = sender as Button; 
            if (button != null) 
            { 
                string filter = button.Text; 
                
                switch (filter) 
                { 
                    case "Weekly": 
                        FilterTasksByCategory("Weekly"); 
                        break; 
                    case "Monthly": FilterTasksByCategory("Monthly"); 
                        break; 
                    case "Yearly": FilterTasksByCategory("Yearly"); 
                        break; 
                } 
            } 
        }

        private void FilterTasksByCategory(string category) 
        { 
            var filteredTasks = _tasks.Where(t => _categories[t.CategoryId] == category).ToList(); 
            scheduledTasksListView.ItemsSource = filteredTasks; 
        }

        private void OnTaskTapped(object sender, ItemTappedEventArgs e)
        {
            var task = e.Item as Tasks;
            if (task != null)
            {
                // Navigate to TaskDetail page with the selected task
                Navigation.PushAsync(new TaskDetail(task));
            }
            // Deselect the item
            ((ListView)sender).SelectedItem = null;
        }

        private void OnScheduleNewTaskButtonClicked(object sender, EventArgs e)
        {
            // Navigate to schedule new task
            Navigation.PushAsync(new TaskDetail(new Tasks()));
        }

        // Other methods...
    }
}