using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.Data
{
	public class Output : IEquatable<Output>
	{
		public string Id { get; set; }
		public int OutputNumber { get; set; }
		public double Current { get; set; }
		public double Power { get; set; }
		public string Status { get; set; }
		public string Name { get; set; }

		public bool Equals(Output other)
		{
			if (other == null)
				return false;

			return Id == other.Id &&
				   OutputNumber == other.OutputNumber &&
				   Current == other.Current &&
				   Power == other.Power &&
				   Status == other.Status &&
				   Name == other.Name;
		}
	}
}
