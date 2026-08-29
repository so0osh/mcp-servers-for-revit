using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using RevitMCPCommandSet.Models.Common;
using RevitMCPCommandSet.Services;
using RevitMCPSDK.API.Base;
using RevitMCPSDK.API.Interfaces;
using System;
using System.Collections.Generic;

namespace RevitMCPCommandSet.Commands;

public class CreateToposolidCommand : ExternalEventCommandBase
{
    private CreateToposolidEventHandler _handler => (CreateToposolidEventHandler)Handler;

    /// <summary>
    ///     命令名称
    /// </summary>
    public override string CommandName => "create_toposolid";

    /// <param name="uiApp">Revit UIApplication</param>
    public CreateToposolidCommand(UIApplication uiApp)
        : base(new CreateToposolidEventHandler(), uiApp)
    {
    }

    public override object Execute(JObject parameters, string requestId)
    {
        try
        {
            List<ToposolidElement> data = parameters["data"].ToObject<List<ToposolidElement>>();
            if (data == null)
                throw new ArgumentNullException(nameof(data), "AI 传入数据为空");

            _handler.SetParameters(data);

            if (RaiseAndWaitForCompletion(10000))
                return _handler.Result;
            throw new TimeoutException("创建地形楼板操作超时");
        }
        catch (Exception ex)
        {
            throw new Exception($"创建地形楼板失败: {ex.Message}");
        }
    }
}
