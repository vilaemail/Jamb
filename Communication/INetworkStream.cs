using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Communication
{
	/// <summary>
	/// Interface hiding NetworkStream of .NET. 
	/// This is not meant to abstract away NetworkStream implementation details it is used purely for unit test mocking.
	/// </summary>
	internal interface INetworkStream : IDisposable
	{
		bool DataAvailable { get; }
		int Read(byte[] buffer, int offset, int size);
		void Write(byte[] buffer, int offset, int size);
	}
}
