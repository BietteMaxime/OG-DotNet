//-----------------------------------------------------------------------
// <copyright file="ViewCalculationConfiguration.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.engine.value;

namespace OGDotNet.Mappedtypes.engine.View
{
    public class ViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly List<ValueRequirement> _specificRequirements;
        private readonly Dictionary<string, HashSet<Tuple<string, ValueProperties>>> _portfolioRequirementsBySecurityType;
        private readonly ValueProperties _defaultProperties;

        public ViewCalculationConfiguration(string name, List<ValueRequirement> specificRequirements, Dictionary<string, HashSet<Tuple<string, ValueProperties>>> portfolioRequirementsBySecurityType)
            : this(name, specificRequirements, portfolioRequirementsBySecurityType, ValueProperties.Create())
        {
        }
        public ViewCalculationConfiguration(string name, List<ValueRequirement> specificRequirements, Dictionary<string, HashSet<Tuple<string, ValueProperties>>> portfolioRequirementsBySecurityType, ValueProperties defaultProperties)
        {
            _name = name;
            _specificRequirements = specificRequirements;
            _portfolioRequirementsBySecurityType = portfolioRequirementsBySecurityType;
            _defaultProperties = defaultProperties;
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<ValueRequirement> SpecificRequirements
        {
            get { return _specificRequirements; }
        }

        public Dictionary<string, HashSet<Tuple<string, ValueProperties>>> PortfolioRequirementsBySecurityType
        {
            get { return _portfolioRequirementsBySecurityType; }
        }

        public ValueProperties DefaultProperties
        {
            get { return _defaultProperties; }
        }

        public static ViewCalculationConfiguration FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");

            List<ValueRequirement> specificRequirements = GetList<ValueRequirement>(ffc, "specificRequirement", deserializer);

            //TODO MAP deserializer by magic
            var portfolioRequirementsBySecurityType = new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>();
            foreach (var portfolioReqField in ffc.GetAllByName("portfolioRequirementsBySecurityType"))
            {
                const string securitytypeKey = "securityType";
                var securityType = ((IFudgeFieldContainer)portfolioReqField.Value).GetValue<string>(securitytypeKey);

                var enumerable = ((IFudgeFieldContainer) portfolioReqField.Value).GetAllByName("portfolioRequirement").Select(f => GetReqPair(f, deserializer));

                portfolioRequirementsBySecurityType.Add(securityType, new HashSet<Tuple<string, ValueProperties>>(enumerable) );    
            }

            var defaultProperties = deserializer.FromField<ValueProperties>(ffc.GetByName("defaultProperties"));

            return new ViewCalculationConfiguration(name, specificRequirements, portfolioRequirementsBySecurityType, defaultProperties);
        }

        private static Tuple<string, ValueProperties> GetReqPair(IFudgeField field, IFudgeDeserializer deserializer)
        {
            var ffc = (IFudgeFieldContainer) field.Value;
            string requiredOutput = ffc.GetString("requiredOutput");
            var constraints = deserializer.FromField<ValueProperties>(ffc.GetByName("constraints"));

            return Tuple.Create(requiredOutput, constraints);
        }

        private static List<T> GetList<T>(IFudgeFieldContainer ffc, string fieldName, IFudgeDeserializer deserializer) where T : class
        {
            return ffc.GetAllByName(fieldName).Select(deserializer.FromField<T>).ToList();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer calcConfigMsg, IFudgeSerializer s)
        {
            var fudgeSerializer = new FudgeSerializer(s.Context);


            calcConfigMsg.Add("name", Name);
            foreach (var securityTypeRequirements in PortfolioRequirementsBySecurityType)
            {
                FudgeMsg securityTypeRequirementsMsg = new FudgeMsg(s.Context);
                securityTypeRequirementsMsg.Add("securityType", securityTypeRequirements.Key);
                foreach (var requirement in securityTypeRequirements.Value)
                {
                    var newMessage = s.Context.NewMessage();
                    newMessage.Add("requiredOutput", requirement.Item1);
                    newMessage.Add("constraints", fudgeSerializer.SerializeToMsg(requirement.Item2));

                    securityTypeRequirementsMsg.Add("portfolioRequirement", newMessage);
                }

                calcConfigMsg.Add("portfolioRequirementsBySecurityType", securityTypeRequirementsMsg);
            }


            foreach (var specificRequirement in SpecificRequirements)
            {
                var sReqMsg = fudgeSerializer.SerializeToMsg(specificRequirement);
                calcConfigMsg.Add("specificRequirement", sReqMsg);

            }

            var defaultPropsMessage = fudgeSerializer.SerializeToMsg(DefaultProperties);
            calcConfigMsg.Add("defaultProperties", defaultPropsMessage);

            //TODO delta defn
            calcConfigMsg.Add("deltaDefinition", new FudgeMsg());
        }
    }
}