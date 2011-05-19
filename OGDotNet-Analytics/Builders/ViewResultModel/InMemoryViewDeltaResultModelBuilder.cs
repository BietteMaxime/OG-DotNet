//-----------------------------------------------------------------------
// <copyright file="InMemoryViewDeltaResultModelBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using Fudge.Types;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Utils;

namespace OGDotNet.Builders.ViewResultModel
{
    internal class InMemoryViewDeltaResultModelBuilder : InMemoryViewResultModelBuilderBase<InMemoryViewDeltaResultModel>
    {
        public InMemoryViewDeltaResultModelBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        protected override InMemoryViewDeltaResultModel BuildObject(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, Dictionary<string, ViewCalculationResultModel> configurationMap, UniqueIdentifier viewProcessId, UniqueIdentifier viewCycleId, DateTimeOffset inputDataTimestamp, DateTimeOffset resultTimestamp)
        {
            var previousResultTimestamp = msg.GetValue<FudgeDateTime>("previousTS").ToDateTimeOffsetWithDefault();
            return new InMemoryViewDeltaResultModel(viewProcessId, viewCycleId, inputDataTimestamp, resultTimestamp, configurationMap, previousResultTimestamp);
        }
    }
}