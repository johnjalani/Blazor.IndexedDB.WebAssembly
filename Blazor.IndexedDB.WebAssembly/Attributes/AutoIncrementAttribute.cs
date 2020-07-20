using System;

namespace Blazor.IndexedDB.WebAssembly.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AutoIncrementAttribute : Attribute { }
}
