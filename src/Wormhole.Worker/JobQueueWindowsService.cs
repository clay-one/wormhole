using System;
using System.ServiceProcess;
using Nebula;

namespace Wormhole.Worker
{
    public class JobQueueWindowsService : ServiceBase
    {
        private readonly NebulaContext _nebulaContext;

        public JobQueueWindowsService(NebulaContext context)
        {
            _nebulaContext = context;
            ServiceName = "Wormhole JobQueue Worker service ";
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Worker starting");
            _nebulaContext.StartWorkerService();
            Console.WriteLine("Worker started");

        }

        protected override void OnStop()
        {
            Console.WriteLine("Worker stopping");
            _nebulaContext.StopWorkerService();
            Console.WriteLine("Worker stopped");
        }
    }
}