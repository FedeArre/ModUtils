using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class SmallHeader : ISetting
    {
        public string Text;

        public SmallHeader(string text)
        {
            Text = text;
        }
    }
}
