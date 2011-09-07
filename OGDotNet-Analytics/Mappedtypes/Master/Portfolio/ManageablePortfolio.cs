//-----------------------------------------------------------------------
// <copyright file="ManageablePortfolio.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class ManageablePortfolio
    {
        //TODO this
        private readonly string _name;
        private readonly UniqueId _uniqueId;

        public ManageablePortfolio(string name, UniqueId uniqueId)
        {
            _name = name;
            _uniqueId = uniqueId;
        }

        public string Name
        {
            get { return _name; }
        }

        public UniqueId UniqueId
        {
            get { return _uniqueId; }
        }

        public static ManageablePortfolio FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new ManageablePortfolio(ffc.GetString("name"), UniqueId.Parse(ffc.GetString("uniqueId")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}