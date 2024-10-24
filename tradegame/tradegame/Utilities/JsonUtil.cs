using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tradegame.Utilities
{
    public static class JsonUtil
    {
        public static bool CanDeserialize<T>(string json)
        {
            try
            {
                JsonConvert.DeserializeObject<T>(json);
                return true; // Deserialization was successful
            }
            catch (JsonException)
            {
                return false; // Deserialization failed
            }
        }
    }
}
