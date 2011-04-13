//-----------------------------------------------------------------------
// <copyright file="Paging.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace OGDotNet.Mappedtypes.Util.Db
{
    [Serializable]
    public class Paging
    {

        public int Page;
        public int PagingSize;
        public int TotalItems;

        public int CurrentPage
        {
            get{return Page;}
        }
        public int Pages
        {
            get { return ((TotalItems -1)/PagingSize) + 1; }
        }
    }
}