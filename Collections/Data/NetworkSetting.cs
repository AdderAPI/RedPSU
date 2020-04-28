using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.Data
{
	public class NetworkSetting : IEquatable<NetworkSetting>
	{
		public string IPAddress { get; set; }
		public string MACAddress { get; set; }
		public string NetMask { get; set; }
		public string Gateway { get; set; }
		public bool UseDhcp { get; set; }

		public bool Equals(NetworkSetting other)
		{
			if (other == null)
				return false;

			return IPAddress == other.IPAddress &&
				   MACAddress == other.MACAddress &&
				   NetMask == other.NetMask &&
				   Gateway == other.Gateway &&
				   UseDhcp == other.UseDhcp;
		}
	}
}

