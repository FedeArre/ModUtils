using System;

namespace SimplePartLoader
{
    public class InformationDialogOptions
    {
        public string Title;
        public string Description;
        public string ButtonText;
        public Action OnButtonClick;

        public InformationDialogOptions() : this("", "") { }

        public InformationDialogOptions(string title, string description)
        {
            Title = title;
            Description = description;
            ButtonText = "OK";
            OnButtonClick = null;
        }
    }
}
