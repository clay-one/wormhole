namespace Wormhole.Kafka
{
    public class KafkaConfig
    {
        public const string SectionName = "Kafka";
        public const string BootstrapServersKey = "bootstrap.servers";
        public const string ConsumerAutoCommitIntervalMsKey = "auto.commit.interval.ms";
        public const string StatisticsIntervalMsKey = "statistics.interval.ms";
        public const string EnableAutoCommitKey = "enable.auto.commit";
        public const string DefaultTopicConfigKey = "default.topic.config";
        public string BootstrapServers { get; set; }
        public string ServerAddress { get; set; }
        public string ConsumerAutoCommitIntervalMs { get; set; }
        public string StatisticsIntervalMs { get; set; }
        public string EnableAutoCommit { get; set; }
    }
}
