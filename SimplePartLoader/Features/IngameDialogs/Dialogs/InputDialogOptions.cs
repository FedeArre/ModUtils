using System;

namespace SimplePartLoader
{
    public class InputDialogOptions
    {
        public string Title;
        public string Description;
        public string YesButtonText;
        public string NoButtonText;
        public string InputDefaultText;
        public string InputPlaceholderText;
        public Action<string> OnYesClick;
        public Action OnNoClick;

        public InputDialogOptions() : this("", "") { }

        public InputDialogOptions(string title, string description)
        {
            Title = title;
            Description = description;
            YesButtonText = "Yes";
            NoButtonText = "No";
            InputDefaultText = "";
            InputPlaceholderText = "";
            OnYesClick = null;
            OnNoClick = null;
        }
    }
}
