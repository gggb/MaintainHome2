using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using MaintainHome.Models;

namespace MaintainHome.Views
{
    public partial class PartBuyDetail : ContentPage
    {
        public PartBuyDetail()
        {
            InitializeComponent();

            //// Initialize the control
            //CustomControl.TitleLabel.Text = "Parts Buy Detail";
            //CustomControl.TaskTitleLabel.Text = "Task: Buy Parts";
            //var parts = new ObservableCollection<PartBuy>
            //{
            //    new PartBuy { SourceName = "Home Depot", SourceURL = "www.homedepot.com" },
            //    new PartBuy { SourceName = "Lowes", SourceURL = "www.lowes.com" }
            //};
            //CustomControl.ItemsCollectionView.ItemsSource = parts;

            //// Set up item selection event
            //CustomControl.ItemsCollectionView.SelectionChanged += ItemsCollectionView_SelectionChanged;
        }

        private void ItemsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var selectedPart = (PartBuy)e.CurrentSelection.FirstOrDefault();
            //if (selectedPart != null)
            //{
            //    // Create dynamic fields for editing
            //    var fields = new View[]
            //    {
            //        new Entry { Text = selectedPart.SourceName, Placeholder = "Source Name" },
            //        new Entry { Text = selectedPart.SourceURL, Placeholder = "Source URL" }
            //    };
            //    CustomControl.SetDetailFields(fields);
            //}
        }
    }
}
