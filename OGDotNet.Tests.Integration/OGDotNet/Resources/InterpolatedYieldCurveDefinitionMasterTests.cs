using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Model.Resources;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;
using Currency=OGDotNet.Mappedtypes.Core.Common.Currency;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class InterpolatedYieldCurveDefinitionMasterTests : TestWithContextBase
    {
        [Fact]
        public void CanGet()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                Assert.NotNull(interpolatedYieldCurveDefinitionMaster);
            }
        }

        [Fact]
        public void CanAdd()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinition = new YieldCurveDefinition(Currency.GetInstance("USD"), "My very special curve" + Guid.NewGuid(), "dummo");
                var yieldCurveDefinitionDocument = new YieldCurveDefinitionDocument
                                                                                {
                                                                                    Definition = yieldCurveDefinition
                                                                                };
                var newDoc = interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);

                Assert.True(ReferenceEquals(newDoc, yieldCurveDefinitionDocument));
                Assert.True(ReferenceEquals(newDoc.Definition, yieldCurveDefinitionDocument.Definition));
                Assert.NotNull(yieldCurveDefinitionDocument.UniqueId);
            }
        }
        [Fact]
        public void CantAddTwice()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinition = new YieldCurveDefinition(Currency.GetInstance("USD"), "My very special curve" + Guid.NewGuid(), "dummo");
                var yieldCurveDefinitionDocument = new YieldCurveDefinitionDocument
                {
                    Definition = yieldCurveDefinition
                };
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                Assert.Throws<ArgumentException>(() => interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument));
                
            }
        }

        [Fact]
        public void CantAddOrUpdate()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinition = new YieldCurveDefinition(Currency.GetInstance("USD"), "My very special curve" + Guid.NewGuid(), "dummo");
                var yieldCurveDefinitionDocument = new YieldCurveDefinitionDocument()
                {
                    Definition = yieldCurveDefinition
                };
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);
            }
        }
        [Fact]
        public void CantAddOrUpdateAfterAdd()
        {
            using (RemoteClient remoteClient = Context.CreateUserClient())
            {
                InterpolatedYieldCurveDefinitionMaster interpolatedYieldCurveDefinitionMaster = remoteClient.InterpolatedYieldCurveDefinitionMaster;
                var yieldCurveDefinition = new YieldCurveDefinition(Currency.GetInstance("USD"), "My very special curve" + Guid.NewGuid(), "dummo");
                var yieldCurveDefinitionDocument = new YieldCurveDefinitionDocument()
                {
                    Definition = yieldCurveDefinition
                };
                interpolatedYieldCurveDefinitionMaster.Add(yieldCurveDefinitionDocument);
                interpolatedYieldCurveDefinitionMaster.AddOrUpdate(yieldCurveDefinitionDocument);

            }
        }
    }
}
