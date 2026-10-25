using System;

namespace Intense.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class TextAttribute : System.Attribute
    {
        public string Text { get; }

        public TextAttribute(string text) => Text = text;
    }
}