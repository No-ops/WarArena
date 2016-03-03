using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExcersiseSerialization
{
    //[Serializable]
    //[KnownType(typeof(Person))]
    [DataContract]
    class Person
    {
        public Person(string name, int age, Coords coordinates)
        {
            Name = name;
            Age = age;
            Coordinates = coordinates;
        }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Age { get; set; }
        [DataMember]
        public Coords Coordinates { get; set;  }

        public override string ToString()
        {
            return $"Name: {Name} Age: {Age} X: {Coordinates.X} Y: {Coordinates.Y}";
        }
    }
}
