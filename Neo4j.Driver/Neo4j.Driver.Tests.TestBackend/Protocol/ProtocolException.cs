﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Neo4j.Driver.Tests.TestBackend
{   
    class ProtocolException  : IProtocolObject
    {
        public ProtocolExceptionType data { get; set; } = new ProtocolExceptionType();
        [JsonIgnore]
        public Exception ExceptionObj { get; set; }

        public class ProtocolExceptionType
        {   
            public string msg { get; set; }
        }

        public override async Task Process()
        {
            await Task.CompletedTask;
        }
    }
}
