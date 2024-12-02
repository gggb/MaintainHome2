using MaintainHome.Models;
using System.Diagnostics;

namespace MaintainHome.Views
{
    public partial class TaskDetail : ContentPage
    {
        private Tasks _task;

        public TaskDetail(Tasks task)
        {
            InitializeComponent();
            Debug.WriteLine("TaskDetail Page Initialized");

            _task = task ?? new Tasks(); // Initialize a new Tasks object if null
            BindingContext = _task;

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
                DueDateDatePicker.Date = _task.DueDate; // Ensure the DatePicker shows the correct date from the task
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("TaskDetail Page Appearing");

            // Load TaskActivityControl with the taskId
            await TaskActivityControl.LoadTaskActivities(_task.Id);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("TaskDetail Page Disappearing");
        }
    }
}
