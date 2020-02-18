using System;

namespace EFCore.Extensions.Scripting
{
    public abstract class ScriptGenerationBase : IScriptGenerator
    {
        protected IDbContext _context = null;
        public DataModel Model { get; protected set; }

        public ScriptGenerationBase(IDbContext context)
        {
            if (context == null || context.MasterModel == null)
                throw new Exception("The context and model cannot be null.");

            _context = context;
            this.Model = new DataModel(context.MasterModel);
        }

        public abstract string GenerateCreateScript();
        public abstract string GenerateDiffScript(DataModel previousModel, Versioning version);

        void IDisposable.Dispose()
        {
        }

    }
}