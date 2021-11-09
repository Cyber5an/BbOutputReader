using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sre
{

	class HostedService : IHostedService
	{
		#region constructor
		private readonly ILogger<HostedService> logger;
		private readonly ILogic logic;

		public HostedService(ILogger<HostedService> logger, ILogic logic)
		{
			this.logger = logger;
			this.logic = logic;
		}
		#endregion

		/// <summary>
		/// Przeniesione tutaj z Program.cs zeby testy mogly latwiej siegac do konfigurowania serwisow
		/// </summary>
		/// <param name="context"></param>
		/// <param name="services"></param>
		public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			services.Configure<AppSettings>(context.Configuration);
			services.AddHostedService<HostedService>();
			services.TryAddTransient<ILogic, Logic>();
		}

		/// <summary>
		/// Logger w tej metodzie jest domyslnie skonfigurowany tak, ze Information sie nie wyswietla, Error+ tak
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogTrace("Hosted service starting");
			await logic.RunAsync(cancellationToken);
		}
		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Hosted service stopping");

			return Task.CompletedTask;
		}
	}
}