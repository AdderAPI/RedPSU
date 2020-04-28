using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_Network
	{
		[DataContract]
		public class Attributes
		{
			[DataMember]
			public string macAddress { get; set; }
			[DataMember]
			public string ipAddress { get; set; }
			[DataMember]
			public string ipNetmask { get; set; }
			[DataMember]
			public string ipGateway { get; set; }
			[DataMember]
			public bool useDhcp { get; set; }
		}

		[DataContract]
		public class Data
		{
			[DataMember]
			public string id { get; set; }
			[DataMember]
			public string type { get; set; }
			[DataMember]
			public Attributes attributes { get; set; }
		}

		[DataMember]
		public Data data { get; set; }
	}
}
