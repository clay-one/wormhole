using System.ComponentModel.DataAnnotations;

namespace Wormhole.Api.Model
{
    public class OutputChannelAddRequest
    {

        [Required]
        [MaxLength(Constants.ExternalKeyMaxLength)]
        [RegularExpression(Constants.ExternalKeyRegex)]
        public string ExternalKey { get; set; }

        [Required]
        [MaxLength(Constants.TenantIdMaxLength)]
        [RegularExpression(Constants.TenantIdRegex)]
        public string TenantId { get; set; }

        [MaxLength(Constants.OutputMessageCategoryMaxLength)]
        [RegularExpression(Constants.OutputMessageCategoryRegex)]
        public string Category { get; set; }

        [MaxLength(Constants.OutputMessageTagMaxLength)]
        [RegularExpression(Constants.OutputMessageTagRegex)]
        public string Tag { get; set; }
    }
}