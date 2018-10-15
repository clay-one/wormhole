using System.Collections.Generic;

namespace Wormhole.Kafka
{
    public class KafkaConfiguration
    {
        public const string SectionName = "Kafka";
        public const string BootstrapServersKey = "bootstrap.servers";
        public const string ConsumerAutoCommitIntervalMsKey = "auto.commit.interval.ms";
        public const string StatisticsIntervalMsKey = "statistics.interval.ms";
        public const string EnableAutoCommitKey = "enable.auto.commit";
        public const string DefaultTopicConfigKey = "default.topic.config";
        public string BootstrapServers { get; set; }
        public int ConsumerAutoCommitIntervalMs { get; set; }
        public int StatisticsIntervalMs { get; set; }
        public int EnableAutoCommit { get; set; }
        public Dictionary<string, object> DefaultTopicConfig { get; set; }
    }
}