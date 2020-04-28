using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.Data
{
	public class PSU : IEquatable<PSU>
	{
		public string Id { get; set; }
		public int Number { get; set; }
		public string Status { get; set; }
		public double InternalTemperature { get; set; }
		public double AmbientTemperature { get; set; }
		public int FanSpeed { get; set; }
		public double MainsCurrent { get; set; }
		public double OutputCurrent { get; set; }

		public bool Equals(PSU other)
		{
			if (other == null)
				return false;

			return Id == other.Id &&
				   Number == other.Number &&
				   Status == other.Status &&
				   InternalTemperature == other.InternalTemperature &&
				   AmbientTemperature == other.AmbientTemperature &&
				   FanSpeed == other.FanSpeed &&
				   MainsCurrent == other.MainsCurrent &&
				   OutputCurrent == other.OutputCurrent;
		}
	}
}
