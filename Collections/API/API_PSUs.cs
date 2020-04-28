using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RedPSUAPI.Collections.API
{
	[DataContract]
	public class API_PSUs
	{
		[DataContract]
		public class Attributes
		{
			[DataMember]
			public int psuNumber { get; set; }
			[DataMember]
			public string status { get; set; }
			[DataMember]
			public double internalTemperature { get; set; }
			[DataMember]
			public double ambientTemperature { get; set; }
			[DataMember]
			public int fanSpeed { get; set; }
			[DataMember]
			public double mainsCurrent { get; set; }
			[DataMember]
			public double outputCurrent { get; set; }
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
