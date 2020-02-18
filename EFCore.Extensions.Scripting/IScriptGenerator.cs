namespace EFCore.Extensions.Scripting
{
    public interface IScriptGenerator
    {
        string GenerateCreateScript();
        DataModel Model { get; }
        string GenerateDiffScript(DataModel previousModel);
    }
}
