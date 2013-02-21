// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewCalculationConfiguration.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
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

using OpenGamma.Engine.Function.Resolver;
using OpenGamma.Engine.Value;
using OpenGamma.Util;

namespace OpenGamma.Engine.View
{
    public class ViewCalculationConfiguration
    {
        private readonly string _name;
        private readonly ValueProperties _defaultProperties;
        private readonly IResolutionRuleTransform _resolutionRuleTransform;
        private readonly DeltaDefinition _deltaDefinition;
        private readonly ISet<ValueRequirement> _specificRequirements = new HashSet<ValueRequirement>();
        private readonly IDictionary<string, ISet<Tuple<string, ValueProperties>>> _portfolioRequirementsBySecurityType = new Dictionary<string, ISet<Tuple<string, ValueProperties>>>();

        public ViewCalculationConfiguration(string name)
            : this(name, ValueProperties.Create(), new DeltaDefinition(null), null)
        {
        }

        public ViewCalculationConfiguration(string name, ValueProperties defaultProperties, DeltaDefinition deltaDefinition, IResolutionRuleTransform resolutionRuleTransform)
        {
            ArgumentChecker.NotNull(deltaDefinition, "deltaDefinition");
            _name = name;
            _defaultProperties = defaultProperties;
            _resolutionRuleTransform = resolutionRuleTransform;
            _deltaDefinition = deltaDefinition;
        }

        public string Name
        {
            get { return _name; }
        }

        public ValueProperties DefaultProperties
        {
            get { return _defaultProperties; }
        }

        public DeltaDefinition DeltaDefinition
        {
            get { return _deltaDefinition; }
        }

        public IResolutionRuleTransform ResolutionRuleTransform
        {
            get { return _resolutionRuleTransform; }
        }

        public ISet<ValueRequirement> SpecificRequirements
        {
            get { return _specificRequirements; }
        }

        public IDictionary<string, ISet<Tuple<string, ValueProperties>>> PortfolioRequirementsBySecurityType
        {
            get { return _portfolioRequirementsBySecurityType; }
        }

        public void AddPortfolioRequirement(string securityType, string requiredOutput, ValueProperties constraints = null)
        {
            ArgumentChecker.NotNull(securityType, "securityType");
            ArgumentChecker.NotNull(requiredOutput, "requiredOutput");
            if (constraints == null)
            {
                constraints = ValueProperties.Create();
            }
            AddPortfolioRequirements(securityType, new HashSet<Tuple<string, ValueProperties>> { Tuple.Create(requiredOutput, constraints) });
        }

        public void AddPortfolioRequirements(string securityType, IEnumerable<Tuple<string, ValueProperties>> requiredOutputs)
        {
            ArgumentChecker.NotNull(securityType, "securityType");
            ArgumentChecker.NotNull(requiredOutputs, "requiredOutputs");
            ISet<Tuple<string, ValueProperties>> secTypeRequirements;
            if (_portfolioRequirementsBySecurityType.ContainsKey(securityType))
            {
                secTypeRequirements = _portfolioRequirementsBySecurityType[securityType];
            }
            else
            {
                secTypeRequirements = new HashSet<Tuple<string, ValueProperties>>();
                _portfolioRequirementsBySecurityType[securityType] = secTypeRequirements;
            }
            foreach (Tuple<string, ValueProperties> requirement in requiredOutputs)
            {
                secTypeRequirements.Add(requirement);
            }
        }

        public void AddSpecificRequirement(ValueRequirement requirement)
        {
            ArgumentChecker.NotNull(requirement, "requirement");
            _specificRequirements.Add(requirement);
        }

        public void AddSpecificRequirements(IEnumerable<ValueRequirement> requirements)
        {
            ArgumentChecker.NotNull(requirements, "requirements");
            foreach (ValueRequirement requirement in requirements)
            {
                _specificRequirements.Add(requirement);
            }
        }

        public static ViewCalculationConfiguration FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var name = ffc.GetValue<string>("name");
            var defaultProperties = deserializer.FromField<ValueProperties>(ffc.GetByName("defaultProperties"));
            var deltaDefinition = deserializer.FromField<DeltaDefinition>(ffc.GetByName("deltaDefinition"));

            IResolutionRuleTransform transform = null;
            IFudgeField transformField = ffc.GetByName("resolutionRuleTransform");
            if (transformField != null)
            {
                transform = deserializer.FromField<IResolutionRuleTransform>(transformField);
            }

            var calcConfig = new ViewCalculationConfiguration(name, defaultProperties, deltaDefinition, transform);
            foreach (ValueRequirement specificRequirement in GetList<ValueRequirement>(ffc, "specificRequirement", deserializer))
            {
                calcConfig.AddSpecificRequirement(specificRequirement);
            }
            foreach (var portfolioReqField in ffc.GetAllByName("portfolioRequirementsBySecurityType"))
            {
                const string securitytypeKey = "securityType";
                var securityType = ((IFudgeFieldContainer)portfolioReqField.Value).GetValue<string>(securitytypeKey);
                var requirements = ((IFudgeFieldContainer) portfolioReqField.Value).GetAllByName("portfolioRequirement").Select(f => GetReqPair(f, deserializer));
                calcConfig.AddPortfolioRequirements(securityType, requirements); 
            }
            return calcConfig;
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
            var context = s.Context;

            var fudgeSerializer = new FudgeSerializer(context);

            calcConfigMsg.Add("name", Name);
            foreach (var securityTypeRequirements in PortfolioRequirementsBySecurityType)
            {
                FudgeMsg securityTypeRequirementsMsg = new FudgeMsg(context);
                securityTypeRequirementsMsg.Add("securityType", securityTypeRequirements.Key);
                foreach (var requirement in securityTypeRequirements.Value)
                {
                    var newMessage = context.NewMessage();
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

            var deltaDefnMessage = fudgeSerializer.SerializeToMsg(_deltaDefinition);
            calcConfigMsg.Add("deltaDefinition", deltaDefnMessage);

            if (ResolutionRuleTransform != null)
            {
                var transformMessage = fudgeSerializer.SerializeToMsg(ResolutionRuleTransform);
                calcConfigMsg.Add("resolutionRuleTransform", transformMessage);
            }
        }
    }
}