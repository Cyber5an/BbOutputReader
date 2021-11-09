namespace Sre
{

	class Program
	{
		static int Main(string[] args)
		{
			(var host, var returnCode) = TryBuildHost(args);
			if (returnCode.HasValue) return returnCode.Value;
			if (host != null) return (int)TryRunHost(host!);
			Console.WriteLine($"Critical error. I should not be here. Host is null. Returning {(int)ErrorCode.FatalGeneralError}");
			return (int)ErrorCode.FatalGeneralError;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns>Ready Host OR error code to return out of app</returns>
		private static (IHost?, int?) TryBuildHost(string[] args)
		{
			IHost? host;
			try
			{
				host = Host
					.CreateDefaultBuilder(args) // methods below listed in order of execution
					.ConfigureHostConfiguration(configurationBuilder =>
					{
					})
					.ConfigureAppConfiguration(configurationBuilder =>
					{
						configurationBuilder.AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true);
						configurationBuilder.AddEnvironmentVariables("BO_PRICEGETTER_"); // defined env variables, eg BO_PRICEGETTER_VERSION - Version taken from AppSettings
				})
					.ConfigureLogging(loggingBuilder =>
					{
						loggingBuilder.AddSimpleConsole(options =>
						{
							options.IncludeScopes = false;
							options.SingleLine = true;
							options.TimestampFormat = "yyyy-MM-dd hh:mm:ss ";
							options.ColorBehavior = LoggerColorBehavior.Enabled;
						});
						loggingBuilder.AddConsole(x =>
						{

						});
					})
					.ConfigureServices((context, services) =>
					{
						HostedService.ConfigureServices(context, services);
					})
					.Build();

				return (host, null);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Exception during host initialisation, returning {ErrorCode.GeneralNotCatchedExceptionDuringHostBuild} {ex.Message}");
				return (null, (int)ErrorCode.GeneralNotCatchedExceptionDuringHostBuild);
			}
		}

		private static ErrorCode TryRunHost(IHost host)
		{
			try
			{
				var logger = host.Services.GetRequiredService<ILogger<Program>>();
				var env = host.Services.GetRequiredService<IHostEnvironment>();
				var appSettings = host.Services.GetRequiredService<IOptions<AppSettings>>().Value;
				logger.LogInformation($"Running host for application: {env.ApplicationName} {appSettings.Version} Environment: {env.EnvironmentName} Content root path: {env.ContentRootPath}");
				host.Run();
				logger.LogInformation($"Proper end of program, returning {(int)ErrorCode.NoError}");
				return ErrorCode.NoError;
			}
			catch (TaskCanceledException)
			{
				Console.WriteLine($"Program cancelled, most probably due to Ctrl-C. There was no proper software finish, returning {(int)ErrorCode.TaskCancelled}");
				return ErrorCode.TaskCancelled;
			}
			catch (Exception ex)
			{
				try
				{
					var logger = host.Services.GetRequiredService<ILogger<Program>>();
					logger.LogCritical($"General not catched exception during program execution, returning {(int)ErrorCode.GeneralNotCatchedExceptionDuringHostRun} {ex.GetType().Name} {ex.Message}");
					return ErrorCode.GeneralNotCatchedExceptionDuringHostRun;
				}
				catch (Exception inner)
				{
					Console.WriteLine($@"General not catched exception during program execution AND UNABLE TO GET LOGGER, 
						returning {(int)ErrorCode.GeneralNotCatchedExceptionAndUnableToGetLogger}. 
						Outer exception {ex.GetType().Name} {ex.Message}
						Inner exception {inner.GetType().Name} {inner.Message}");

					return ErrorCode.GeneralNotCatchedExceptionAndUnableToGetLogger;
				}
			}
		}
	}
}