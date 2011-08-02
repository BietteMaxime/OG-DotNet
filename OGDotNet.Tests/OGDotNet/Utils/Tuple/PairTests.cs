//-----------------------------------------------------------------------
// <copyright file="PairTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Util.Time;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Model;
using Xunit;

namespace OGDotNet.Tests.OGDotNet.Utils.Tuple
{
    public class PairTests
    {
        readonly OpenGammaFudgeContext _fudgeContext = new OpenGammaFudgeContext();
        
        [Fact]
        public void CanRoundTrip()
        {
            //TODO: should be theory
            CheckRoundTrip(Pair.Create(1, 2), true);
            CheckRoundTrip(Pair.Create((long) 1, 2), true);

            CheckRoundTrip(Pair.Create((long)1, (long)2));
            CheckRoundTrip(Pair.Create((long) 1, "X"));

            CheckRoundTrip(Pair.Create(new Tenor("1D"), "X"));
        }

        private void CheckRoundTrip<TFirst, TSecond>(Pair<TFirst, TSecond> orig, bool skipNonGenericCheck = false)
        {
            CheckRoundTripImpl(orig, skipNonGenericCheck);
            CheckRoundTripImpl(Pair.Create(orig.Second, orig.First), skipNonGenericCheck);
        }

        private void CheckRoundTripImpl<TFirst, TSecond>(Pair<TFirst, TSecond> orig, bool skipNonGenericCheck = false)
        {
            var fudgeSerializer = _fudgeContext.GetSerializer();
            var message = fudgeSerializer.SerializeToMsg(orig);
            
            CheckRoundTrip(fudgeSerializer, orig, message);

            if (!skipNonGenericCheck)
            {
                var nonGenericMessage = new FudgeMsg(message.GetAllFields().Where(f => f.Ordinal != 0 || !((string)f.Value).Contains('`')).ToArray());
                CheckRoundTrip(fudgeSerializer, orig, nonGenericMessage);
            }
        }

        private static void CheckRoundTrip<TFirst, TSecond>(FudgeSerializer fudgeSerializer, Pair<TFirst, TSecond> orig, FudgeMsg message)
        {
            var genericRound = fudgeSerializer.Deserialize<Pair<TFirst, TSecond>>(message);
            Assert.Equal(orig.First, genericRound.First);
            Assert.Equal(orig.Second, genericRound.Second);

            var nonGenericRound = (Pair<TFirst, TSecond>)fudgeSerializer.Deserialize<Pair>(message);

            Assert.Equal(orig.First, nonGenericRound.First);
            Assert.Equal(orig.Second, nonGenericRound.Second);
        }
    }
}
