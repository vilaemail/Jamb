using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Values
{
	public interface IValue<TValue>
	{
		TValue Get();
	}
}
