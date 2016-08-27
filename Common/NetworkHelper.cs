using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Common
{
	/// <summary>
	/// Contains helper functions to get information about our network.
	/// </summary>
	class NetworkHelper
	{
		/// <summary>
		/// Finds all IP addresses and returns only ones that are IPv4 in a list.
		/// </summary>
		public static List<IPAddress> GetLocalIPv4Addresses()
		{
			List<IPAddress> result = new List<IPAddress>();
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					result.Add(ip);
				}
			}
			return result;
		}
	}
}
