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
        private readonly ExternalId _identifier;

        public CounterpartyImpl(ExternalId identifier)
        {
            ArgumentChecker.NotNull(identifier, "identifier");
            _identifier = identifier;
        }

        public ExternalId Identifier
        {
            get { return _identifier; }
        }
    }
}
