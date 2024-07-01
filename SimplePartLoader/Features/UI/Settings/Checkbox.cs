using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class Checkbox : ISetting
    {
        public string Text;
        public bool Checked;
        public Action<bool> OnValueChange;

        public string SettingSaveId;

        public Checkbox(string saveId, string text, bool @checked, Action<bool> onValueChange = null)
        {
            SettingSaveId = saveId;
            Text = text;
            Checked = @checked;
            OnValueChange = onValueChange;
        }
    }
}
