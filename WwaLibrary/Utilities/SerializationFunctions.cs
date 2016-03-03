using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace WwaLibrary.Utilities
{
    public static class SerializationFunctions
    {
        public static string SerializeObject<T>(T obj) where T : class
        {
            var stream = new MemoryStream();
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            jsonSerializer.WriteObject(stream, obj);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static T DeserializeObject<T>(string json) where T : class
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes);
            return (T)jsonSerializer.ReadObject(stream);
        }
    }
}
