using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_Output
	{
		public class Attributes
		{
			[DataMember]
			public int outputNumber { get; set; }
			[DataMember]
			public int current { get; set; }
			[DataMember]
			public int power { get; set; }
			[DataMember]
			public string status { get; set; }
			[DataMember]
			public string name { get; set; }
		}

		[DataContract]
		public class Data
		{
			[DataMember]
			public string type { get; set; }
			[DataMember]
			public string id { get; set; }
			[DataMember]
			public Attributes attributes { get; set; }
		}

		[DataMember]
		public Data data { get; set; }
	}
}
