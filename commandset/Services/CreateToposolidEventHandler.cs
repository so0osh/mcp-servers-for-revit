using Autodesk.Revit.UI;
using RevitMCPCommandSet.Models.Common;
using RevitMCPCommandSet.Utils;
using RevitMCPSDK.API.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
#if REVIT2027_OR_GREATER
using Autodesk.Revit.DB;
#endif

namespace RevitMCPCommandSet.Services;

/// <summary>
///     创建地形楼板 (Toposolid) 事件处理器
///     Revit 2027+ only - Toposolid API was introduced in Revit 2027, replacing TopographySurface.
/// </summary>
public class CreateToposolidEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
{
    private Autodesk.Revit.UI.UIApplication uiApp;
    private Autodesk.Revit.ApplicationServices.Application app => uiApp.Application;
    private Autodesk.Revit.DB.Document doc => uiApp.ActiveUIDocument.Document;

    /// <summary>
    ///     事件重置信号器
    /// </summary>
    private readonly System.Threading.ManualResetEvent _resetEvent = new(false);

    /// <summary>
    ///     执行结果（传出数据）
    /// </summary>
    public AIResult<List<int>> Result { get; private set; }

    private List<string> _warnings = new();

    private List<ToposolidElement> CreatedInfo { get; set; }

    public void SetParameters(List<ToposolidElement> data)
    {
        CreatedInfo = data;
        _resetEvent.Reset();
    }

    public void Execute(Autodesk.Revit.UI.UIApplication uiapp)
    {
        uiApp = uiapp;
        _warnings.Clear();
        try
        {
#if REVIT2027_OR_GREATER
            var elementIds = new List<int>();

            using (var transaction = new Autodesk.Revit.DB.Transaction(doc, "创建地形楼板"))
            {
                transaction.Start();

                foreach (var data in CreatedInfo)
                {
                    var requestedTypeId = data.TypeId;

                    Autodesk.Revit.DB.Level baseLevel = doc.FindNearestLevel(data.BaseLevel / 304.8);
                    if (baseLevel == null)
                        continue;

                    double baseOffset = (data.BaseOffset) / 304.8;

                    ToposolidType toposolidType = null;
                    if (requestedTypeId != -1 && requestedTypeId != 0)
                    {
                        var typeEleId = new Autodesk.Revit.DB.ElementId(requestedTypeId);
                        var typeEle = doc.GetElement(typeEleId);
                        toposolidType = typeEle as ToposolidType;
                        if (toposolidType == null)
                            _warnings.Add($"Requested toposolid typeId {requestedTypeId} not found or invalid. Falling back to first available type.");
                    }

                    if (toposolidType == null)
                    {
                        toposolidType = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                            .OfClass(typeof(ToposolidType))
                            .Cast<ToposolidType>()
                            .FirstOrDefault();
                    }

                    if (toposolidType == null)
                    {
                        _warnings.Add("No toposolid types available in project.");
                        continue;
                    }

                    var curveLoop = Autodesk.Revit.DB.CurveLoop.Create(
                        data.Boundary.OuterLoop.Select(l => JZLine.ToLine(l) as Autodesk.Revit.DB.Curve).ToList());
                    var curveLoops = new List<Autodesk.Revit.DB.CurveLoop> { curveLoop };

                    if (data.Boundary.InnerLoops != null)
                        foreach (var inner in data.Boundary.InnerLoops)
                            curveLoops.Add(Autodesk.Revit.DB.CurveLoop.Create(
                                inner.Select(l => JZLine.ToLine(l) as Autodesk.Revit.DB.Curve).ToList()));

                    var toposolid = Toposolid.Create(doc, curveLoops, toposolidType.Id, baseLevel.Id);

                    if (toposolid != null)
                    {
                        var offsetParam = toposolid.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.TOPOSOLID_HEIGHTABOVELEVEL_PARAM);
                        if (offsetParam != null && !offsetParam.IsReadOnly)
                            offsetParam.Set(baseOffset);

                        elementIds.Add(toposolid.Id.GetIntValue());
                    }
                }

                transaction.Commit();
            }

            string message = $"Successfully created {elementIds.Count} toposolid(s).";
            if (_warnings.Count > 0)
                message += "\n\nWarnings:\n • " + string.Join("\n • ", _warnings);

            Result = new AIResult<List<int>>
            {
                Success = true,
                Message = message,
                Response = elementIds
            };
#else
            Result = new AIResult<List<int>>
            {
                Success = false,
                Message = "Toposolid creation is only supported in Revit 2027 and later versions."
            };
#endif
        }
        catch (Exception ex)
        {
            Result = new AIResult<List<int>>
            {
                Success = false,
                Message = $"创建地形楼板时出错: {ex.Message}"
            };
        }
        finally
        {
            _resetEvent.Set(); // 通知等待线程操作已完成
        }
    }

    /// <summary>
    ///     等待外部事件执行完成
    /// </summary>
    /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
    /// <returns>操作是否在超时前完成</returns>
    public bool WaitForCompletion(int timeoutMilliseconds = 10000)
    {
        return _resetEvent.WaitOne(timeoutMilliseconds);
    }

    /// <summary>
    ///     实现IExternalEventHandler接口的GetName方法
    /// </summary>
    /// <returns>事件处理器名称</returns>
    public string GetName()
    {
        return "创建地形楼板";
    }
}
