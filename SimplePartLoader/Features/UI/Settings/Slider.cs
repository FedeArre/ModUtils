using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePartLoader
{
    public class ModSlider : ISetting
    {
        public float MinValue;
        public float MaxValue;
        public float Value;
        public bool WholeNumbers;
        public Action<float> OnValueChanged;
        public string SettingSaveId;

        public ModSlider(string saveId, float minValue, float maxValue, bool WholeNumbers = true, Action<float> onValueChanged = null)
        {
            SettingSaveId = saveId;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = minValue;
            OnValueChanged = onValueChanged;
            this.WholeNumbers = WholeNumbers;
        }

        public ModSlider(string saveId, float minValue, float maxValue, float value, bool WholeNumbers = true, Action<float> onValueChanged = null)
        {
            SettingSaveId = saveId;
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
            OnValueChanged = onValueChanged;
            this.WholeNumbers = WholeNumbers;
        }
    }
}
