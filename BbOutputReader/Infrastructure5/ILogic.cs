using System.Threading;
using System.Threading.Tasks;

namespace Sre
{
	public interface ILogic
	{
		Task RunAsync(CancellationToken cancellationToken);
	}
}