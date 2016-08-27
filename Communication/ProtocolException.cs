using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication
{
    public class ProtocolException : CommunicationException
    {
        public ProtocolException()
        {
        }

        public ProtocolException(string message)
            : base(message)
        {
        }

        public ProtocolException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
