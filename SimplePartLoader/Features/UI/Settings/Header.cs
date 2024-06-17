using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class Header : ISetting
    {
        public string Text;

        public Header(string text)
        {
            Text = text;
        }
    }
}
