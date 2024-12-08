using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MaintainHome.Controls
{
    public interface IListEditBaseControl
    {
        bool IsAddVisible { get; set; }
        bool IsCancelVisible { get; set; }
        bool IsDeleteVisible { get; set; }

        // ======================================================
        // Likely should be removed-- not common to all 5 controls.
        //ObservableCollection<TaskNote> ItemsSource { get; set; }
        // =======================================================


        Page GetParentPage();
        SelectionMode CollectionSelectionMode { get; set; }
        string ClassLabelText { get; set; }
        string SectionTitle { get; set; }
        string Title { get; set; }
        Task DisplayAlert(string title, string message, string cancel);
        Task LoadData(int taskId);
        Task OnCancelButtonClickedAsync(object sender, EventArgs e);
        Task OnDeleteButtonClickedAsync(object sender, EventArgs e);
        Task OnSaveButtonClickedAsync(object sender, EventArgs e);

        void OnCancelButtonClicked(object sender, EventArgs e);
        void OnDeleteButtonClicked(object sender, EventArgs e);
        void OnSaveButtonClicked(object sender, EventArgs e);



        Task<bool> ValidateForm();

        // ======================================================
        // Likely should be removed-- not common to all 5 controls.
        //TaskNote SelectedItem { get; set; }
        //TaskNote SelectedItem { get; set; }
        // =======================================================

        void OnNewButtonClicked(object sender, EventArgs e);
        void OnSelectionChanged(object sender, SelectionChangedEventArgs e);
        int TaskId { get; set; }
    }
}
