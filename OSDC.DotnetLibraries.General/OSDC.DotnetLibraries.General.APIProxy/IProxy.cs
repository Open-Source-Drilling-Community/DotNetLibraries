using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.APIProxy
{
	public interface IProxy
	{
		public string EndPoint { get; set; } 
		public string API { get; set; } 
		public string Host { get; set; }
	}
}
