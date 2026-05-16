using System;
using UnityEditor;

namespace Submodules.Common.Tools.Overlays
{
    public class ScriptRuleContext
    {
        public string Path { get; }
        public MonoScript Script { get; }
        public Type Type { get; }
        public string AssemblyName { get; }

        public ScriptRuleContext(string path, MonoScript script, Type type, string assemblyName)
        {
            Path = path;
            Script = script;
            Type = type;
            AssemblyName = assemblyName;
        }
    }
}