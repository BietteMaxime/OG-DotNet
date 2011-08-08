//-----------------------------------------------------------------------
// <copyright file="CounterpartyImpl.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    class CounterpartyImpl : ICounterparty
    {
        private readonly ExternalId _counterpartyId;

        public CounterpartyImpl(ExternalId counterpartyId)
        {
            ArgumentChecker.NotNull(counterpartyId, "counterpartyId");
            _counterpartyId = counterpartyId;
        }

        public ExternalId ExternalId
        {
            get { return _counterpartyId; }
        }
    }
}
