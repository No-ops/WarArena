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
        public static T DeserializeObject<T>(Stream stream)
            where T : class
        {
            T obj = null;
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            stream.Position = 0;
            obj = (T)jsonSerializer.ReadObject(stream);
            return obj;
        }
    }
}
