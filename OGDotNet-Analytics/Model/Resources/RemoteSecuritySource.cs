using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;

using OGDotNet_Analytics.Mappedtypes.Core.Security;
using OGDotNet_Analytics.Mappedtypes.Id;

namespace OGDotNet_Analytics.Model.Resources
{
    public class RemoteSecuritySource
    {
        private readonly RestTarget _restTarget;

        public RemoteSecuritySource(RestTarget restTarget)
        {
            _restTarget = restTarget;
        }

        public Security GetSecurity(UniqueIdentifier uid)
        {//TODO use this
            var fudgeMsg = _restTarget.GetSubMagic("securities").GetSubMagic("security").GetSubMagic(uid.ToString()).GetReponse();
            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            return fudgeSerializer.Deserialize<Security> (fudgeMsg); 
        }

        private class OrderedComparison<T> : IEqualityComparer<List<T>>
        {
            public static readonly OrderedComparison<T> Instance = new OrderedComparison<T>();

            public bool Equals(List<T> x, List<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<T> obj)
            {
                return obj[0].GetHashCode();
            }
        }
        readonly Dictionary<List<Identifier>, List<Security>> _securitiesCache = new Dictionary<List<Identifier>, List<Security>>(OrderedComparison<Identifier>.Instance);

        public List<Security> GetSecurities(IEnumerable<Identifier> idEnum)
        {
            var ids= idEnum.ToList();

            List<Security> ret;
            if (_securitiesCache.TryGetValue(ids, out ret))
            {
                return ret;
            }


            var parameters = ids.Select(s => new Tuple<string,string>("id", s.ToString())).ToArray();
            var fudgeMsg = _restTarget.GetSubMagic("securities", parameters).GetReponse();

            var fudgeSerializer = FudgeConfig.GetFudgeSerializer();
            ret = fudgeMsg.GetAllByName("security").Select(f => f.Value).Cast<FudgeMsg>().Select(fudgeSerializer.Deserialize<Security>).ToList();


            _securitiesCache[ids] = ret;

            return ret.ToList();
        }
    }
}