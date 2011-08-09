//-----------------------------------------------------------------------
// <copyright file="RemoteViewCycle.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Engine.Value;
using OGDotNet.Mappedtypes.Engine.View;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Engine.View.Compilation;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util.Tuple;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    internal class RemoteViewCycle : IViewCycle
    {
        private readonly RestTarget _location;

        public RemoteViewCycle(RestTarget location)
        {
            _location = location;
        }

        public UniqueId UniqueId
        {
            get
            {
                return _location.Resolve("id").Get<UniqueId>();
            }
        }

        public ICompiledViewDefinitionWithGraphs GetCompiledViewDefinition()
        {
            return new RemoteCompiledViewDefinitionWithGraphs(_location.Resolve("compiledViewDefinition"));
        }

        public IViewComputationResultModel GetResultModel()
        {
            return _location.Resolve("result").Get<IViewComputationResultModel>();
        }

        public ComputationCacheResponse QueryComputationCaches(ComputationCacheQuery computationCacheQuery)
        {
            ArgumentChecker.NotNull(computationCacheQuery, "computationCacheQuery");
            ArgumentChecker.NotEmpty(computationCacheQuery.ValueSpecifications, "computationCacheQuery.ValueSpecifications");

            return _location.Resolve("queryCaches").Post<ComputationCacheResponse>(computationCacheQuery) ?? new ComputationCacheResponse(new List<Pair<ValueSpecification, object>>());
        }

        public UniqueId GetViewProcessId()
        {
            return _location.Resolve("viewProcessId").Get<UniqueId>();
        }

        public ViewCycleState GetState()
        {
            var fudgeMsg = _location.Resolve("state").GetFudge();
            return EnumBuilder<ViewCycleState>.Parse(fudgeMsg.GetString(1));
        }

        public long GetDurationNanos()
        {
            //TODO duration builder
            var fudgeMsg = _location.Resolve("duration").GetFudge();
            long seconds = -1;
            long nanos = -1;
            foreach (var field in fudgeMsg)
            {
                if (field.Ordinal == 0)
                {
                    continue;
                }
                switch (field.Name)
                {
                    case "seconds":
                        seconds = GetLong(field);
                        break;
                    case "nanos":
                        nanos = GetLong(field);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            if (seconds < 0 || nanos < 0)
            {
                throw new ArgumentException();
            }
            const long nanosPerSecond = 1000000000;
            return nanos + seconds * nanosPerSecond;
        }

        private static long GetLong(IFudgeField field)
        {
            return ((IConvertible)field.Value).ToInt64(null);
        }
    }
}