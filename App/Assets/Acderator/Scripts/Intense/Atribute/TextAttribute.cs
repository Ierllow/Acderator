using System;
using ZLinq;

namespace Intense.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TextAttribute : System.Attribute
    {
        public string Text { get; }

        public TextAttribute(string text)
        {
            Text = text;
        }
    }

    public static class AttributeExtensions
    {
        public static TextAttribute GetTextAttribute(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            return fieldInfo?.GetCustomAttributes(typeof(TextAttribute), false).AsValueEnumerable().Cast<TextAttribute>().FirstOrDefault();
        }
    }
}