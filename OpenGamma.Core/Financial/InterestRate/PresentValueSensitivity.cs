// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PresentValueSensitivity.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Fudge;
using Fudge.Serialization;

namespace OpenGamma.Financial.InterestRate
{
    public class PresentValueSensitivity
    {
        private readonly Dictionary<string, List<Tuple<double, double>>> _sensitivities;

        private PresentValueSensitivity(Dictionary<string, List<Tuple<double, double>>> sensitivities)
        {
            _sensitivities = sensitivities;
        }

        public Dictionary<string, List<Tuple<double, double>>> Sensitivities
        {
            get { return _sensitivities; }
        }

        public static PresentValueSensitivity FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var data = new Dictionary<string, List<Tuple<double, double>>>();
            var curveNames = ffc.GetAllByName("curve name");
            var sensitivities = ffc.GetAllByName("sensitivities");
            int id = 0;
            foreach (var tuple in curveNames.Zip(sensitivities, Tuple.Create))
            {
                string curveName = new StringBuilder("TODO ").Append(id++).ToString(); // I believe the java is broken
                var listSensitivities = (IFudgeFieldContainer) tuple.Item2;
                var pairsFields = listSensitivities.GetAllByName(null);
                var tuples = pairsFields.Select(ReadPair).ToList();
                data.Add(curveName, tuples);
            }

            return new PresentValueSensitivity(data);
        }

        private static Tuple<double, double> ReadPair(IFudgeField f)
        {
            var fudgeFieldContainer = (IFudgeFieldContainer) f.Value;
            return Tuple.Create(fudgeFieldContainer.GetDouble(0).Value, fudgeFieldContainer.GetDouble(1).Value);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}
