using ZLinq;

namespace System
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TextAttribute : Attribute
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