﻿using Grpc.Core;
using MediaContract;
using SIPSorcery.GB28181.Servers.SIPMessage;
using System.Threading.Tasks;
namespace GrpcAgent.WebsocketRpcServer
{
    public class SSMediaSessionImpl : VideoSession.VideoSessionBase
    {

        private MediaEventSource _eventSource = null;
        private SIPCoreMessageService _sipCoreMessageService = null;

        public SSMediaSessionImpl(MediaEventSource eventSource, SIPCoreMessageService sipCoreMessageService)
        {
            _eventSource = eventSource;
            _sipCoreMessageService = sipCoreMessageService;
        }

        public override Task<KeepAliveReply> KeepAlive(KeepAliveRequest request, ServerCallContext context)
        {
            ///TODO ....
            return base.KeepAlive(request, context);
        }



        public override Task<StartLiveReply> StartLive(StartLiveRequest request, ServerCallContext context)
        {
            _eventSource?.FireLivePlayRequestEvent(request, context);

            if (_sipCoreMessageService == null)
            {
                throw new System.Exception("instance not exist!");
            }

            if (_sipCoreMessageService.NodeMonitorService.ContainsKey(request.Gbid))
            {
                var targetService = _sipCoreMessageService.NodeMonitorService[request.Gbid];
                // make the real request
                targetService.RealVideoReq(new int[] { request.Port }, request.Ipaddr);

            }

            var result = Task.Factory.StartNew(() =>
                        {
                            //get the response .
                            var res = new StartLiveReply()
                            {
                                Ipaddr = "127.0.0.1",
                                Port = 50005
                            };

                            return res;


                        });
            return result;

            //_sipCoreMessageService.MonitorService[request.Gbid]
            // return base.StartLive(request, context);
        }

        public override Task<StartPlaybackReply> StartPlayback(StartPlaybackRequest request, ServerCallContext context)
        {
            if (request.IsDownload)
            {
                _eventSource?.FireDownloadRequestEvent(request, context);
            }
            else
            {
                _eventSource?.FirePlaybackRequestEvent(request, context);
            }

            return base.StartPlayback(request, context);
        }

        public override Task<StopReply> Stop(StopRequest request, ServerCallContext context)
        {
            return base.Stop(request, context);
        }
    }
}
