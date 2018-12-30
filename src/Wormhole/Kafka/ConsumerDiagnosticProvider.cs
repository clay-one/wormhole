using System.Collections.Concurrent;
using System.Linq;

namespace Wormhole.Kafka
{public class ConsumerDiagnosticProvider : IDiagnosticProvider
    {
        private static readonly ConcurrentDictionary<string, ConsumerDiagnostic> Dictionary = new ConcurrentDictionary<string, ConsumerDiagnostic>();

        public string Name => nameof(ConsumerDiagnosticProvider);
        public object CollectDiagnosticData()
        {
            return Dictionary.Values.ToList();
        }

        public static ConsumerDiagnostic GetStat(string fullClassName, string topicName)
        {
            var key = $"{fullClassName}-{topicName}";
            return Dictionary.GetOrAdd(key, s => new ConsumerDiagnostic(s));

        }
    }
}