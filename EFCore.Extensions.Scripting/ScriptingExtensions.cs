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
                            Newtonsoft.Json.Formatting.None,
                            new Newtonsoft.Json.JsonSerializerSettings
                            {
                                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                            });
        }

        public static T FromJson<T>(string json)
            where T : new()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

    }
}
