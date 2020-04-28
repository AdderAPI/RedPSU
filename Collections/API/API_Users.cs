using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_Users
	{
		[DataContract]
		public class Attributes
		{
			[DataMember]
			public string username { get; set; }
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
		public List<Data> data { get; set; }
	}
}
