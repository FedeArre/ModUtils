using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class Label : ISetting
    {
        public string Text;

        public Label(string text)
        {
            Text = text;
        }
    }
}
