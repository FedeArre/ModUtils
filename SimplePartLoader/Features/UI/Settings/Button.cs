using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class ModButton : ISetting
    {
        public string Text;
        public Action OnButtonPress;

        public ModButton(string text, Action onButtonPress)
        {
            Text = text;
            OnButtonPress = onButtonPress;
        }
    }
}
