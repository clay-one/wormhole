namespace Wormhole.Kafka
{
    public interface IDiagnosticProvider
    {
        string Name { get; }
        object CollectDiagnosticData();
    }
}