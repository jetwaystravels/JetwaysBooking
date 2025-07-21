namespace OnionConsumeWebAPI.Models
{
	public class IPAddress
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public IPAddress(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}


		public string GetVisitorIPAddress()
		{
			var httpContext = _httpContextAccessor.HttpContext;
			string visitorIPAddress = string.Empty;
			if (httpContext != null)
			{
				visitorIPAddress = httpContext.Request.Headers["X-Forwarded-For"];

				// If the X-Forwarded-For header contains multiple IPs, take the first one
				if (!string.IsNullOrEmpty(visitorIPAddress))
				{
					visitorIPAddress = visitorIPAddress.Split(',')[0].Trim();
				}

				// Fallback to REMOTE_ADDR or UserHostAddress if X-Forwarded-For is empty
				if (string.IsNullOrEmpty(visitorIPAddress))
				{
					visitorIPAddress = httpContext.Connection.RemoteIpAddress?.ToString();
				}

				if (string.IsNullOrEmpty(visitorIPAddress) || visitorIPAddress.Trim() == "::1")
				{
					
					visitorIPAddress = string.Empty;
				}

				//return visitorIPAddress;
			}

			if (string.IsNullOrEmpty(visitorIPAddress))
			{
				//This is for Local(LAN) Connected ID Address
				string stringHostName = System.Net.Dns.GetHostName();
				//Get Ip Host Entry
				if (!string.IsNullOrEmpty(stringHostName))
				{
					System.Net.IPHostEntry ipHostEntries = System.Net.Dns.GetHostEntry(stringHostName);
					//Get Ip Address From The Ip Host Entry Address List
					System.Net.IPAddress[] arrIpAddress = ipHostEntries.AddressList;

					try
					{
						visitorIPAddress = arrIpAddress[arrIpAddress.Length - 2].ToString();
					}
					catch
					{
						try
						{
							visitorIPAddress = arrIpAddress[0].ToString();
						}
						catch
						{
							try
							{
								arrIpAddress = System.Net.Dns.GetHostAddresses(stringHostName);
								visitorIPAddress = arrIpAddress[0].ToString();
							}
							catch
							{
								visitorIPAddress = "127.0.0.1";
							}
						}
					}
				}
			}
			return visitorIPAddress;

			
		}
	}
}
