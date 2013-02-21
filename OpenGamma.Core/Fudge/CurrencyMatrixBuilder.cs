// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CurrencyMatrixBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Financial.Ccy;
using OpenGamma.Id;

namespace OpenGamma.Fudge
{
    internal class CurrencyMatrixBuilder : BuilderBase<ICurrencyMatrix>
    {
        private const string UniqueIdFieldName = "uniqueId";
        private const string FixedRateFieldName = "fixedRate";
        private const string ValueRequirementsFieldName = "valueReq";
        private const string CrossConvertFieldName = "crossConvert";

        public CurrencyMatrixBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override ICurrencyMatrix DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var matrixImpl = new CurrencyMatrix();

            IFudgeField field = msg.GetByName(UniqueIdFieldName);
            if (field != null)
            {
                var uid = UniqueId.Parse((string)field.Value);
                matrixImpl.UniqueId = uid;
            }

            field = msg.GetByName(CrossConvertFieldName);
            if (field != null)
            {
                var crossconvert = (FudgeMsg) field.Value;
                matrixImpl.LoadCross(crossconvert);
            }

            field = msg.GetByName(FixedRateFieldName);
            if (field != null)
            {
                throw new NotImplementedException();
            }

            field = msg.GetByName(ValueRequirementsFieldName);
            if (field != null)
            {
                var value = (FudgeMsg) field.Value;
                matrixImpl.LoadReq(value, deserializer);
            }

            return matrixImpl;
        }
    }
}