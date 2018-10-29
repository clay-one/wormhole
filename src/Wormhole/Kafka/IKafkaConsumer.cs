using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Wormhole.Kafka
{
    public interface IKafkaConsumer<TKey, TValue>
    {
        void Dispose();
        //
        // Summary:
        //     Raised on critical errors, e.g. connection failures or all brokers down. Note
        //     that the client will try to automatically recover from errors - these errors
        //     should be seen as informational rather than catastrophic
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<Error> OnError;
        //
        // Summary:
        //     Raised when a consumed message has an error != NoError (both when Consume or
        //     Poll is used for polling). Also raised on deserialization errors.
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<Message> OnConsumeError;
        //
        // Summary:
        //     Raised on librdkafka statistics events. JSON formatted string as defined here:
        //     https://github.com/edenhill/librdkafka/wiki/Statistics
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<string> OnStatistics;
        //
        // Summary:
        //     Raised when there is information that should be logged.
        //
        // Remarks:
        //     Note: By default not many log messages are generated. You can specify one or
        //     more debug contexts using the 'debug' configuration property and a log level
        //     using the 'log_level' configuration property to enable more verbose logging,
        //     however you shouldn't typically need to do this. Warning: Log handlers are called
        //     spontaneously from internal librdkafka threads and the application must not call
        //     any Confluent.Kafka APIs from within a log handler or perform any prolonged operations.
        event EventHandler<LogMessage> OnLog;
        //
        // Summary:
        //     Raised to report the result of (automatic) offset commits. Not raised as a result
        //     of the use of the CommitAsync method.
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<CommittedOffsets> OnOffsetsCommitted;
        //
        // Summary:
        //     Raised when a partition assignment is revoked. You should typically call the
        //     Consumer.Unassign method in this handler.
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<List<TopicPartition>> OnPartitionsRevoked;
        //
        // Summary:
        //     Raised on new partition assignment. You should typically call the Consumer.Assign
        //     method in this handler.
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<List<TopicPartition>> OnPartitionsAssigned;
        //
        // Summary:
        //     Raised when the consumer reaches the end of a topic/partition it is reading from.
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<TopicPartitionOffset> OnPartitionEOF;
        //
        // Summary:
        //     Raised when a new message is avaiable for consumption. NOT raised when Consumer.Consume
        //     is used for polling (only when Consmer.Poll is used for polling). NOT raised
        //     when the message has an Error (OnConsumeError is raised in that case).
        //
        // Remarks:
        //     Executes on the same thread as every other Consumer event handler (except OnLog
        //     which may be called from an arbitrary thread).
        event EventHandler<Message<TKey, TValue>> OnMessage;

        //
        // Summary:
        //     Gets the (dynamic) group member id of this consumer (as set by the broker).
        string MemberId { get; }
        //
        // Summary:
        //     Gets the current partition subscription as set by Subscribe.
        List<string> Subscription { get; }

        //
        // Summary:
        //     Poll for new consumer events, including new messages ready to be consumed (which
        //     will trigger the OnMessage event). Blocks until a new event is available to be
        //     handled or the timeout period timeout has elapsed.
        //
        // Parameters:
        //   timeout:
        //     The maximum time to block. You should typically use a relatively short timout
        //     period because this operation cannot be cancelled.
        void Poll(TimeSpan timeout);

        //
        // Summary:
        //     Update the subscription set to a single topic. Any previous subscription will
        //     be unassigned and unsubscribed first.
        void Subscribe(string topic);

        /// <summary>
        ///     Update the subscription set to topics.
        /// 
        ///     Any previous subscription will be unassigned and unsubscribed first.
        /// 
        ///     The subscription set denotes the desired topics to consume and this
        ///     set is provided to the partition assignor (one of the elected group
        ///     members) for all clients which then uses the configured
        ///     partition.assignment.strategy to assign the subscription sets's
        ///     topics's partitions to the consumers, depending on their subscription.
        /// </summary>
        void Subscribe(IEnumerable<string> topics);

        //
        // Summary:
        //     Update the assignment set to partitions. The assignment set is the complete set
        //     of partitions to consume from and will replace any previous assignment.
        //
        // Parameters:
        //   partitions:
        //     The set of partitions to consume from. If an offset value of Offset.Invalid (-1001)
        //     is specified for a partition, consumption will resume from the last committed
        //     offset on that partition, or according to the 'auto.offset.reset' configuration
        //     parameter if no offsets have been committed yet.
        void Assign(IEnumerable<TopicPartitionOffset> partitions);
        //
        // Summary:
        //     Stop consumption and remove the current assignment.
        void Unassign();

        void SetDiagnostic(ConsumerDiagnostic consumerDiagnostic);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="onMessageEventHandler">Event Handler on Recive Payload</param>
        void Initialize(ICollection<KeyValuePair<string, object>> config, EventHandler<Message<Null, string>> onMessageEventHandler);
    }
}