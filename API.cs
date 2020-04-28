using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Timers;
using RedPSUAPI.Classes;
using RedPSUAPI.Collections.API;
using RedPSUAPI.Collections.Data;

namespace RedPSUAPI
{
	public class API
	{
		#region Events
		public event EventHandler OnError;
		public event EventHandler OnComplete;
		public event EventHandler OnNetworkUpdate;
		public event EventHandler OnSystemUpdate;
		public event EventHandler OnOutputUpdated;
		public event EventHandler OnPSUUpdated;
		public event EventHandler OnUserAdded;
		public event EventHandler OnUserRemoved;
		public event EventHandler OnUserUpdated;
		#endregion

		#region Enum
		private enum ResetLevel
		{
			UNIT, FACTORY
		}

		private enum PowerState
		{
			ON, OFF
		}
		#endregion

		private SynchronizationContext _context = null;
		private Connect _connect = new Connect();
		private bool _debug = false;
		private bool _autoupdate = false;
		private Thread _thread;
		private System.Timers.Timer _timergetupdate = new System.Timers.Timer(2000);

		/// <summary>
		/// Enable Logging to Console
		/// </summary>
		public bool Debug
		{
			get { return _debug; }
			set { _debug = value; _connect.Debug = _debug; }
		}

		/// <summary>
		/// List of Outputs
		/// </summary>
		public ObservableCollection<Output> Outputs = new ObservableCollection<Output>();

		/// <summary>
		/// Network Settings
		/// </summary>
		public NetworkSetting Network { get; private set; } = null;

		/// <summary>
		/// System Settings
		/// </summary>
		public SystemSettings System { get; private set; } = null;

		/// <summary>
		/// List of Users
		/// </summary>
		public ObservableCollection<User> Users = new ObservableCollection<User>();

		/// <summary>
		/// List of PSUs
		/// </summary>
		public ObservableCollection<PSU> PSUs = new ObservableCollection<PSU>();

		/// <summary>
		/// IP Address for the REDPSU
		/// </summary>
		public string IPAddress
		{
			get { return _connect.IPAddress; }
			set { _connect.IPAddress = value; }
		}

		/// <summary>
		/// Username for the REDPSU
		/// </summary>
		public string Username
		{
			get { return _connect.Username; }
			set { _connect.Username = value; }
		}

		/// <summary>
		/// Password for the REDPSU
		/// </summary>
		public string Password
		{
			get { return _connect.Password; }
			set { _connect.Password = value; }
		}
		/// <summary>
		/// Use Authentication to connect to REDPSU
		/// </summary>
		public bool Authenticate
		{
			get { return _connect.Authenticate; }
			set { _connect.Authenticate = value; }
		}
	
		/// <summary>
		/// Return Error Message
		/// </summary>
		public ErrorMessage Error
		{
			get
			{
				ErrorMessage msg = new ErrorMessage();
				msg.IsError = _connect.IsError;
				msg.Code = _connect.StatusCode.ToString();
				msg.Message = _connect.ErrorMessage;
				return msg;
			}
		}

		/// <summary>
		/// Enable Auto Update
		/// </summary>
		public bool AutoUpdate
		{
			get { return _autoupdate; }
			set { _autoupdate = value; }
		}

		/// <summary>
		/// Set the Automatic update interval
		/// </summary>
		public double AutoUpdateInterval
		{
			get { return _timergetupdate.Interval; }
			set { _timergetupdate.Interval = value; }
		}

		public API()
		{
			Initialise();
		}

		public API(string ipaddress, string username, string password, bool authenticate)
		{
			Initialise();
			_connect = new Connect(ipaddress, username, password, authenticate);
		}

		private void Initialise()
		{
			_context = SynchronizationContext.Current;
			if (_context == null) throw new Exception("You must initialise this class inside a function.");

			Outputs.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedEvent);
			PSUs.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedEvent);
			Users.CollectionChanged += new NotifyCollectionChangedEventHandler(CollectionChangedEvent);

			_timergetupdate.Elapsed += OnTimerGetUpdate;
		}

		/// <summary>
		/// Get Data from REDPSU
		/// </summary>
		/// <returns></returns>
		public void GetData()
		{
			if (!_connect.IsBusy)
			{
				if (!_connect.IsError) GetSystem();
				if (!_connect.IsError) GetNetwork();
				if (!_connect.IsError) GetOutputs();
				if (!_connect.IsError) GetUsers();
				if (!_connect.IsError) GetPSUs();
				if (!_connect.IsError) CompleteEvent(this, new EventArgs());
			}

			if (_autoupdate && !_timergetupdate.Enabled)
			{
				_timergetupdate.Enabled = true;
			}
		}

		/// <summary>
		/// Get Data on Thread
		/// </summary>
		public void GetDataAsyc()
		{
			if (_thread == null)
			{
				_thread = new Thread(GetData);
				_thread.IsBackground = true;
				_thread.Name = "GetData: " + DateTime.Now.ToLongTimeString();
				_thread.Start();
			}
		}

		/// <summary>
		/// Stop the Async Updates
		/// </summary>
		public void StopUpdates()
		{
			if (_thread != null)
			{
				_thread.Abort();
				_autoupdate = false;
				_timergetupdate.Enabled = false;
			}
		}

		/// <summary>
		/// Reboot the REDPSU
		/// </summary>
		/// <returns></returns>
		public bool Reboot()
		{
			return Reset(ResetLevel.UNIT);
		}

		/// <summary>
		/// Factory Reset the REDPSU to default settings
		/// </summary>
		/// <returns></returns>
		public bool FactoryReset()
		{
			return Reset(ResetLevel.FACTORY);
		}

		private bool Reset(ResetLevel level)
		{
			bool result = false;
			string json = "{ \"parameters\":{ \"level\":\"" + level.ToString() + "\"} }";
			APIResponse apiresponse = _connect.Get("/system/reset", Connect.Method.POST, json);

			if (apiresponse.StatusCode == 204)
				result = true;
			else
				ErrorEvent(this, new EventArgs());

			return result;
		}

		/// <summary>
		/// Get Output by Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Output GetOutput(string id)
		{
			Output output = null;

			if (id == string.Empty)
			{
				throw new Exception("Id can not be empty");
			}

			APIResponse apiresponse = _connect.Get("/outputs/" + id, Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_Output apioutput = JSON.Deserialize<API_Output>(apiresponse.Response);

				if (apioutput != null)
				{
					output = new Output();
					output.Id = apioutput.data.id;
					output.Name = apioutput.data.attributes.name;
					output.OutputNumber = apioutput.data.attributes.outputNumber;
					output.Current = apioutput.data.attributes.current;
					output.Power = apioutput.data.attributes.power;
					output.Status = apioutput.data.attributes.status;
				}
			}

			if (output == null) ErrorEvent(this, new EventArgs());

			return output;
		}


		/// <summary>
		/// Get List of Outputs
		/// </summary>
		/// <returns>Success</returns>
		public bool GetOutputs()
		{
			bool result = false;
			APIResponse apiresponse = _connect.Get("/outputs", Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_Outputs apioutputs = JSON.Deserialize<API_Outputs>(apiresponse.Response);

				if (apioutputs != null)
				{
					List<Output> tmpoutputs = new List<Output>();
					foreach (API_Outputs.Data data in apioutputs.data)
					{
						Output tmp = new Output();
						tmp.Id = data.id;
						tmp.Name = data.attributes.name;
						tmp.OutputNumber = data.attributes.outputNumber;
						tmp.Current = data.attributes.current;
						tmp.Power = data.attributes.power;
						tmp.Status = data.attributes.status;
						tmpoutputs.Add(tmp);
					}

					lock (Outputs)
					{
						foreach (Output output in tmpoutputs)
						{
							if (!Outputs.Contains(output))
							{
								int index = -1;

								for (int i = 0; i < Outputs.Count; i++)
								{
									if (Outputs[i].Id == output.Id) index = i;
								}

								if (index > -1)
									Outputs[index] = output;
								else
									Outputs.Add(output);
							}
						}
					}
					result = true;
				}
			}

			if (!result) ErrorEvent(this, new EventArgs());

			return result;
		}

		/// <summary>
		/// Turn Output On
		/// </summary>
		/// <param name="id">Output Id</param>
		/// <returns>Success</returns>
		public bool SetOutputOn(string id)
		{
			return SetOutputPowerState(id, PowerState.ON);
		}

		/// <summary>
		/// Turn Output Off
		/// </summary>
		/// <param name="id">Output Id</param>
		/// <returns>Success</returns>
		public bool SetOutputOff(string id)
		{
			return SetOutputPowerState(id, PowerState.OFF);
		}

		/// <summary>
		/// Set ALl Ouputs to On
		/// </summary>
		/// <returns></returns>
		public bool SetOutputsAllOn()
		{
			bool result = true;
			int loop = 0;

			while(loop < Outputs.Count && result)
			{
				result = SetOutputOn(Outputs[loop].Id);
				loop++;
			}

			return result;
		}

		/// <summary>
		/// Set All Outputs to Off
		/// </summary>
		/// <returns></returns>
		public bool SetOutputsAllOff()
		{
			bool result = true;
			int loop = 0;

			while (loop < Outputs.Count && result)
			{
				result = SetOutputOff(Outputs[loop].Id);
				loop++;
			}

			return result;
		}

		private bool SetOutputPowerState(string id, PowerState power)
		{
			bool result = false;
			Output output = FindOutputById(id);

			if (output != null)
			{
				string json = "{ \"data\":{ \"type\":\"output\",\"id\":\"" + output.Id + "\",\"attributes\":{ \"name\":\"" + output.Name + "\",\"status\":\"" + power.ToString() + "\"} } }";
				APIResponse apiresponse = _connect.Get("/outputs/" + id, Connect.Method.PUT, json);
				if (apiresponse.StatusCode == 204)
					result = true;
				else
					ErrorEvent(this, new EventArgs());
			}
			else
			{
				throw new Exception("Output Id does not exist");
			}

			return result;
		}

		/// <summary>
		/// Set the Output Name
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <returns>Success</returns>
		public bool SetOutputName(string id, string name)
		{
			bool result = false;
			Output output = FindOutputById(id);

			if (output != null)
			{
				string json = "{ \"data\":{ \"type\":\"output\",\"id\":\"" + output.Id + "\",\"attributes\":{ \"name\":\"" + name + "\"}}}";
				APIResponse apiresponse = _connect.Get("/outputs/" + id, Connect.Method.PUT, json);
				if (apiresponse.StatusCode == 204)
				{
					output.Name = name;
					result = true;
				}
				else
					ErrorEvent(this, new EventArgs());
			}
			return result;
		}

		/// <summary>
		/// Find Output by Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Output</returns>
		public Output FindOutputById(string id)
		{
			Output output = null;

			foreach (Output findoutput in Outputs)
			{
				if (findoutput.Id == id)
				{
					output = findoutput;
					break;
				}
			}

			return output;
		}

		private bool GetNetwork()
		{
			bool result = false;

			APIResponse apiresponse = _connect.Get("/network", Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_Network apinetwork = JSON.Deserialize<API_Network>(apiresponse.Response);

				if (apinetwork != null)
				{
					NetworkSetting tmp = new NetworkSetting();
					tmp.IPAddress = apinetwork.data.attributes.ipAddress;
					tmp.NetMask = apinetwork.data.attributes.ipNetmask;
					tmp.Gateway = apinetwork.data.attributes.ipGateway;
					tmp.MACAddress = apinetwork.data.attributes.macAddress;
					tmp.UseDhcp = apinetwork.data.attributes.useDhcp;

					if (Network == null) Network = new NetworkSetting();
					if (!Network.Equals(tmp))
					{
						Network = tmp;
						NetworkSettingsEvent(this, new EventArgs());
					}

					result = true;
				}
			}

			return result;
		}


		/// <summary>
		/// Set Network IP Address settings
		/// </summary>
		/// <param name="ipaddress"></param>
		/// <param name="netmask"></param>
		/// <param name="gateway"></param>
		/// <returns></returns>
		public bool SetNetworkIPSettings(string ipaddress, string netmask, string gateway)
		{
			bool result = false;

			//Checks
			if (Network == null)
			{
				throw new Exception("Network object not intialised");
			}

			if (!Tools.IsValidIP4(ipaddress) || !Tools.IsValidIP4(gateway))
			{
				throw new Exception("Invalid IPAddress or Gateway");
			}

			//string json = "{\"data\":{ \"type\":\"network\",\"id\":\"info\",\"attributes\":{ \"macAddress\":\"" + network.MACAddress + "\",\"ipAddress\":\"" + network.IPAddress + "\",\"ipNetmask\":\"" + network.NetMask + "\",\"ipGateway\":\"" + network.Gateway + "\",\"useDhcp\":" + network.UseDhcp.ToString() + "}}}";
			string json = "{\"data\":{ \"type\":\"network\",\"id\":\"info\",\"attributes\":{ \"ipAddress\":\"" + ipaddress + "\",\"ipNetmask\":\"" + netmask + "\",\"ipGateway\":\"" + gateway + "\"}}}";

			APIResponse apiresponse = _connect.Get("/network", Connect.Method.PUT, json);
			if (apiresponse.StatusCode == 204)
			{
				Network.IPAddress = ipaddress;
				Network.Gateway = gateway;
				Network.NetMask = netmask;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Set the Network Use DHCP
		/// </summary>
		/// <param name="usedhcp"></param>
		/// <returns>Success</returns>
		public bool SetNetworkUseDHCP(bool usedhcp)
		{
			bool result = false;

			//Checks
			if (Network == null)
			{
				throw new Exception("Network object not intialised");
			}

			string json = "{\"data\":{ \"type\":\"network\",\"id\":\"info\",\"attributes\":{ \"useDhcp\":" + usedhcp.ToString() + "}}}";

			APIResponse apiresponse = _connect.Get("/network", Connect.Method.PUT, json);
			if (apiresponse.StatusCode == 204)
			{
				Network.UseDhcp = usedhcp;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Get System Settings
		/// </summary>
		/// <returns>Success</returns>
		public bool GetSystem()
		{
			bool result = false;

			APIResponse apiresponse = _connect.Get("/system", Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_System apisystem = JSON.Deserialize<API_System>(apiresponse.Response);

				if (apisystem != null)
				{
					SystemSettings tmp = new SystemSettings();
					tmp.FirmwareVersion = apisystem.data.attributes.firmwareVersion;
					tmp.Name = apisystem.data.attributes.name;
					tmp.Description = apisystem.data.attributes.description;
					tmp.Location = apisystem.data.attributes.location;
					tmp.RequirePassword = apisystem.data.attributes.requirePassword;
					tmp.StartupDelay = apisystem.data.attributes.startupDelay;
					tmp.ChannelInterval = apisystem.data.attributes.channelInterval;

					if (System == null) System = new SystemSettings();
					if (!System.Equals(tmp))
					{
						System = tmp;
						SystemSettingsEvent(this, new EventArgs());
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Set the System Name
		/// </summary>
		/// <param name="name"></param>
		/// <returns>Success</returns>
		public bool SetSystemName(string name)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			//string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"description\":\"" + System.Description + "\",\"location\":\"" + System.Location + "\",\"name\":\"" + name + "\",\"requirePassword\":" + System.RequirePassword.ToString() + ",\"startupDelay\":" + System.StartupDelay.ToString() + ",\"channelInterval\":" + System.ChannelInterval.ToString() + "}}}";
			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"name\":\"" + name + "\"}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.Name = name;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Set the System Description
		/// </summary>
		/// <param name="description"></param>
		/// <returns>Success</returns>
		public bool SetSystemDescription(string description)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"description\":\"" + description + "\"}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.Description = description;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Set the System Location
		/// </summary>
		/// <param name="location"></param>
		/// <returns>Success</returns>
		public bool SetSystemLocation(string location)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"location\":\"" + location + "\"}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.Location = location;
				result = true;
			}

			return result;
		}


		/// <summary>
		/// Set the System Require Password
		/// </summary>
		/// <param name="requirepassword"></param>
		/// <returns>Success</returns>
		public bool SetSystemRequirePassword(bool requirepassword)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"requirePassword\":" + requirepassword.ToString() + "\"}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.RequirePassword = requirepassword;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Set the System Startup Delay
		/// </summary>
		/// <param name="delay">Between 1 and 10000</param>
		/// <returns>Success</returns>
		public bool SetSystemStartupDelay(int delay)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			if (delay < 1 || delay > 10000)
			{
				throw new Exception("Delay must be between 1 and 10000");
			}

			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"startupDelay\":" + delay.ToString() + "\"}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.StartupDelay = delay;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Set the System Channel Interval
		/// </summary>
		/// <param name="interval">Between 1 and 2000</param>
		/// <returns>Success</returns>
		public bool SetSystemChannelInterval(int interval)
		{
			bool result = false;

			if (System == null)
			{
				throw new Exception("System object not intialised");
			}

			if (interval < 1 || interval > 2000)
			{
				throw new Exception("Interval must be between 1 and 2000");
			}

			string json = "{\"data\":{\"type\":\"system\",\"id\":\"info\",\"attributes\":{\"channelInterval\":" + interval.ToString() + "}}}";
			APIResponse apiresponse = _connect.Get("/system", Connect.Method.PUT, json);

			if (apiresponse.StatusCode == 204)
			{
				System.ChannelInterval = interval;
				result = true;
			}

			return result;
		}

		/// <summary>
		/// Get Users
		/// </summary>
		/// <returns>Success</returns>
		public bool GetUsers()
		{
			bool result = false;

			APIResponse apiresponse = _connect.Get("/users", Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_Users apisystem = JSON.Deserialize<API_Users>(apiresponse.Response);

				if (apisystem != null)
				{
					List<User> tmpusers = new List<User>();
					foreach (API_Users.Data data in apisystem.data)
					{
						User tmp = new User();
						tmp.Id = data.id;
						tmp.Name = data.attributes.username;
						tmpusers.Add(tmp);
					}

					foreach (User user in tmpusers)
					{
						if (!Users.Contains(user))
						{
							int index = -1;

							for (int i = 0; i < Users.Count; i++)
							{
								if (Users[i].Id == user.Id) index = i;
							}

							if (index > -1)
								Users[index] = user;
							else
								Users.Add(user);
						}
					}
					result = true;
				}
			}

			if (!result) ErrorEvent(this, new EventArgs());

			return result;
		}


		/// <summary>
		/// Add User
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns>Success</returns>
		public bool AddUser(string username, string password)
		{
			bool result = false;

			string cmd = "{ \"data\": { \"type\": \"user\", \"attributes\": { \"username\": \"" + username + "\", \"password\": \"" + password + "\" }}}";
			APIResponse apiresponse = _connect.Get("/users", Connect.Method.POST, cmd);

			if (apiresponse.StatusCode == 201)
			{
				result = true;
			}

			if (!result) ErrorEvent(this, new EventArgs());

			return result;
		}

		/// <summary>
		/// Set User Password By Id
		/// </summary>
		/// <param name="id"></param>
		/// <param name="password"></param>
		/// <returns>Success</returns>
		public bool SetUserPasswordById(string id, string password)
		{
			bool result = false;

			string cmd = "{ \"data\": { \"type\": \"user\", \"id\": \"" + id + "\", \"attributes\": { \"password\": \"" + password + "\" }}}";
			APIResponse apiresponse = _connect.Get("/users/" + id, Connect.Method.PUT, cmd);

			if (apiresponse.StatusCode == 204)
			{
				result = true;
			}

			if (!result) ErrorEvent(this, new EventArgs());

			return result;
		}

		/// <summary>
		/// Set User Password by User name
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns>Success</returns>
		public bool SetUserPasswordByName(string username, string password)
		{
			bool result = false;

			foreach (User user in Users)
			{
				if (user.Name.ToLower() == username.ToLower())
				{
					result = SetUserPasswordById(user.Id, password);
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Delete User by Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Success</returns>
		public bool DeleteUserById(string id)
		{
			bool result = false;
			
			APIResponse apiresponse = _connect.Get("/users/" + id, Connect.Method.DELETE);

			if (apiresponse.StatusCode == 204)
			{
				result = true;
			}

			if (!result) ErrorEvent(this, new EventArgs());		

			return result;
		}

		/// <summary>
		/// Delete User by Name
		/// </summary>
		/// <param name="name"></param>
		/// <returns>Success</returns>
		public bool DeleteUserByName(string username)
		{
			bool result = false;

			foreach (User user in Users)
			{
				if (user.Name.ToLower() == username.ToLower())
				{
					result = DeleteUserById(user.Id);
					break;
				}
			}			

			return result;
		}


		public bool GetPSUs()
		{
			bool result = false;

			APIResponse apiresponse = _connect.Get("/psus", Connect.Method.GET);
			if (apiresponse.StatusCode == 200)
			{
				API_PSUs apipsus = JSON.Deserialize<API_PSUs>(apiresponse.Response);

				if (apipsus != null)
				{
					List<PSU> tmppsus = new List<PSU>();
					foreach (API_PSUs.Data data in apipsus.data)
					{
						PSU tmp = new PSU();
						tmp.Id = data.id;
						tmp.Number = data.attributes.psuNumber;
						tmp.Status = data.attributes.status;
						tmp.InternalTemperature = data.attributes.internalTemperature;
						tmp.AmbientTemperature = data.attributes.ambientTemperature;
						tmp.FanSpeed = data.attributes.fanSpeed;
						tmp.MainsCurrent = data.attributes.mainsCurrent;
						tmp.OutputCurrent = data.attributes.outputCurrent;
						tmppsus.Add(tmp);
					}

					foreach (PSU psu in tmppsus)
					{
						if (!PSUs.Contains(psu))
						{
							int index = -1;

							for (int i = 0; i < PSUs.Count; i++)
							{
								if (PSUs[i].Id == psu.Id) index = i;
							}

							if (index > -1)
								PSUs[index] = psu;
							else
								PSUs.Add(psu);
						}
					}
					result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Indentify the Unit By Flashing the LED's
		/// </summary>
		/// <param name="times"></param>
		/// <returns>Success</returns>
		public bool Indentify(int times)
		{
			bool result = false;
			string cmd = "{ \"repeat\": "+ times.ToString() + " }";
			APIResponse apiresponse = _connect.Get("/system/identify", Connect.Method.POST, cmd);

			if (apiresponse.StatusCode == 204)
				result = true;
			else
				ErrorEvent(this, new EventArgs());

			return result;
		}

		/// <summary>
		/// Indentify the Unit By Flashing the LED's
		/// </summary>
		/// <returns>Success</returns>
		public bool IndentifyOn()
		{
			return Indentify(-1);
		}

		/// <summary>
		/// Stop Flashing the LED's that Indentify the unit.
		/// </summary>
		/// <returns>Succes</returns>
		public bool IndentifyOff()
		{
			return Indentify(0);
		}

		private void OnTimerGetUpdate(object sender, ElapsedEventArgs e)
		{
			GetData();
		}

		#region EventHandlers
		private void CollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			//Add
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				if (e.NewItems != null)
				{
					foreach (object item in e.NewItems)
					{						
						if (item.GetType() == typeof(User))
						{
							User user = (User)item;
							OnUserAdded?.Invoke(user, new EventArgs());
						}
					}
				}
			}

			//Remove
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				if (e.OldItems != null)
				{
					foreach (object item in e.OldItems)
					{
						if (item.GetType() == typeof(User))
						{
							User user = (User)item;
							OnUserRemoved?.Invoke(user, new EventArgs());
						}
					}
				}
			}

			//Updates
			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (e.NewItems != null)
				{
					foreach (Object item in e.NewItems)
					{
						if (item.GetType() == typeof(Output))
						{
							Output output = (Output)item;
							OnOutputUpdated?.Invoke(this, new EventArgs());
						}
						if (item.GetType() == typeof(User))
						{
							User user = (User)item;
							OnUserUpdated?.Invoke(user, new EventArgs());
						}
						if (item.GetType() == typeof(PSU))
						{
							PSU psu = (PSU)item;
							OnPSUUpdated?.Invoke(psu, new EventArgs());
						}

					}
				}
			}
		}

		private void CollectionChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_context != null)
				Synchronise(this.CollectionChangedEventMainThread, e);
			else
				CollectionChanged(e);
		}
		private void CollectionChangedEventMainThread(object obj)
		{
			if (obj != null)
			{
				NotifyCollectionChangedEventArgs e = (NotifyCollectionChangedEventArgs)obj;
				CollectionChanged(e);
			}
		}

		private void NetworkSettingsEvent(Object sender, EventArgs e)
		{
			if (_context != null)
				Synchronise(this.NetworkSettingsMainThread, e);
			else
				OnNetworkUpdate?.Invoke(sender, e);
		}
		private void NetworkSettingsMainThread(object obj)
		{
			OnNetworkUpdate?.Invoke(this, (EventArgs)obj);
		}

		private void SystemSettingsEvent(Object sender, EventArgs e)
		{
			if (_context != null)
				Synchronise(this.SystemSettingsMainThread, e);
			else
				OnSystemUpdate?.Invoke(sender, e);
		}
		private void SystemSettingsMainThread(object obj)
		{
			OnSystemUpdate?.Invoke(this, (EventArgs)obj);
		}

		private void ErrorEvent(Object sender, EventArgs e)
		{
			if (_context != null)
				Synchronise(this.ErrorEventMainThread, e);
			else
				OnError?.Invoke(sender, e);
		}
		private void ErrorEventMainThread(object obj)
		{
			OnError?.Invoke(this, (EventArgs)obj);
		}

		private void CompleteEvent(Object sender, EventArgs e)
		{
			lock (this)
			{
				if (_context != null)
					Synchronise(this.CompleteEventMainThread, e);
				else
					if (OnComplete != null) OnComplete(this, e);
			}
		}
		private void CompleteEventMainThread(object obj)
		{
			OnComplete?.Invoke(this, (EventArgs)obj);
		}

		private void Synchronise(SendOrPostCallback handler, object argument)
		{
			if (_context != null)
				_context.Post(handler, argument);
		}
		#endregion
	}
}