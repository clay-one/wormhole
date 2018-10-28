using System.ComponentModel.DataAnnotations;

namespace Wormhole.Api.Model
{
    public class HttpPushOutputChannelAddRequest : OutputChannelAddRequest
    {
        [Required]
        [MaxLength(Constants.UrlMaxLength)]
        public string TargetUrl { get; set; }

        public bool PayloadOnly { get; set; }

    }
}