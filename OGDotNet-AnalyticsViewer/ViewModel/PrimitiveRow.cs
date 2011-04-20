//-----------------------------------------------------------------------
// <copyright file="PrimitiveRow.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.AnalyticsViewer.ViewModel
{
    public class PrimitiveRow : DynamicRow
    {
        private readonly UniqueIdentifier _targetId;

        public PrimitiveRow(UniqueIdentifier targetId)
        {
            _targetId = targetId;
        }

        public UniqueIdentifier TargetId
        {
            get { return _targetId; }
        }

        public string TargetName
        {
            get { return _targetId.ToString(); }//This is what the WebUI does
        }
    }
}