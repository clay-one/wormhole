using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wormhole.DomainModel;

namespace Wormhole.DataImplementation
{
    public interface IMessageLogDa
    {
        Task AddAsync(OutgoingMessageLog outgoingMessageLog);
        Task<IList<OutgoingMessageLog>>  FindAsync(string jobStepId);

    }
}