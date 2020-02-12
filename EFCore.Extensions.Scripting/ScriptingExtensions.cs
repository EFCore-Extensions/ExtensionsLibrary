using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    public static class ScriptingExtensions
    {
        public static string ToJson<T>(this T obj)
            where T : new()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj,
                            Newtonsoft.Json.Formatting.Indented,
                            new Newtonsoft.Json.JsonSerializerSettings
                            {
                                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                            });
        }

        public static T FromJson<T>(string json)
            where T : new()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new PrivateSetterContractResolver()
            };

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, settings);
        }

    }
}
