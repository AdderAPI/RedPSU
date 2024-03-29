﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RedPSUAPI.Classes
{
	public class JSON
	{
		public static string Serialize<T>(T obj)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
			MemoryStream ms = new MemoryStream();
			serializer.WriteObject(ms, obj);
			string retVal = Encoding.UTF8.GetString(ms.ToArray());
			return retVal;
		}

		public static T Deserialize<T>(string json)
		{
			T obj = Activator.CreateInstance<T>();
			MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
			obj = (T)serializer.ReadObject(ms);
			ms.Close();
			return obj;
		}

		public static string CreateJson(Dictionary<string, string> fields)
		{
			string json = string.Empty;
			for (int i = 0; i < fields.Count; i++)
			{
				if (json != string.Empty) json += ",";
				json += "\"" + fields.ElementAt(i).Key + "\":" + "\"" + fields.ElementAt(i).Value + "\"";
			}

			if (json != string.Empty)
			{
				json = "{" + json + "}";
			}

			return json;
		}
	}
}
