using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using System.Windows.Media;

namespace NeeView
{
    public class JintCustomObjectConverter : IObjectConverter
    {
        public bool TryConvert(Engine engine, object value, out JsValue result)
        {
            if (value is Color color)
            {
                result = color.ToString();
                return true;
            }

            result = JsValue.Undefined;
            return false;
        }
    }
}
