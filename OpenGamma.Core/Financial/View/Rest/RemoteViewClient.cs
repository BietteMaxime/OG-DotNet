﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoteViewClient.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Fudge;

using OpenGamma.Engine.View;
using OpenGamma.Engine.View.Calc;
using OpenGamma.Engine.View.Client;
using OpenGamma.Engine.View.Execution;
using OpenGamma.Engine.View.Listener;
using OpenGamma.Financial.View.Rest;
using OpenGamma.Fudge;
using OpenGamma.Id;
using OpenGamma.Model;
using OpenGamma.Model.Resources;
using OpenGamma.Util;
using OpenGamma.Util.PublicAPI;

namespace OpenGamma.Financial.view.rest
{
    /// <summary>
    /// See DataViewClientResource on the java side
    /// </summary>
    public class RemoteViewClient : RestfulJmsResultConsumerBase<IViewResultListener>
    {
        private readonly RemoteViewProcessor _viewProcessor;

        public RemoteViewClient(OpenGammaFudgeContext fudgeContext, RestTarget clientUri, MQTemplate mqTemplate, RemoteViewProcessor viewProcessor) : base(fudgeContext, clientUri, mqTemplate, (o, l) => new ResultEvent(o).ApplyTo(l))
        {
            _viewProcessor = viewProcessor;
        }

        public void Pause()
        {
            REST.Resolve("pause").Post();
        }

        public void Resume()
        {
            REST.Resolve("resume").Post();
        }

        public void TriggerCycle()
        {
            REST.Resolve("triggerCycle").Post();
        }

        public void SetUpdatePeriod(long periodMillis)
        {
            REST.Resolve("updatePeriod").Put(new FudgeMsg(FudgeContext, new Field("updatePeriod", periodMillis)));
        }

        public void SetResultMode(ViewResultMode mode)
        {
            var fudgeMsg = new FudgeMsg(FudgeContext) {{1, EnumBuilder<ViewResultMode>.GetJavaName(mode)}};
            REST.Resolve("resultMode").Put(fudgeMsg);
        }

        public void SetJobResultMode(ViewResultMode mode)
        {
            var fudgeMsg = new FudgeMsg(FudgeContext) { { 1, EnumBuilder<ViewResultMode>.GetJavaName(mode) } };
            REST.Resolve("jobResultMode").Put(fudgeMsg);
        }

        public bool IsAttached
        {
            get
            {
                var reponse = REST.Resolve("isAttached").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public void AttachToViewProcess(UniqueId viewDefinitionId, IViewExecutionOptions executionOptions, bool newBatchProcess = false)
        {
            ArgumentChecker.NotNull(viewDefinitionId, "viewDefinitionId");
            ArgumentChecker.NotNull(executionOptions, "executionOptions");
            var request = new AttachToViewProcessRequest(viewDefinitionId, executionOptions, newBatchProcess);
            REST.Resolve("attachSearch").Post(request);
        }

        public void AttachToViewProcess(UniqueId processId)
        {
            ArgumentChecker.NotNull(processId, "processId");

            REST.Resolve("attachDirect").Post(processId);
        }

        public void DetachFromViewProcess()
        {
            REST.Resolve("detach").Post();
        }

        public RemoteLiveDataInjector LiveDataOverrideInjector
        {
            get
            {
                return new RemoteLiveDataInjector(REST.Resolve("overrides"));
            }
        }

        public bool IsResultAvailable
        {
            get
            {
                var reponse = REST.Resolve("resultAvailable").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public bool IsCompleted
        {
            get
            {
                var reponse = REST.Resolve("completed").GetFudge();
                return 1 == (sbyte)reponse.GetByName("value").Value;
            }
        }

        public bool GetViewCycleAccessSupported()
        {
            var reponse = REST.Resolve("viewCycleAccessSupported").GetFudge();
            return 1 == (sbyte)reponse.GetByName("value").Value;
        }

        public void SetViewCycleAccessSupported(bool isViewCycleAccessSupported)
        {
            var msg = FudgeContext.NewMessage(
                new Field("isViewCycleAccessSupported", isViewCycleAccessSupported)
            );
            REST.Resolve("viewCycleAccessSupported").PostFudge(msg);
        }

        public IEngineResourceReference<IViewCycle> CreateLatestCycleReference()
        {
            return CreateCycleReference(null);
        }

        public IEngineResourceReference<IViewCycle> CreateCycleReference(UniqueId cycleId)
        {
            var location = REST.Resolve("createLatestCycleReference").Create(cycleId);
            return location == null ? null : new RemoteViewCycleReference(location);
        }

        public IViewComputationResultModel GetLatestResult()
        {
            return REST.Resolve("latestResult").Get<IViewComputationResultModel>();
        }

        public ViewDefinition GetViewDefinition()
        {
            return REST.Resolve("viewDefinition").Get<ViewDefinition>();
        }

        public VersionCorrection GetProcessVersionCorrection()
        {
            return REST.Resolve("processVersionCorrection").Get<VersionCorrection>();
        }

        public UniqueId GetUniqueId()
        {
            var restTarget = REST.Resolve("id");
            return restTarget.Get<UniqueId>();
        }

        public ViewClientState GetState()
        {
            var target = REST.Resolve("state");
            var msg = target.GetFudge();

            return EnumBuilder<ViewClientState>.Parse((string) msg.GetByOrdinal(1).Value);
        }
    }
}