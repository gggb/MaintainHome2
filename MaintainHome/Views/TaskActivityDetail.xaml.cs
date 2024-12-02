using MaintainHome.Models;

namespace MaintainHome.Views
{
    public partial class TaskActivityDetail : ContentPage
    {
        public TaskActivityDetail()
        {
            InitializeComponent();
            BindingContext = new TaskActivity(); // Assuming you have a TaskActivityViewModel
        }
    }
}
