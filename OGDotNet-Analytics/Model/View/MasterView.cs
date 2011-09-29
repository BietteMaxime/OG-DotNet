//-----------------------------------------------------------------------
// <copyright file="MasterView.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OGDotNet.Mappedtypes;
using OGDotNet.Mappedtypes.Core.Change;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Util;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.View
{
    public static class MasterView
    {
        public static MasterView<T> Create<T>(IMaster<T> master) where T : AbstractDocument
        {
            return new MasterView<T>(master);
        }
    }
    public class MasterView<T> : DisposableBase, IChangeListener where T : AbstractDocument
    {
        private readonly object _lock = new object();
        private readonly IMaster<T> _master;
        private readonly RemoteChangeManger _changeManager;
        private readonly Dictionary<ObjectId, int> _indexOfObject;
        private readonly ObservableCollection<T> _documents;

        public MasterView(IMaster<T> master)
        {
            lock (_lock)
            {
                _master = master;
                _changeManager = _master.ChangeManager;
                _changeManager.AddChangeListener(this);
                _documents = new ObservableCollection<T>(GetAllDocuments());
                _indexOfObject = GetIndex();
            }
        }

        private IEnumerable<T> GetAllDocuments()
        {
            return _master.Search("*", PagingRequest.All).Documents;
        }

        private Dictionary<ObjectId, int> GetIndex()
        {
            return _documents.Select((d, i) => Tuple.Create(d.UniqueId.ObjectID, i)).ToDictionary(t => t.Item1, t => t.Item2);
        }

        public ObservableCollection<T> Documents
        {
            get { return _documents; }
        }

        public void EntityChanged(ChangeEvent changeEvent)
        {
            if (IsDisposed)
            {
                return;
            }
            //TODO sort?
            lock (_lock)
            {
                int index;
                switch (changeEvent.Type)
                {
                    case ChangeType.Added:
                        if (_indexOfObject.ContainsKey(changeEvent.AfterId.ObjectID))
                        {
                            throw new OpenGammaException("Added object already present");
                        }
                        _indexOfObject.Add(changeEvent.AfterId.ObjectID, _documents.Count);
                        _documents.Add(_master.Get(changeEvent.AfterId.ToLatest()));
                        break;
                    case ChangeType.Updated:
                        if (! _indexOfObject.TryGetValue(changeEvent.AfterId.ObjectID, out index))
                        {
                            throw new OpenGammaException("Updated object not present yet");
                        }
                        _documents[index] = _master.Get(changeEvent.AfterId.ToLatest());
                        break;
                    case ChangeType.Removed:
                        if (! _indexOfObject.TryGetValue(changeEvent.BeforeId.ObjectID, out index))
                        {
                            throw new OpenGammaException("Removed object not present yet");
                        }
                        _documents.RemoveAt(index);
                        break;
                    case ChangeType.Corrected:
                        //TODO
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _changeManager.RemoveChangeListener(this);
        }
    }
}
