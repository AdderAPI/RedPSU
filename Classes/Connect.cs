using System;
using System.Net;
using System.IO;
using RedPSUAPI.Collections.Data;
using System.Net.Sockets;

namespace RedPSUAPI.Classes
{
	public class Connect
	{		
		public enum Method { POST, PUT, GET, DELETE }
		public bool Debug = false;
		public string Username = "admin";
		public string Password = "password";
		public string IPAddress = "192.168.1.22";
		public bool Authenticate = false;
		public int Retries = 1;
		public int Timeout = 1000;
		public bool IsBusy = false;

		public string Response { get; private set; } = string.Empty;
		public string ErrorMessage { get; private set; } = string.Empty;
		public int StatusCode { get; private set; } = 0;
		public bool IsError { get; private set; } = false;

		public Connect() { }

		public Connect(string ipaddress, string username, string password, bool authenticate)
		{
			IPAddress = ipaddress;
			Username = username;
			Password = password;
			Authenticate = authenticate;
		}

		public APIResponse Get(string parameter, Method method)
		{
			return Get(parameter, method, "");
		}

		public APIResponse Get(string parameter, Method method, string json)
		{
			string url = "http://" + IPAddress + "/api" + parameter;

			APIResponse apiResponse = null;

			int loop = 0;
			bool complete = false;
			DateTime datetime = DateTime.MinValue;

			if (parameter == string.Empty || Username == string.Empty)
			{
				throw new Exception("You must provided a URL and Username");
			}

			IsBusy = true;

			while (loop <= Retries && !complete)
			{
				apiResponse = new APIResponse();
				IsError = false;
				ErrorMessage = string.Empty;
				Response = string.Empty;
				datetime = DateTime.Now;

				HttpWebRequest request = null;
				HttpWebResponse response = null;

				try
				{
					if (Debug)
					{
						Console.WriteLine("Atempt " + (loop + 1).ToString() + " of " + (Retries + 1).ToString());
						Console.WriteLine("Sending WebRequest: " + method.ToString() + " " + url);
					}

					request = (HttpWebRequest)WebRequest.Create(url);
					request.ContentType = "application/json";
					request.Method = method.ToString();
					request.Timeout = Timeout;
					request.ReadWriteTimeout = Timeout;

					if (Authenticate)
					{
						request.UseDefaultCredentials = true;
						request.Credentials = new NetworkCredential(Username, Password);
					}

					if (json != string.Empty)
					{
						if (Debug) Console.WriteLine("Data TX: " + json);

						using (var streamWriter = new StreamWriter(request.GetRequestStream()))
						{
							streamWriter.Write(json);
							streamWriter.Flush();
							streamWriter.Close();
						}

					}

					if (Debug) Console.WriteLine("Waiting for Response");

					using (response = (HttpWebResponse)request.GetResponse())
					{
						Stream dataStream = response.GetResponseStream();
						StreamReader reader = new StreamReader(dataStream);						
						apiResponse.Response = reader.ReadToEnd();
						apiResponse.StatusCode = (int)((HttpWebResponse)response).StatusCode;						
						reader.Close();
						dataStream.Close();						
					}

					if (Debug)
					{
						Console.WriteLine("Data RX: " + apiResponse.Response);
						Console.WriteLine("Status Code: " + apiResponse.StatusCode);
					}

					complete = true;
				}

				catch (SocketException ex)
				{
					apiResponse.Error = true;
					apiResponse.ErrorMessage = ex.Message;
					apiResponse.StatusCode = (int)response.StatusCode;
				}
				catch (IOException ex)
				{
					apiResponse.Error = true;
					apiResponse.ErrorMessage = ex.Message;
					apiResponse.StatusCode = (int)response.StatusCode;
				}
				catch (WebException ex)
				{
					apiResponse.Error = true;
					apiResponse.ErrorMessage = ex.Message;
					if (ex.Response != null)
					{
						apiResponse.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
					}
				}
				catch (Exception ex)
				{
					apiResponse.Error = true;
					apiResponse.ErrorMessage = ex.Message;
					apiResponse.StatusCode = (int)response.StatusCode;
				}

				if (Debug)
				{
					if (apiResponse.Error)
					{
						Console.WriteLine("Error:       " + apiResponse.ErrorMessage);
						Console.WriteLine("Status Code: " + apiResponse.StatusCode);
					}

					TimeSpan ts = DateTime.Now.Subtract(datetime);
					Console.WriteLine("TimeTaken:   " + ts.TotalMilliseconds + "ms");
				}

				loop++;				
			}

			IsError = apiResponse.Error;
			Response = apiResponse.Response;
			ErrorMessage = apiResponse.ErrorMessage;
			StatusCode = apiResponse.StatusCode;
			IsBusy = false;

			return apiResponse;
		}
	}
}
