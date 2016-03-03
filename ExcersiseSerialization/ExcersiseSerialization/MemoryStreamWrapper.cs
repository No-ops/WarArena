using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcersiseSerialization
{
    class MemoryStreamWrapper
    {
        public MemoryStream Stream { get; set; }
        public SerializationFormat Format { get; set; }
    }
}
