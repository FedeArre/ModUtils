using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class TextInput : ISetting
    {
        public string Text;
        public string CurrentValue;
        public Action<string> OnValueChange = null;
        public string SettingSaveId;

        public TextInput(string saveId, string text, string currentValue, Action<string> onValueChange)
        {
            SettingSaveId = saveId;
            Text = text;
            CurrentValue = currentValue;
            OnValueChange = onValueChange;
        }
    }
}
