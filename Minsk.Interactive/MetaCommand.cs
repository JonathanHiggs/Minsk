﻿using System.Reflection;

namespace Minsk.Interactive
{
    internal sealed class MetaCommand
    {
        public MetaCommand(string name, string description, MethodInfo method)
        {
            Name = name;
            Description = description;
            Method = method;
        }

        public string Name { get; }
        public string Description { get; }
        public MethodInfo Method { get; }
    }
}
