using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.Data
{
	public class SystemSettings
	{
		public string FirmwareVersion { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Location { get; set; }
		public bool RequirePassword { get; set; }
		public int StartupDelay { get; set; }
		public int ChannelInterval { get; set; }
	}
}
