using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMCPCommandSet.Models.Common;
using RevitMCPSDK.API.Interfaces;
using System;
using System.Threading;

namespace RevitMCPCommandSet.Services
{
    public class GetProjectLocationEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        public AIResult<ProjectLocationInfo> Result { get; private set; }

        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            _resetEvent.Reset();
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                var uiDoc = app.ActiveUIDocument;
                var doc = uiDoc.Document;

                var siteLocation = doc.SiteLocation;
                var projectLocation = doc.ActiveProjectLocation;
                var position = projectLocation.GetProjectPosition(XYZ.Zero);

                var info = new ProjectLocationInfo
                {
                    Latitude = siteLocation.Latitude * 180.0 / Math.PI,
                    Longitude = siteLocation.Longitude * 180.0 / Math.PI,
                    SiteElevation = siteLocation.Elevation * 304.8,
                    TimeZone = siteLocation.TimeZone,
                    PlaceName = siteLocation.PlaceName ?? "",
                    EastWest = position.EastWest * 304.8,
                    NorthSouth = position.NorthSouth * 304.8,
                    Elevation = position.Elevation * 304.8,
                    Angle = position.Angle,
                };

                Result = new AIResult<ProjectLocationInfo>
                {
                    Success = true,
                    Message = "Successfully retrieved project location.",
                    Response = info,
                };
            }
            catch (Exception ex)
            {
                Result = new AIResult<ProjectLocationInfo>
                {
                    Success = false,
                    Message = $"Failed to get project location: {ex.Message}",
                };
            }
            finally
            {
                _resetEvent.Set();
            }
        }

        public string GetName()
        {
            return "获取项目地理位置信息";
        }
    }
}
