using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SSLRegularCheck
{
	class Program
	{
		private static int ErrorCount { get; set; }

		static void Main(string[] args)
		{
			ErrorCount = 0;

			var domains = ConfigurationManager.AppSettings["domains"]
				.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
				.Where(x => x.Trim().Length > 0)
				.OrderBy(x => x);

			foreach (var domain in domains)
			{
				var request = WebRequest.CreateHttp($"https://{domain.Trim()}");
				request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;

				try
				{
					using (var response = request.GetResponse()) { }
				}
				catch (Exception ex)
				{
					ErrorCount += 1;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($" ERROR: {ex.Message}");
				}
			}

			if (ErrorCount > 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\r\n{ErrorCount} errors! Please check failing domains.");
				Console.ReadLine();
			}
		}

		private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write($"#{((HttpWebRequest)sender).Address}");

			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(" SSL OK");
				return true;
			}
			else
			{
				ErrorCount += 1;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(" SSL ERROR");
				return false;
			}
		}
	}
}