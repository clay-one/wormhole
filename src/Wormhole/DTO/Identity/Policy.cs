using System.Collections.Generic;

namespace Wormhole.DTO.Identity
{
    public class Policy
    {
        public string Version { get; set; }
        public ClientCertificate Certificate { get; set; }
        public IList<string> ValidIps { get; set; }
    }
}