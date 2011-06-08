//-----------------------------------------------------------------------
// <copyright file="LiveDataStream.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine;
using OGDotNet.Mappedtypes.engine.depGraph.DependencyGraph;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.engine.View;
using OGDotNet.Mappedtypes.engine.View.Execution;
using OGDotNet.Mappedtypes.engine.View.listener;
using OGDotNet.Mappedtypes.financial.view;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context.MarketDataSnapshot
{
    public class LiveDataStream : DisposableBase, INotifyPropertyChanged
    {
        private const string CalcConfigName = "Default";
        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RemoteEngineContext _remoteEngineContext;
        private readonly ManualResetEventSlim _prepared = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _haveValues = new ManualResetEventSlim(false);

        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<Tuple<UniqueIdentifier, string>, double> _currentValues = new Dictionary<Tuple<UniqueIdentifier, string>, double>();

        private string _temporaryViewName;
        private RemoteClient _remoteClient;
        private RemoteViewClient _remoteViewClient;

        public LiveDataStream(ManageableMarketDataSnapshot snapshot, RemoteEngineContext remoteEngineContext)
        {
            _snapshot = snapshot;
            _remoteEngineContext = remoteEngineContext;
            ThreadPool.QueueUserWorkItem(Prepare);
        }

        private void Prepare(object state)
        {
            //TODO reinit when snapshot changes shape?
            try
            {
                List<Tuple<MarketDataValueSpecification, string>> marketDataValueSpecifications =
                    Wrap(_snapshot.GlobalValues).Concat(_snapshot.YieldCurves.SelectMany(yc => Wrap(yc.Value.Values))).Distinct().ToList();

                _remoteClient = _remoteEngineContext.CreateUserClient();

                var temporaryViewName = typeof(LiveDataStream).FullName + Guid.NewGuid() + _snapshot.BasisViewName;
                var viewDefinition = new ViewDefinition(temporaryViewName, new ResultModelDefinition(ResultOutputMode.TerminalOutputs), null, calculationConfigurationsByName: new Dictionary<string, ViewCalculationConfiguration> { { CalcConfigName, GetCalcConfig(marketDataValueSpecifications) } });

                _remoteClient.ViewDefinitionRepository.AddViewDefinition(new AddViewDefinitionRequest(
                                                                             viewDefinition));
                _temporaryViewName = temporaryViewName;

                _remoteViewClient = _remoteEngineContext.ViewProcessor.CreateClient();
                var eventViewResultListener = new EventViewResultListener();
                eventViewResultListener.CycleCompleted += (sender, e) => Update(e.FullResult.AllLiveData);
                _remoteViewClient.SetResultListener(eventViewResultListener);
                _remoteViewClient.AttachToViewProcess(temporaryViewName, ExecutionOptions.RealTime);
            }
            finally
            {
                _prepared.Set();
            }
        }

        private static IEnumerable<Tuple<MarketDataValueSpecification, string>> Wrap(ManageableUnstructuredMarketDataSnapshot manageableUnstructuredMarketDataSnapshot)
        {
            return manageableUnstructuredMarketDataSnapshot.Values.SelectMany(kvp => kvp.Value.Keys.SelectMany(kk => kvp.Value.Keys.Select(v => Tuple.Create(kvp.Key, v))));
        }

        private static ViewCalculationConfiguration GetCalcConfig(IEnumerable<Tuple<MarketDataValueSpecification, string>> marketDataValueSpecifications)
        {
            return new ViewCalculationConfiguration(CalcConfigName, marketDataValueSpecifications.Select(m => new ValueRequirement(m.Item2, new ComputationTargetSpecification(
                GetType(m.Item1),
                m.Item1.UniqueId
                ))), new Dictionary<string, HashSet<Tuple<string, ValueProperties>>>());
        }

        private static ComputationTargetType GetType(MarketDataValueSpecification m)
        {
            return EnumUtils<MarketDataValueType, ComputationTargetType>.ConvertTo(m.Type);
        }

        [IndexerName("Item")]
        public double? this[MarketDataValueSpecification spec, string valueName]
        {
            get
            {
                double ret;
                if (_currentValues.TryGetValue(Tuple.Create(spec.UniqueId, valueName), out ret))
                {
                    return ret;
                }
                else
                {
                    return null;
                }
            }
        }

        private void Update(IEnumerable<ComputedValue> allLiveData)
        {
            _currentValues = allLiveData.ToDictionary(cv => Tuple.Create(cv.Specification.TargetSpecification.Uid, cv.Specification.ValueName), cv => (double)cv.Value);
            _haveValues.Set();
            InvokePropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }

        private void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _prepared.Wait();

                try
                {
                    _remoteClient.ViewDefinitionRepository.RemoveViewDefinition(_temporaryViewName);
                }
                catch (DataNotFoundException)
                {
                    // This is fine
                }
                if (_remoteClient != null)
                {
                    _remoteClient.Dispose();
                }

                if (_remoteViewClient != null)
                {
                    _remoteViewClient.Dispose();
                }
            }
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(ManageableMarketDataSnapshot snapshot)
        {
            //TODO handle view changing shape, or its compilation changing
            _haveValues.Wait();
            var snapshotValues = _currentValues;  // Take a point in time copy

            return new ManageableMarketDataSnapshot(snapshot.BasisViewName,
                GetNewForUpdate(snapshot.GlobalValues, snapshotValues),
                snapshot.YieldCurves.ToDictionary(kvp => kvp.Key, kvp => GetNewForUpdate(kvp.Value, snapshotValues)));
        }

        private static ManageableYieldCurveSnapshot GetNewForUpdate(ManageableYieldCurveSnapshot globalValues, Dictionary<Tuple<UniqueIdentifier, string>, double> snapshotValues)
        {
            return new ManageableYieldCurveSnapshot(GetNewForUpdate(globalValues.Values, snapshotValues), globalValues.ValuationTime); //TODO valuationTime
        }

        private static ManageableUnstructuredMarketDataSnapshot GetNewForUpdate(ManageableUnstructuredMarketDataSnapshot globalValues, Dictionary<Tuple<UniqueIdentifier, string>, double> snapshotValues)
        {
            return new ManageableUnstructuredMarketDataSnapshot(
                globalValues.Values.ToDictionary(k => k.Key, k => GetNewForUpdate(k.Key, k.Value, snapshotValues)));
        }

        private static IDictionary<string, ValueSnapshot> GetNewForUpdate(MarketDataValueSpecification spec, IDictionary<string, ValueSnapshot> values, Dictionary<Tuple<UniqueIdentifier, string>, double> snapshotValues)
        {
            var ret = new Dictionary<string, ValueSnapshot>();
            foreach (var valueSnapshot in values)
            {
                double marketValue;
                if (snapshotValues.TryGetValue(Tuple.Create(spec.UniqueId, valueSnapshot.Key), out marketValue))
                {
                    ret.Add(valueSnapshot.Key, new ValueSnapshot(marketValue));
                }
            }
            return ret;
        }
    }
}
