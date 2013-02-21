// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FittedSmileDataPoints.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Util.Time;
using OpenGamma.Util.Tuple;

namespace OpenGamma.Financial.Analytics.Volatility.Cube.Fitting
{
    /// <summary>
    /// TODO: this
    /// </summary>
    public class FittedSmileDataPoints
    {
        public static readonly string TENOR_PAIR_FIELD_NAME = "Tenor pairs";

        /** Field name */
        public static readonly string EXTERNAL_IDS_ARRAY_FIELD_NAME = "External ids";

        /** Field name */
        public static readonly string RELATIVE_STRIKES_ARRAY_FIELD_NAME = "Relative strikes";

        private readonly Dictionary<Pair<Tenor, Tenor>, ExternalId[]> _externalIds;

        private readonly Dictionary<Pair<Tenor, Tenor>, double[]> _relativeStrikes;

        public FittedSmileDataPoints(
            Dictionary<Pair<Tenor, Tenor>, ExternalId[]> externalIds, 
            Dictionary<Pair<Tenor, Tenor>, double[]> relativeStrikes)
        {
            _externalIds = externalIds;
            _relativeStrikes = relativeStrikes;
        }

        public Dictionary<Pair<Tenor, Tenor>, ExternalId[]> ExternalIds
        {
            get
            {
                return _externalIds;
            }
        }

        public Dictionary<Pair<Tenor, Tenor>, double[]> RelativeStrikes
        {
            get
            {
                return _relativeStrikes;
            }
        }

        public static FittedSmileDataPoints FromFudgeMsg(IFudgeFieldContainer message, IFudgeDeserializer deserializer)
        {
            var tenorPairFields = message.GetAllByName(TENOR_PAIR_FIELD_NAME);
            var externalIdsFields = message.GetAllByName(EXTERNAL_IDS_ARRAY_FIELD_NAME);
            var relativeStrikesFields = message.GetAllByName(RELATIVE_STRIKES_ARRAY_FIELD_NAME);
            var externalIds = new Dictionary<Pair<Tenor, Tenor>, ExternalId[]>();
            var relativeStrikes = new Dictionary<Pair<Tenor, Tenor>, double[]>();
            if (tenorPairFields.Count != externalIdsFields.Count || tenorPairFields.Count != relativeStrikesFields.Count)
            {
                throw new OpenGammaException("Should never happen");
            }

            for (int i = 0; i < tenorPairFields.Count; i++)
            {
                var tenors = deserializer.FromField<Pair<Tenor, Tenor>>(tenorPairFields[i]);
                var externalIdList = GetList<string>(externalIdsFields[i]).Select(ExternalId.Parse).ToList();
                var relativeStrikesList = GetList<double>(relativeStrikesFields[i]);
                externalIds.Add(tenors, externalIdList.ToArray());
                relativeStrikes.Add(tenors, relativeStrikesList.ToArray());
            }

            return new FittedSmileDataPoints(externalIds, relativeStrikes);
        }

        private static List<T> GetList<T>(IFudgeField field)
        {
            var msg = (IFudgeFieldContainer)field.Value;
            return msg.Where(f => f.Name == null && f.Ordinal == null).Select(f => f.Value).Cast<T>().ToList();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
