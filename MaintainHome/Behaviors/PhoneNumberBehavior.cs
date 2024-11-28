using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace MaintainHome.Behaviors
{
    public class PhoneNumberBehavior : Behavior<Entry>
    {
        static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly(
            nameof(IsValid), typeof(bool), typeof(PhoneNumberBehavior), false);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidPropertyKey, value); }
        }

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

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry; var cursorPosition = entry.CursorPosition; // Save the current cursor position
            var phoneNumber = e.NewTextValue;

            // Remove all non-numeric characters
            phoneNumber = Regex.Replace(phoneNumber, @"[^\d]", "");

            // Limit to 10 digits
            if (phoneNumber.Length > 10)
            {
                phoneNumber = phoneNumber.Substring(0, 10);
            }



            // Format the phone number
            if (phoneNumber.Length > 0)
            {
                if (phoneNumber.Length <= 3)
                    phoneNumber = $"({phoneNumber}";
                else if (phoneNumber.Length <= 6)
                    phoneNumber = $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3)}";
                else
                    phoneNumber = $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6)}";
            }

            entry.Text = phoneNumber;

            // Validate the phone number format
            IsValid = Regex.IsMatch(phoneNumber, @"^\(\d{3}\) \d{3}-\d{4}$");

            // Restore the cursor position to the end of the text
            entry.CursorPosition = phoneNumber.Length;
        }
    }
}
