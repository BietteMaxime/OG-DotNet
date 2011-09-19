//-----------------------------------------------------------------------
// <copyright file="PortfolioSearchRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Mappedtypes.Master.Portfolio
{
    public class PortfolioSearchRequest
    {
        //TODO: this
        private readonly PagingRequest _pagingRequest;
        private readonly List<ObjectId> _portfolioObjectIds;
        private readonly List<ObjectId> _nodeObjectIds;
        private readonly string _name;

        public PortfolioSearchRequest(PagingRequest pagingRequest, List<ObjectId> portfolioObjectIds, List<ObjectId> nodeObjectIds)
        {
            _pagingRequest = pagingRequest;
            _portfolioObjectIds = portfolioObjectIds;
            _nodeObjectIds = nodeObjectIds;
        }

        public PortfolioSearchRequest(PagingRequest pagingRequest, string name)
        {
            _pagingRequest = pagingRequest;
            _name = name;
        }

        public PagingRequest PagingRequest
        {
            get { return _pagingRequest; }
        }

        public string Name
        {
            get { return _name; }
        }

        public List<ObjectId> PortfolioObjectIds
        {
            get { return _portfolioObjectIds; }
        }

        public List<ObjectId> NodeObjectIds
        {
            get { return _nodeObjectIds; }
        }

        public static PortfolioSearchRequest FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            throw new NotImplementedException();
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "pagingRequest", _pagingRequest);
            if (_name != null)
            {
                a.Add("name", _name);
            }
            if (_portfolioObjectIds != null)
            {
                s.WriteInline(a, "portfolioObjectIds", _portfolioObjectIds);
            }
            if (_nodeObjectIds != null)
            {
                s.WriteInline(a, "nodeObjectIds", _nodeObjectIds);
            }
        }
    }
}
