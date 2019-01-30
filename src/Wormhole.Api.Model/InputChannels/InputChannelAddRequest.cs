using System.ComponentModel.DataAnnotations;

namespace Wormhole.Api.Model.InputChannels
{
    public class InputChannelAddRequest
    {
        [Required]
        [MaxLength(Constants.ExternalKeyMaxLength)]
        [RegularExpression(Constants.ExternalKeyRegex)]
        public string ExternalKey { get; set; }

        [Required]
        [MaxLength(Constants.TenantIdMaxLength)]
        [RegularExpression(Constants.TenantIdRegex)]
        public string TenantId { get; set; }
      
        public ChannelType ChannelType { get; set; }
        public InputChannelSpecification TypeSpecification { get; set; }
    }
}