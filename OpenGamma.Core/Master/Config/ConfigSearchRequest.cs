// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigSearchRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Fudge;
using Fudge.Serialization;

using OpenGamma.Id;
using OpenGamma.Util;

namespace OpenGamma.Master.Config
{
    public class ConfigSearchRequest
    {
        private PagingRequest _pagingRequest = PagingRequest.All;
        private IList<ObjectId> _configIds;
        private string _name;
        private ConfigSearchSortOrder _sortOrder;

        public IList<ObjectId> ConfigIds
        {
            get { return _configIds; }
            set { _configIds = value; }
        }

        public void AddConfigId(ObjectId objectId)
        {
            if (_configIds == null)
            {
                _configIds = new List<ObjectId>();
            }
            else
            {
                _configIds.Add(objectId);
            }
        }

        public PagingRequest PagingRequest
        {
            get { return _pagingRequest; }
            set { _pagingRequest = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ConfigSearchSortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "pagingRequest", _pagingRequest);
            if (_name != null)
            {
                a.Add("name", _name);
            }

            if (_configIds != null)
            {
                s.WriteInline(a, "configIds", _configIds);
            }

            s.WriteInline(a, "sortOrder", _sortOrder);
        }
    }
}
