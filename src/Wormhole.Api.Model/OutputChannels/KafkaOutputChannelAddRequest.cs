using System.ComponentModel.DataAnnotations;

namespace Wormhole.Api.Model.OutputChannels
{
    public class KafkaOutputChannelAddRequest : OutputChannelAddRequest
    {
        [Required]
        [RegularExpression(Constants.KafkaTopicRegex)]
        [MaxLength(Constants.KafkaTopicMaxLength)]
        public string TopicId { get; set; }
    }
}