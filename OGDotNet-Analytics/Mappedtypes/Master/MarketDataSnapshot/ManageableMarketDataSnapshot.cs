//-----------------------------------------------------------------------
// <copyright file="ManageableMarketDataSnapshot.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.financial.analytics.Volatility.cube;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Context.MarketDataSnapshot.Warnings;
using OGDotNet.Utils;

namespace OGDotNet.Mappedtypes.Master.marketdatasnapshot
{
    public class ManageableMarketDataSnapshot : INotifyPropertyChanged, IUpdatableFrom<ManageableMarketDataSnapshot>, IUniqueIdentifiable
    {
        private readonly ManageableUnstructuredMarketDataSnapshot _globalValues;

        private readonly IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> _yieldCurves;
        private readonly IDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> _volatilityCubes;
        private readonly IDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> _volatilitySurfaces;

        private string _basisViewName;

        private UniqueIdentifier _uniqueId;

        public ManageableMarketDataSnapshot(string basisViewName, ManageableUnstructuredMarketDataSnapshot globalValues,
                                            IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> yieldCurves,
                                            IDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> volatilityCubes,
                                            IDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> volatilitySurfaces,
                                            UniqueIdentifier uniqueId = null)
        {
            _basisViewName = basisViewName;
            _globalValues = globalValues;
            _yieldCurves = new ConcurrentDictionary<YieldCurveKey, ManageableYieldCurveSnapshot>(yieldCurves);
            _volatilityCubes = new ConcurrentDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>(volatilityCubes);
            _volatilitySurfaces = new ConcurrentDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>(volatilitySurfaces);
            _uniqueId = uniqueId;
        }

        public UniqueIdentifier UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; }
        }

        public string Name { get; set; }

        public IDictionary<YieldCurveKey, ManageableYieldCurveSnapshot> YieldCurves
        {
            get { return _yieldCurves; }
        }

        public IDictionary<VolatilityCubeKey, ManageableVolatilityCubeSnapshot> VolatilityCubes
        {
            get { return _volatilityCubes; }
        }

        public IDictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> VolatilitySurfaces
        {
            get { return _volatilitySurfaces; }
        }

        public string BasisViewName
        {
            get
            {
                return _basisViewName;
            }
            set
            {
                _basisViewName = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("BasisViewName"));
            }
        }

        public ManageableUnstructuredMarketDataSnapshot GlobalValues
        {
            get { return _globalValues; }
        }

        public IDictionary<MarketDataValueSpecification, IDictionary<string, ValueSnapshot>> Values
        {
            get { return _globalValues.Values; }
        }

        public bool HaveOverrides()
        {
            return _globalValues.HaveOverrides() || _yieldCurves.Any(yc => yc.Value.HaveOverrides()) || _volatilityCubes.Any(vc => vc.Value.HaveOverrides()) || _volatilitySurfaces.Any(vc => vc.Value.HaveOverrides());
        }

        public void RemoveAllOverrides()
        {
            _globalValues.RemoveAllOverrides();

            foreach (var yieldCurveSnapshot in _yieldCurves.Values)
            {
                yieldCurveSnapshot.RemoveAllOverrides();
            }
            foreach (var volatilityCubeSnapshot in VolatilityCubes.Values)
            {
                volatilityCubeSnapshot.RemoveAllOverrides();
            }
            foreach (var surface in VolatilitySurfaces.Values)
            {
                surface.RemoveAllOverrides();
            }
        }

        public UpdateAction<ManageableMarketDataSnapshot> PrepareUpdateFrom(ManageableMarketDataSnapshot newSnapshot)
        {
            var globalUpdate = _globalValues.PrepareUpdateFrom(newSnapshot._globalValues);

            var ycActions = _yieldCurves.ProjectStructure(newSnapshot._yieldCurves,
                (k, va, vb) => va.PrepareUpdateFrom(vb).Wrap<ManageableMarketDataSnapshot>(s => s._yieldCurves[k]),
                PrepareCurveRemoveAction,
                PrepareCurveAddAction
                );

            var cubeActions = _volatilityCubes.ProjectStructure(newSnapshot._volatilityCubes,
                (k, va, vb) => va.PrepareUpdateFrom(vb).Wrap<ManageableMarketDataSnapshot>(s => s._volatilityCubes[k]),
                PrepareCubeRemoveAction,
                PrepareCubeAddAction
                );

            var surfaceActions = _volatilitySurfaces.ProjectStructure(newSnapshot._volatilitySurfaces,
                (k, va, vb) => va.PrepareUpdateFrom(vb).Wrap<ManageableMarketDataSnapshot>(s => s._volatilitySurfaces[k]),
                PrepareSurfaceRemoveAction,
                PrepareSurfaceAddAction
                );

            return globalUpdate.Wrap<ManageableMarketDataSnapshot>(s => s._globalValues)
                .Concat(UpdateAction<ManageableMarketDataSnapshot>.Of(ycActions))
                .Concat(UpdateAction<ManageableMarketDataSnapshot>.Of(cubeActions))
                .Concat(UpdateAction<ManageableMarketDataSnapshot>.Of(surfaceActions));
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareCurveRemoveAction(YieldCurveKey key, ManageableYieldCurveSnapshot value)
        {
            return new UpdateAction<ManageableMarketDataSnapshot>(
                delegate(ManageableMarketDataSnapshot snap)
                {
                    snap._yieldCurves.Remove(key);
                    snap.InvokePropertyChanged(new PropertyChangedEventArgs("YieldCurves"));
                },
                    OverriddenYieldCurveDisappearingWarning.Of(key, value)
                );
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareCurveAddAction(YieldCurveKey key, ManageableYieldCurveSnapshot value)
        {
            var manageableYieldCurveSnapshot = value.Clone();

            return new UpdateAction<ManageableMarketDataSnapshot>(
               delegate(ManageableMarketDataSnapshot snap)
               {
                   snap._yieldCurves.Add(key, manageableYieldCurveSnapshot.Clone());
                   snap.InvokePropertyChanged(new PropertyChangedEventArgs("YieldCurves"));
               }
               );
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareCubeRemoveAction(VolatilityCubeKey key, ManageableVolatilityCubeSnapshot value)
        {
            return new UpdateAction<ManageableMarketDataSnapshot>(
                delegate(ManageableMarketDataSnapshot snap)
                {
                    snap._volatilityCubes.Remove(key);
                    snap.InvokePropertyChanged(new PropertyChangedEventArgs("VolatilityCubes"));
                },
                    OverriddenVolatilityCubeDisappearingWarning.Of(key, value)
                );
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareCubeAddAction(VolatilityCubeKey key, ManageableVolatilityCubeSnapshot value)
        {
            var valueClone = value.Clone();

            return new UpdateAction<ManageableMarketDataSnapshot>(
               delegate(ManageableMarketDataSnapshot snap)
               {
                   snap._volatilityCubes.Add(key, valueClone.Clone());
                   snap.InvokePropertyChanged(new PropertyChangedEventArgs("VolatilityCubes"));
               }
               );
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareSurfaceRemoveAction(VolatilitySurfaceKey volatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot manageableVolatilitySurfaceSnapshot)
        {
            return new UpdateAction<ManageableMarketDataSnapshot>(
                delegate(ManageableMarketDataSnapshot snap)
                {
                    snap._volatilitySurfaces.Remove(volatilitySurfaceKey);
                    snap.InvokePropertyChanged(new PropertyChangedEventArgs("VolatilitySurfaces"));
                },
                    OverriddenVolatilitySurfaceDisappearingWarning.Of(volatilitySurfaceKey, manageableVolatilitySurfaceSnapshot)
                );
        }

        private static UpdateAction<ManageableMarketDataSnapshot> PrepareSurfaceAddAction(VolatilitySurfaceKey key, ManageableVolatilitySurfaceSnapshot value)
        {
            var valueClone = value.Clone();

            return new UpdateAction<ManageableMarketDataSnapshot>(
               delegate(ManageableMarketDataSnapshot snap)
               {
                   snap._volatilitySurfaces.Add(key, valueClone.Clone());
                   snap.InvokePropertyChanged(new PropertyChangedEventArgs("VolatilitySurfaces"));
               }
               );
        }

        public static ManageableMarketDataSnapshot FromFudgeMsg(IFudgeFieldContainer ffc,
                                                                IFudgeDeserializer deserializer)
        {
            var uidString = ffc.GetString("uniqueId");
            UniqueIdentifier uid = uidString == null ? null : UniqueIdentifier.Parse(uidString);

            var surfaceMessage = ffc.GetMessage("volatilitySurfaces");
            Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot> surfaces;
            if (surfaceMessage == null)
            {
                //Handle old format
                surfaces = new Dictionary<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>();
            }
            else
            {
                surfaces = MapBuilder.FromFudgeMsg<VolatilitySurfaceKey, ManageableVolatilitySurfaceSnapshot>(surfaceMessage, deserializer);
            }
            var manageableMarketDataSnapshot = new ManageableMarketDataSnapshot(
                ffc.GetString("basisViewName"),
                deserializer.FromField<ManageableUnstructuredMarketDataSnapshot>(ffc.GetByName("globalValues")),
                MapBuilder.FromFudgeMsg<YieldCurveKey, ManageableYieldCurveSnapshot>(ffc.GetMessage("yieldCurves"), deserializer),
                MapBuilder.FromFudgeMsg<VolatilityCubeKey, ManageableVolatilityCubeSnapshot>(ffc.GetMessage("volatilityCubes"), deserializer),
                surfaces,
                uid

                ) { Name = ffc.GetString("name") };

            return manageableMarketDataSnapshot;
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            s.WriteInline(a, "globalValues", _globalValues);

            a.Add("name", Name);
            a.Add("basisViewName", _basisViewName);
            if (_uniqueId != null)
            {
                a.Add("uniqueId", _uniqueId);
            }

            a.Add("yieldCurves", MapBuilder.ToFudgeMsg(s, _yieldCurves));
            a.Add("volatilityCubes", MapBuilder.ToFudgeMsg(s, _volatilityCubes));
            a.Add("volatilitySurfaces", MapBuilder.ToFudgeMsg(s, _volatilitySurfaces));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}