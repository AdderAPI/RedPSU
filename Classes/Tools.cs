using System;
using System.Text.RegularExpressions;

namespace RedPSUAPI.Classes
{
	public class Tools
	{
		public static bool IsValidIP4(String ipaddress)
		{
			Regex regex = new Regex(@"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$");
			return regex.IsMatch(ipaddress);
		}

		public static bool IsValidMacAddress(String macaddress)
		{
			Regex regex = new Regex(@"^([0-9a-fA-F][0-9a-fA-F]:){5}([0-9a-fA-F][0-9a-fA-F])$");
			return regex.IsMatch(macaddress);
		}
	}
}
