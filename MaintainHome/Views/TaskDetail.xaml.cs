using MaintainHome.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MaintainHome.Views
{
    public partial class TaskDetail : ContentPage
    {
        private Tasks _task;

        public ObservableCollection<string> StatusOptions { get; set; } =
            new ObservableCollection<string> { "Completed", "Pending", "On-Hold", "Canceled" };

        public ObservableCollection<string> PriorityOptions { get; set; } =
            new ObservableCollection<string> { "Low", "Medium", "Urgent", };

        public TaskDetail(Tasks task)
        {
            InitializeComponent();
            Debug.WriteLine("TaskDetail Page Initialized");

            try
            {
                // Initialize options
                PriorityOptions = new ObservableCollection<string>
                {
                    "Low", "Medium", "Urgent"
                };
                StatusOptions = new ObservableCollection<string>
                {
                    "Completed", "Pending", "On-Hold", "Canceled"
                };

                // Initialize task
                _task = task ?? new Tasks(); // Initialize a new Tasks object if null
                BindingContext = this;
                this.BindingContext = _task;
                Debug.WriteLine($"BindingContext set: {_task.Title}");

                bool taskZero = _task.Id == 0;
                if (_task == null || taskZero)  // Create blank detail form for new task to be created
                {
                    Debug.WriteLine("Creating new task form");
                    _task.DueDate = DateTime.Today; // Set the task's DueDate to today's date
                    DueDateDatePicker.Date = DateTime.Today; // Set the DatePicker's date to today's date
                }
                else  // Load _task fields into form text fields to be edited
                {
                    Debug.WriteLine("Loading existing task details");
                    DueDateDatePicker.Date = _task.DueDate ?? DateTime.Today; // Ensure the DatePicker shows the correct date from the task
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing TaskDetail: {ex.Message}");
                // Since we cannot use await here, we handle the exception differently
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "An error occurred while initializing the task details.", "OK");
                });
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("TaskDetail Page Appearing");

            try
            {
                // Load TaskActivityControl with the taskId
                await TaskActivityControl.LoadData(_task.Id);

                // Load NotificationControl with the taskId
                await NotificationControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await NoteControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await HelpControl.LoadData(_task.Id);

                // Load NoteControl with the taskId
                await PartControl.LoadData(_task.Id);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while loading task activities or notifications.", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("TaskDetail Page Disappearing");

            try
            {
                // Any additional cleanup code can go here
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnDisappearing: {ex.Message}");
                // No need for a DisplayAlert here, since this is less critical
            }
        }
    }
}
