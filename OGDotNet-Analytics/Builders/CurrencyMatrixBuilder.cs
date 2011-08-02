//-----------------------------------------------------------------------
// <copyright file="CurrencyMatrixBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Financial.currency;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    internal class CurrencyMatrixBuilder : BuilderBase<ICurrencyMatrix>
    {
        private const string UniqueIDFieldName = "uniqueId";
        private const string FixedRateFieldName = "fixedRate";
        private const string ValueRequirementsFieldName = "valueReq";
        private const string CrossConvertFieldName = "crossConvert";

        public CurrencyMatrixBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        public override ICurrencyMatrix DeserializeImpl(IFudgeFieldContainer message, IFudgeDeserializer deserializer)
        {
            var matrixImpl = new CurrencyMatrix();

            IFudgeField field = message.GetByName(UniqueIDFieldName);
            if (field != null)
            {
                var uid = UniqueIdentifier.Parse((string)field.Value);
                matrixImpl.UniqueId = uid;
            }
            field = message.GetByName(CrossConvertFieldName);
            if (field != null)
            {
                var crossconvert = (FudgeMsg)field.Value;
                matrixImpl.LoadCross(crossconvert);
            }
            field = message.GetByName(FixedRateFieldName);
            if (field != null)
            {
                throw new NotImplementedException();
            }
            field = message.GetByName(ValueRequirementsFieldName);
            if (field != null)
            {
                FudgeMsg value = (FudgeMsg)field.Value;
                matrixImpl.LoadReq(value, deserializer);
            }

            return matrixImpl;
        }
    }
}