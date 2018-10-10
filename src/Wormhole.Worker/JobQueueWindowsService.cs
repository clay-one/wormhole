using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.ServiceProcess;
using log4net;
using Nebula;

namespace Wormhole.Worker
{
    public class JobQueueWindowsService : ServiceBase
    {
        private readonly ILog Logger = LogManager.GetLogger(Assembly.GetCallingAssembly(),"start");
        private readonly NebulaContext _nebulaContext;

        public JobQueueWindowsService(NebulaContext context)
        {
            _nebulaContext = context;
            ServiceName = "Wormhole JobQueue Worker service ";
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Worker starting");
            _nebulaContext.StartWorkerService();
            Logger.Info("Worker started");

        }

        protected override void OnStop()
        {
            Logger.Info("Worker stopping");
            _nebulaContext.StopWorkerService();
            Logger.Info("Worker stopped");
        }
    }
}