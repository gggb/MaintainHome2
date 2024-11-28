using Microsoft.Maui.Controls;

namespace MaintainHome.Controls;

public class DetailPageControl : ContentView
{
    public Label TitleLabel { get; private set; }
    public Label TaskTitleLabel { get; private set; }
    public CollectionView ItemsCollectionView { get; private set; }
    public StackLayout DetailStack { get; private set; }

    public DetailPageControl()
    {
        // Initialize components
        TitleLabel = new Label { FontSize = 24, HorizontalOptions = LayoutOptions.Center };
        TaskTitleLabel = new Label { FontSize = 18, HorizontalOptions = LayoutOptions.Center };
        ItemsCollectionView = new CollectionView
        {
            ItemTemplate = new DataTemplate(() =>
            {
                var stack = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = 5 };
                var sourceNameLabel = new Label { FontSize = 16 };
                sourceNameLabel.SetBinding(Label.TextProperty, "SourceName");
                var sourceUrlLabel = new Label { FontSize = 16, TextColor = Colors.Blue };
                sourceUrlLabel.SetBinding(Label.TextProperty, "SourceURL");
                stack.Children.Add(sourceNameLabel);
                stack.Children.Add(sourceUrlLabel);
                return stack;
            })
        };
        DetailStack = new StackLayout
        {
            Children =
            {
                new Label { Text = "View/Edit Parts", FontSize = 18, HorizontalOptions = LayoutOptions.Center }
                // Fields will be dynamically added here
            }
        };

        // Arrange components in a StackLayout
        var stackLayout = new StackLayout
        {
            Padding = 10,
            Children =
            {
                TitleLabel,
                TaskTitleLabel,
                ItemsCollectionView,
                DetailStack
            }
        };

        // Set the content of the control
        Content = stackLayout;
    }

    public void SetDetailFields(View[] fields)
    {
        // Clear previous fields
        DetailStack.Children.Clear();
        DetailStack.Children.Add(new Label { Text = "View/Edit Parts", FontSize = 18, HorizontalOptions = LayoutOptions.Center });
        foreach (var field in fields)
        {
            DetailStack.Children.Add(field);
        }
    }
}
