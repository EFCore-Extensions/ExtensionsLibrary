using System;

namespace EFCore.Extensions.Scripting
{
    public interface IScriptGenerator : IDisposable
    {
        string GenerateCreateScript();
        DataModel Model { get; }
        string GenerateDiffScript(DataModel previousModel, Versioning version);
    }
}
