using System.Collections.Generic;
using System.Runtime.Serialization;


namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_Outputs
	{
		[DataContract]
		public class Attributes
		{
			[DataMember]
			public int outputNumber { get; set; }
			[DataMember]
			public double current { get; set; }
			[DataMember]
			public double power { get; set; }
			[DataMember]
			public string status { get; set; }
			[DataMember]
			public string name { get; set; }
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
