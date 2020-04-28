using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_System
	{
		[DataContract]
		public class Attributes
		{
			[DataMember]
			public string firmwareVersion { get; set; }
			[DataMember]
			public string name { get; set; }
			[DataMember]
			public string description { get; set; }
			[DataMember]
			public string location { get; set; }
			[DataMember]
			public bool requirePassword { get; set; }
			[DataMember]
			public int startupDelay { get; set; }
			[DataMember]
			public int channelInterval { get; set; }
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
