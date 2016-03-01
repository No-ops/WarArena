using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace ExcersiseSerialization
{
    enum SerializationFormat
    {
        Binary,
        Json,
        Xml,
        Soap
    }
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var person = new Person("Per Persson", 110);
                for (int i = 0; i < 4; i++)
                {
                    MemoryStreamWrapper wrapper = SerializeObject(person, (SerializationFormat)i);
                    person = DeserializeObject<Person>(wrapper);
                    Console.WriteLine(person);
                }
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        static T DeserializeObject<T>(MemoryStreamWrapper wrapper)
            where T : class
        {
            T obj = null;
            switch (wrapper.Format)
            {
                case SerializationFormat.Binary:
                    var binaryFormatter = new BinaryFormatter();
                    wrapper.Stream.Position = 0;
                    obj = (T)binaryFormatter.Deserialize(wrapper.Stream);
                    break;
                case SerializationFormat.Json:
                    var jsonSerializer = new DataContractJsonSerializer(typeof(T));
                    wrapper.Stream.Position = 0;
                    obj = (T)jsonSerializer.ReadObject(wrapper.Stream);
                    break;
                case SerializationFormat.Xml:
                    var xmlSerializer = new DataContractSerializer(typeof(T));
                    wrapper.Stream.Position = 0;
                    obj = (T)xmlSerializer.ReadObject(wrapper.Stream);
                    break;
                case SerializationFormat.Soap:
                    var soapFormatter = new SoapFormatter();
                    wrapper.Stream.Position = 0;
                    obj = (T)soapFormatter.Deserialize(wrapper.Stream);
                    break;
            }
            return obj;
        }

        static MemoryStreamWrapper SerializeObject(object obj, SerializationFormat format)
        {
            MemoryStream stream = new MemoryStream();
            
            switch (format)
            {
                case SerializationFormat.Binary:
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(stream, obj);
                    break;
                    case SerializationFormat.Json:
                    var jsonSerializer = new DataContractJsonSerializer(typeof(object));
                    jsonSerializer.WriteObject(stream, obj);
                    break;
                    case SerializationFormat.Xml:
                    var xmlSerializer = new DataContractSerializer(typeof(object));
                    xmlSerializer.WriteObject(stream, obj);
                    break;
                case SerializationFormat.Soap:
                    var soapFormatter = new SoapFormatter();
                    soapFormatter.Serialize(stream, obj);
                    break;
            }
            return new MemoryStreamWrapper
            {
                Stream = stream,
                Format = format
            };
        }
    }
}
