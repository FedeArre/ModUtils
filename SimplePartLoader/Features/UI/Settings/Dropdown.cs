using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class ModDropdown : ISetting
    {
        public string Text;
        public string[] Options;
        public int selectedOption;
        public Action<int> OnValueChange = null;
        public string SettingSaveId;

        public ModDropdown(string saveId, string text, string[] options, int selectedOption = 0, Action<int> onValueChange = null)
        {
            SettingSaveId = saveId;
            this.Text = text;
            this.Options = options;
            this.selectedOption = selectedOption;
            OnValueChange = onValueChange;
        }
    }
}
