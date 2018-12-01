using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IMessageLogDa
    {
        Task AddAsync(MessageLog messageLog);
    }
}