using Blazor.IndexedDB.WebAssembly;
using Microsoft.JSInterop;

namespace Blazor.IndexedDB.WebAssembly.Test.Models
{
    public class ExampleDb : IndexedDb
    {
        public ExampleDb(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }

        public IndexedSet<Person> People { get; set; }
    }
}
