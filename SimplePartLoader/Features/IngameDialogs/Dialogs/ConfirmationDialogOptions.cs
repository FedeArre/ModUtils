using System;

namespace SimplePartLoader
{
    public class ConfirmationDialogOptions
    {
        public string Title;
        public string Description;
        public string YesButtonText;
        public string NoButtonText;
        public Action OnYesClick;
        public Action OnNoClick;

        public ConfirmationDialogOptions() : this("", "") { }

        public ConfirmationDialogOptions(string title, string description)
        {
            Title = title;
            Description = description;
            YesButtonText = "Yes";
            NoButtonText = "No";
            OnYesClick = null;
            OnNoClick = null;
        }
    }
}
