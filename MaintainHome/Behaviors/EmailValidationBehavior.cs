using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace MaintainHome.Behaviors
{
    public class EmailValidationBehavior : Behavior<Entry>
    {
        static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly(
            nameof(IsValid), typeof(bool), typeof(EmailValidationBehavior), false);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidPropertyKey, value); }
        }

        static readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += OnTextChanged;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(bindable);
        }

        //void OnTextChanged(object sender, TextChangedEventArgs e)   //this routine was cause runtime exceptions!!!!!
        //{
        //    IsValid = emailRegex.IsMatch(e.NewTextValue);
        //    ((Entry)sender).TextColor = IsValid ? Colors.Black : Colors.Red;
        //}

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var newValue = e.NewTextValue;

            if (!string.IsNullOrEmpty(newValue))
            {
                IsValid = emailRegex.IsMatch(newValue);
            }
            else
            {
                IsValid = false; // Consider empty values as invalid
            }

            entry.TextColor = IsValid ? Colors.Black : Colors.Red;
        }





    }
}

