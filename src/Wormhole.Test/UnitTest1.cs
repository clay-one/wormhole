using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wormhole.DataImplementation;
using Wormhole.DomainModel;
using Wormhole.Utils;

namespace Wormhole.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            AppSettingsProvider.MongoConnectionString = "mongodb://mongodb1-a:22000/Wormhole";

            var kafkaOutputChannel = OutputChannelBuilder.CreateKafkaOutputChannel(externalKey:"ex",targetTopicId: "Messaging",
                tenantId: "Fanap.plus", category:"cat1", tag:"");

            var httpPushChannel = OutputChannelBuilder.CreateHttpPushOutputChannel(externalKey:"ex",
                targetUrl: "http://s1ghasedak01/jhsdf/sk", tenantId: "Fanap.plus", payloadOnly: false, category:"", tag:"");

            var outputChannelDa = new OutputChannelDA();
            await outputChannelDa.AddOutputChannel(kafkaOutputChannel);
            await outputChannelDa.AddOutputChannel(httpPushChannel);
            var channels = outputChannelDa.FindOutputChannels();
            var channel = channels.GetAwaiter().GetResult().Last();
        }
    }
}
