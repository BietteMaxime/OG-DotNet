// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeartbeatSenderTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using OpenGamma.Fudge;
using OpenGamma.Model;
using OpenGamma.Model.Resources;

using Xunit;

namespace OpenGamma
{
    public class HeartbeatSenderTests
    {
        [Fact]
        public void CanCreateAndDispose()
        {
            using (var heartbeatSender = new HeartbeatSender(TimeSpan.FromMilliseconds(int.MaxValue), new RestTarget(new OpenGammaFudgeContext(), new Uri("http://www.opengamma.com"))))
            {
            }
        }

        [Fact]
        public void CanCreateAndDisposeLots()
        {
            Parallel.For(0, 100, _ => CanCreateAndDispose());
        }
    }
}
