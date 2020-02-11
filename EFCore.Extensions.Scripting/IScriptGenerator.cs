using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    public interface IScriptGenerator
    {
        string GenerateCreateScript();
        DataModel Model { get; }
        string GenerateDiffScript(DataModel previousModel);
    }
}
