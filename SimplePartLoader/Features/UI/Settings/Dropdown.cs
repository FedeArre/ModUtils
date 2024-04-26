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

        public ModDropdown(string text, string[] options, int selectedOption = 0, Action<int> onValueChange = null)
        {
            this.Text = text;
            this.Options = options;
            this.selectedOption = selectedOption;
            OnValueChange = onValueChange;
        }
    }
}
