using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using RevitMCPCommandSet.Services;
using RevitMCPSDK.API.Base;
using System;

namespace RevitMCPCommandSet.Commands.Access
{
    public class GetProjectLocationCommand : ExternalEventCommandBase
    {
        private GetProjectLocationEventHandler _handler => (GetProjectLocationEventHandler)Handler;

        public override string CommandName => "get_project_location";

        public GetProjectLocationCommand(UIApplication uiApp)
            : base(new GetProjectLocationEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            if (RaiseAndWaitForCompletion(10000))
            {
                return _handler.Result;
            }
            else
            {
                throw new TimeoutException("获取项目地理位置信息超时");
            }
        }
    }
}
