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
                var person = new Person("Per Persson", 110, new Coords(1, 2));
                string jSonPerson = SerializeObject<Person>(person);
                Console.WriteLine(jSonPerson);
                person = DeserializeObject<Person>(jSonPerson);
                Console.WriteLine(person);
                //for (int i = 0; i < 4; i++)
                //{
                //    string json = SerializeObject<Person>(person, (SerializationFormat)i);
                //    person = DeserializeObject<Person>(wrapper);
                //    Console.WriteLine(person);
                //}

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        static string SerializeObject<T>(T obj) where T : class
        {
            var stream = new MemoryStream();
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            jsonSerializer.WriteObject(stream, obj);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        static T DeserializeObject<T>(string json) where T : class
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes);
            return (T) jsonSerializer.ReadObject(stream);
        }

        //static T DeserializeObject<T>(MemoryStreamWrapper wrapper)
        //    where T : class
        //{
        //    T obj = null;
        //    switch (wrapper.Format)
        //    {
        //        case SerializationFormat.Binary:
        //            var binaryFormatter = new BinaryFormatter();
        //            wrapper.Stream.Position = 0;
        //            obj = (T)binaryFormatter.Deserialize(wrapper.Stream);
        //            break;
        //        case SerializationFormat.Json:
        //            var jsonSerializer = new DataContractJsonSerializer(typeof(T));
        //            wrapper.Stream.Position = 0;
        //            obj = (T)jsonSerializer.ReadObject(wrapper.Stream);
        //            break;
        //        case SerializationFormat.Xml:
        //            var xmlSerializer = new DataContractSerializer(typeof(T));
        //            wrapper.Stream.Position = 0;
        //            obj = (T)xmlSerializer.ReadObject(wrapper.Stream);
        //            break;
        //        case SerializationFormat.Soap:
        //            var soapFormatter = new SoapFormatter();
        //            wrapper.Stream.Position = 0;
        //            obj = (T)soapFormatter.Deserialize(wrapper.Stream);
        //            break;
        //    }
        //    return obj;
        //}

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
