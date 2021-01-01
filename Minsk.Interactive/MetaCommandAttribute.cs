using System;

namespace Minsk.Interactive
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class MetaCommandAttribute : Attribute
    {
        public MetaCommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }
}
