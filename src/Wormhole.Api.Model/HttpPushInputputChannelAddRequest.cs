using System.ComponentModel.DataAnnotations;

namespace Wormhole.Api.Model
{
    public class HttpPushInputputChannelAddRequest
    {
        [Required]
        [MaxLength(Constants.ExternalKeyMaxLength)]
        [RegularExpression(Constants.ExternalKeyRegex)]
        public string ExternalKey { get; set; }

        [Required]
        [MaxLength(Constants.TenantIdMaxLength)]
        [RegularExpression(Constants.TenantIdRegex)]
        public string TenantId { get; set; }
    }
}