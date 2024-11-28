using MaintainHome.Models;

namespace MaintainHome.Views
{
    public partial class TaskDetail : ContentPage
    {
        private Tasks _task;

        public TaskDetail(Tasks task)
        {
            InitializeComponent();
            _task = task ?? new Tasks(); // Initialize a new Tasks object if null
            BindingContext = _task;
        }

        // Other methods...
    }
}
