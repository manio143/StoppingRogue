using Stride.Core;
using Stride.Core.Serialization;
using Stride.Core.Serialization.Contents;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Levels
{
    [DataContract]
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<RawLevel>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<RawLevel>))]
    public class RawLevel
    {
        public string Data;
    }
}
