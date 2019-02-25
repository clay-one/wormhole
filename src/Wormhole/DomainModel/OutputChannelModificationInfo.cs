namespace Wormhole.DomainModel
{
    public class OutputChannelModificationInfo
    {
        public string ExternalKey { get; set; }
        public string TargetUrl { get; set; }
        public OutputChannelModificationType ModificationType { get; set; }
    }

    public enum OutputChannelModificationType
    {
        ADD = 0,
        EDIT = 1,
        DELETE = 2
    }
}
