//-----------------------------------------------------------------------
// <copyright file="SecurityDocument.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master.Security
{
    public class SecurityDocument
    {
        private readonly UniqueIdentifier _uniqueId;
        private readonly ManageableSecurity _security;
        public UniqueIdentifier UniqueId { get { return _uniqueId; } }
        public ManageableSecurity Security { get { return _security; } }//TODO DOTNET-5: type this with proto replacement

        public SecurityDocument(UniqueIdentifier uniqueId, ManageableSecurity security)
        {
            _uniqueId = uniqueId;
            _security = security;
        }

        public override string ToString()
        {
            return Security.ToString();
        }

        public static SecurityDocument FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            return new SecurityDocument(UniqueIdentifier.Parse(ffc.GetString("uniqueId")), deserializer.FromField<ManageableSecurity>(ffc.GetByName("security")));
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}