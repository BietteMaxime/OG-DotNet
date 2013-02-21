// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurfaceFittedSmileDataPoints.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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

namespace OpenGamma.Financial.Analytics.Volatility.Surface.Fitting
{
    public class SurfaceFittedSmileDataPoints
    {
        private readonly Dictionary<double, List<double>> _data;

        public SurfaceFittedSmileDataPoints(Dictionary<double, List<double>> data)
        {
            _data = data;
        }

        public Dictionary<double, List<double>> Data
        {
            get { return _data; }
        }

        public static readonly string T_FIELD_NAME = "t field";
        public static readonly string K_FIELD_NAME = "k field";

        public static SurfaceFittedSmileDataPoints FromFudgeMsg(IFudgeFieldContainer message, IFudgeDeserializer deserializer)
        {
            IList<IFudgeField> tFields = message.GetAllByName(T_FIELD_NAME);
            IList<IFudgeField> kFields = message.GetAllByName(K_FIELD_NAME);
            var map = new Dictionary<double, List<double>>();
            for (int i = 0; i < tFields.Count; i++)
            {
                var t = (double)tFields[i].Value;
                List<double> ks = GetList<double>(kFields[i]);
                map.Add(t, ks);
            }

            return new SurfaceFittedSmileDataPoints(map);
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
