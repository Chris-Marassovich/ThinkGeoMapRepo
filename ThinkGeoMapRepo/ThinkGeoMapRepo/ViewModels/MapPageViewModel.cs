using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThinkGeo.Core;
using ThinkGeo.UI.XamarinForms;
using ThinkGeoMapRepo.Services;

namespace ThinkGeoMapRepo.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        /// <summary>
        /// A reference to the Map.
        /// I do this so I can have all map code in the VM instead of code behind.
        /// </summary>
        readonly private MapView MapView;
        private bool MapInitialised { get; set; } = false;
        private object MapLock = new object();

        //The Overlays and Layers (in their Hierarchy)
        public const string ShapesOverlay = nameof(ShapesOverlay);
        public const string ShapesLayer = nameof(ShapesLayer);

        public const string JobsPopupOverlay = nameof(JobsPopupOverlay);

        public const string JobsOverlay = nameof(JobsOverlay);
        public const string LocationLayer = nameof(LocationLayer);
        public const string TeamMemberDeviceLayer = nameof(TeamMemberDeviceLayer);

        //Feature Column Values - The Keys
        public const string ColumnValueKeyPopupContent = nameof(ColumnValueKeyPopupContent);

        public MapPageViewModel(MapView mapView)
        {
            Title = "ThinkGeo Map";
            MapView = mapView;
        }

        /// <summary>
        /// Perform basic map init.
        /// Intention is to be called from the map page OnAppearing.
        /// </summary>
        public void InitialiseMap()
        {
            lock (MapLock)
            {
                if (MapInitialised == false)
                {
                    // Set the map's unit of measurement to meters(Spherical Mercator)
                    MapView.MapUnit = GeographyUnit.Meter;

                    //OSM overlay impl
                    //Set the zoom level set on the map to make sure its compatible with the OSM zoom levels.
                    MapView.ZoomLevelSet = new OpenStreetMapsZoomLevelSet();

                    // Create a new overlay that will hold our new layer and add it to the map and set the tile size to match up with the OSM til size.
                    var layerOverlay = new LayerOverlay();
                    layerOverlay.TileType = TileType.SingleTile;
                    MapView.Overlays.Add(layerOverlay);

                    var openStreetMapLayer = new OpenStreetMapLayer();
                    layerOverlay.Layers.Add(openStreetMapLayer);
                    //end OSM overlay impl

                    // Init the various Overlays and their Layers
                    ShapeOverlayInitialise();
                    PopupOverlayInitialise();
                    JobsOverlayInitialise();

                    // Set the map extent
                    MapView.CurrentExtent = RectangleShape.ScaleUp(MapView.Overlays[JobsOverlay].GetBoundingBox(), 100).GetBoundingBox();

                    MapView.Refresh();
                    MapInitialised = true;
                }
            }
        }

        private void ShapeOverlayInitialise()
        {
            LayerOverlay layerOverlay = new LayerOverlay();
            // Create the layer that will store the drawn shapes
            InMemoryFeatureLayer featureLayer = new InMemoryFeatureLayer();

            // Add styles for the layer
            featureLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyle.CreateSimpleCircleStyle(GeoColors.Blue, 8, GeoColors.Black);
            featureLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle = LineStyle.CreateSimpleLineStyle(GeoColors.Blue, 4, true);
            featureLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle = AreaStyle.CreateSimpleAreaStyle(new GeoColor(40, GeoColors.Blue), GeoColors.Black);
            featureLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            layerOverlay.Layers.Add(ShapesLayer, featureLayer);
            MapView.Overlays.Add(ShapesOverlay, layerOverlay);
        }

        private void PopupOverlayInitialise()
        {
            PopupOverlay popupOverlay = new PopupOverlay();
            MapView.Overlays.Add(JobsPopupOverlay, popupOverlay);
        }

        public async void JobsOverlayInitialise()
        {
            LayerOverlay jobsOverlay = new LayerOverlay();
            var locationLayer = await CreateLocationsLayer();
            jobsOverlay.Layers.Add(LocationLayer, locationLayer);

            var teamMemberDeviceLayer = await CreateTeamMemberDeviceLayer();
            jobsOverlay.Layers.Add(TeamMemberDeviceLayer, teamMemberDeviceLayer);
            jobsOverlay.TileType = TileType.SingleTile;//Performs better and if using clustering prevents minor UI glitches for the style.

            MapView.Overlays.Add(JobsOverlay, jobsOverlay);
            jobsOverlay.Refresh();
        }

        private async Task<InMemoryFeatureLayer> CreateLocationsLayer()
        {
            InMemoryFeatureLayer locationLayer = new InMemoryFeatureLayer();

            var locationList = await DataStore.GetLocations();
            foreach (var item in locationList)
            {
                try
                {
                    var pointShape = CreatePointShape(item.Geometry);
                    Feature feature = new Feature(pointShape.GetWellKnownBinary(), item.LocationId);
                    feature.ColumnValues.Add(ColumnValueKeyPopupContent, item.Description);
                    locationLayer.InternalFeatures.Add(feature.Id, feature);
                }
                catch (Exception)
                {
                }
            }

            //Add a simple Point Style for now
            //PointStyle pointStyle;
            //try
            //{
            //    Stream pinTargetLocationPng = new IconService().GetIconEmbeddedResource("ThinkGeoMapRepo.Resources.PinTargetLocation.png");
            //    pointStyle = new PointStyle(new GeoImage(pinTargetLocationPng));
            //    pinTargetLocationPng?.Close();
            //}
            //catch (Exception) //Something went wrong so lets take a default PointStyle
            //{
            //    pointStyle = new PointStyle(PointSymbolType.Circle, 10, GeoBrushes.BrightRed, new GeoPen(GeoBrushes.White, 2));
            //}

            var pointStyleCluster = new PointStyle(PointSymbolType.Triangle, 70, GeoBrushes.DarkRed, new GeoPen(GeoBrushes.White, 2));
            var textStyleCluster = new TextStyle("FeatureCount", new GeoFont("Segoe UI", 12, DrawingFontStyles.Bold), GeoBrushes.White)
            {
                YOffsetInPixel = 1
            };
            var clusterPointStyle = new ClusterPointStyle(pointStyleCluster, textStyleCluster)
            {
                MinimumFeaturesPerCellToCluster = 2,
                //CellSize = 1000,
            };

            locationLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Clear();
            //locationLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(pointStyle);
            //clusterPointStyle.CustomStyles.Add(pointStyle);
            locationLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(clusterPointStyle);
            locationLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            return locationLayer;
        }

        private async Task<InMemoryFeatureLayer> CreateTeamMemberDeviceLayer()
        {
            InMemoryFeatureLayer teamMemberDeviceLayer = new InMemoryFeatureLayer();

            var teamMembersDeviceList = await DataStore.GetTeamMemberDevices();
            foreach (var item in teamMembersDeviceList)
            {
                try
                {
                    var pointShape = CreatePointShape(item.Geometry);
                    Feature feature = new Feature(pointShape.GetWellKnownBinary(), item.DeviceId);
                    feature.ColumnValues.Add(ColumnValueKeyPopupContent, item.Description);
                    teamMemberDeviceLayer.InternalFeatures.Add(feature.Id, feature);
                }
                catch (Exception)
                {
                }
            }

            //Add a simple Point Style for now
            //PointStyle pointStyle;
            //try
            //{
            //    Stream pinTeamPng = new IconService().GetIconEmbeddedResource("ThinkGeoMapRepo.Resources.PinTeam.png");
            //    pointStyle = new PointStyle(new GeoImage(pinTeamPng));
            //    pinTeamPng?.Close();
            //}
            //catch (Exception) //Something went wrong so lets take a default PointStyle
            //{
            //    pointStyle = new PointStyle(PointSymbolType.Circle, 20, GeoBrushes.Green, new GeoPen(GeoBrushes.White, 2));
            //}

            //var pointStyle = new PointStyle(PointSymbolType.Circle, 20, GeoBrushes.Green, new GeoPen(GeoBrushes.White, 2));//

            var pointStyleCluster = new PointStyle(PointSymbolType.Circle, 30, GeoBrushes.Green, new GeoPen(GeoBrushes.White, 2));
            var textStyleCluster = new TextStyle("FeatureCount", new GeoFont("Segoe UI", 12, DrawingFontStyles.Bold), GeoBrushes.White)
            {
                YOffsetInPixel = 1
            };
            var clusterPointStyle = new ClusterPointStyle(pointStyleCluster, textStyleCluster)
            {
                MinimumFeaturesPerCellToCluster = 2,
                //CellSize = 1000,
            };

            teamMemberDeviceLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Clear();
            //teamMemberDeviceLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(pointStyle);
            teamMemberDeviceLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(clusterPointStyle);
            teamMemberDeviceLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            return teamMemberDeviceLayer;
        }

        /// <summary>
        /// Given a IGeometryObject it will convert it to a PointShape
        /// along with any projection conversion required.
        /// </summary>
        /// <param name="geometryObject"></param>
        /// <returns></returns>
        private BaseShape CreatePointShape(IGeometryObject geometryObject)
        {
            //Create a new ProjectionConverter to convert between Decimal Degrees(4326) and Spherical Mercator(3857)
            ProjectionConverter projectionConverter = new ProjectionConverter(4326, 3857);
            projectionConverter.Open();
            var pointJson = JsonConvert.SerializeObject(geometryObject);
            var pointShape = projectionConverter.ConvertToExternalProjection(PointShape.CreateShapeFromGeoJson(pointJson));
            projectionConverter.Close();

            return pointShape;
        }

        #region MAP TAPPED STUFF
        public void OnMapView_MapSingleTap(object sender, ThinkGeo.UI.XamarinForms.TouchMapViewEventArgs e)
        {
            Feature selectedFeature = GetFeatureFromLocation(e.PointInWorldCoordinate);
            if (selectedFeature != null)
            {
                DisplayFeatureInfo(selectedFeature);
            }
        }

        private Feature GetFeatureFromLocation(PointShape location)
        {
            InMemoryFeatureLayer nearbyFeaturesLayer = new InMemoryFeatureLayer();
            Collection<Feature> featuresWithinClick;
            Collection<Feature> nearestfeatures;

            FeatureLayer locationLayer = MapView.FindFeatureLayer(LocationLayer);
            locationLayer.Open();
            featuresWithinClick = locationLayer.QueryTools.GetFeaturesWithinDistanceOf(location, GeographyUnit.Meter, DistanceUnit.Meter, 20, ReturningColumnsType.AllColumns);
            locationLayer.Close();
            nearbyFeaturesLayer.Open();
            foreach (var item in featuresWithinClick)
            {
                nearbyFeaturesLayer.InternalFeatures.Add(item.Id, item);
            }
            featuresWithinClick.Clear();
            FeatureLayer teamMemberDeviceLayer = MapView.FindFeatureLayer(TeamMemberDeviceLayer);
            teamMemberDeviceLayer.Open();
            featuresWithinClick = teamMemberDeviceLayer.QueryTools.GetFeaturesWithinDistanceOf(location, GeographyUnit.Meter, DistanceUnit.Meter, 20, ReturningColumnsType.AllColumns);
            teamMemberDeviceLayer.Close();
            foreach (var item in featuresWithinClick)
            {
                nearbyFeaturesLayer.InternalFeatures.Add(item.Id, item);
            }

            nearestfeatures = nearbyFeaturesLayer.QueryTools.GetFeaturesNearestTo(location, GeographyUnit.Meter, 1, ReturningColumnsType.AllColumns);
            nearbyFeaturesLayer.Close();

            return nearestfeatures.FirstOrDefault();
        }

        private void DisplayFeatureInfo(Feature feature)
        {
            PopupOverlay popupOverlay = (PopupOverlay)MapView.Overlays[JobsPopupOverlay];
            Popup popup = new Popup()
            {
                Position = feature.GetShape().GetCenterPoint(),
                Content = feature.ColumnValues[ColumnValueKeyPopupContent]
            };

            popupOverlay.Popups.Clear();
            popupOverlay.Popups.Add(popup);

            popupOverlay.Refresh();
        }
        #endregion
    }
}
