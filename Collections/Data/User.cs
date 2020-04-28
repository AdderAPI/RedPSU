using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Collections.Data
{
	public class User
	{
		public string Id { get; set; }
		public string Name { get; set; }

		public bool Equals(Output other)
		{
			if (other == null)
				return false;

			return Id == other.Id &&
				   Name == other.Name;
		}
	}
}
