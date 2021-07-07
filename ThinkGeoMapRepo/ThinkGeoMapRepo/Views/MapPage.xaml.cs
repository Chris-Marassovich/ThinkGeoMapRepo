using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkGeo.Core;
using ThinkGeo.UI.XamarinForms;
using ThinkGeoMapRepo.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThinkGeoMapRepo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private const string ShapesOverlay = nameof(ShapesOverlay);
        private const string ShapesLayer = nameof(ShapesLayer);
        MapPageViewModel _viewModel;

        public MapPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new MapPageViewModel(mapView);

            instructions.Text = "Navigation Mode - The default map state. Allows you to pan and zoom the map with touch controls.";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.InitialiseMap();
        }

        private void navMode_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value == false) return;

            LayerOverlay shapesOverlay = (LayerOverlay)mapView.Overlays[ShapesOverlay];
            InMemoryFeatureLayer shapesLayer = (InMemoryFeatureLayer)shapesOverlay.Layers[ShapesLayer];

            // Update the layer's features from any previous mode
            UpdateShapeLayerFeatures(shapesLayer, shapesOverlay);

            // Set TrackMode to None, so that the user will no longer draw shapes and will be able to navigate the map normally
            mapView.TrackOverlay.TrackMode = TrackMode.None;

            instructions.Text = "Navigation Mode - The default map state. Allows you to pan and zoom the map with touch controls.";
        }

        private void drawPolygon_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value == false) return;

            LayerOverlay shapeOverlay = (LayerOverlay)mapView.Overlays[ShapesOverlay];
            InMemoryFeatureLayer shapeLayer = (InMemoryFeatureLayer)shapeOverlay.Layers[ShapesLayer];

            // Update the layer's features from any previous mode
            UpdateShapeLayerFeatures(shapeLayer, shapeOverlay);

            // Set TrackMode to Polygon, which draws a new polygon on the map on touch. Double taps to finish drawing the polygon.
            mapView.TrackOverlay.TrackMode = TrackMode.Polygon;

            // Update instructions
            instructions.Text = "Draw Polygon Mode - Begin creating a Polygon Shape by tapping on the map. Each subsequent tap adds another vertex to the polygon. Long tap to finish creating the Shape.";

        }

        private void editShape_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value == false) return;

            LayerOverlay shapeOverlay = (LayerOverlay)mapView.Overlays[ShapesOverlay];
            InMemoryFeatureLayer shapeLayer = (InMemoryFeatureLayer)shapeOverlay.Layers[ShapesLayer];

            // Update the layer's features from any previous mode
            UpdateShapeLayerFeatures(shapeLayer, shapeOverlay);

            // Set TrackMode to None, so that the user will no longer draw shapes
            mapView.TrackOverlay.TrackMode = TrackMode.None;

            // Put all features in the shapeLayer into the EditOverlay
            foreach (Feature feature in shapeLayer.InternalFeatures)
            {
                mapView.EditOverlay.EditShapesLayer.InternalFeatures.Add(feature.Id, feature);
            }

            // Clear all the features in the shapeLayer so that the editing features don't overlap with the original shapes
            // In UpdateShapeLayerFeatures, we will add them all back to the shapeLayer once the user switches modes
            shapeLayer.InternalFeatures.Clear();

            // This method draws all the handles and manipulation points on the map to edit. Essentially putting them all in edit mode.
            mapView.EditOverlay.CalculateAllControlPoints();

            // Refresh the map so that the features properly show that they are in edit mode
            mapView.EditOverlay.Refresh();
            shapeOverlay.Refresh();

            // Update instructions
            instructions.Text = "Edit Shapes Mode - Translate, rotate, or scale a shape using the anchor controls around the shape. Line and Polygon Shapes can also be modified: move a vertex by taping and dragging on an existing vertex, add a vertex by tapping on a line segment, and remove a vertex by double tapping on an existing vertex.";

        }

        private void deleteShape_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value == false) return;

            LayerOverlay shapeOverlay = (LayerOverlay)mapView.Overlays[ShapesOverlay];
            InMemoryFeatureLayer shapeLayer = (InMemoryFeatureLayer)shapeOverlay.Layers[ShapesLayer];

            // Update the layer's features from any previous mode
            UpdateShapeLayerFeatures(shapeLayer, shapeOverlay);

            // Set TrackMode to None, so that the user will no longer draw shapes
            mapView.TrackOverlay.TrackMode = TrackMode.None;

            // Add the event handler that will delete features on map tap
            mapView.MapSingleTap += MapView_SingleTapDeleteMode;

            // Update instructions
            instructions.Text = "Delete Shape Mode - Deletes a shape by tapping on the shape.";
        }

        /// <summary>
        /// Update the Shape layer whenever the user switches modes
        /// </summary>
        private void UpdateShapeLayerFeatures(InMemoryFeatureLayer featureLayer, LayerOverlay layerOverlay)
        {
            // If the user switched away from a Drawing Mode, add all the newly drawn shapes in the TrackOverlay into the shapeLayer
            foreach (Feature feature in mapView.TrackOverlay.TrackShapeLayer.InternalFeatures)
            {
                featureLayer.InternalFeatures.Add(feature.Id, feature);
            }

            // Clear out all the TrackOverlay's features
            mapView.TrackOverlay.TrackShapeLayer.InternalFeatures.Clear();

            // If the user switched away from Edit Mode, add all the shapes that were in the EditOverlay back into the shapeLayer
            foreach (Feature feature in mapView.EditOverlay.EditShapesLayer.InternalFeatures)
            {
                featureLayer.InternalFeatures.Add(feature.Id, feature);
            }

            // Clear out all the EditOverlay's features
            mapView.EditOverlay.EditShapesLayer.InternalFeatures.Clear();

            // Refresh the overlays to show latest results
            mapView.TrackOverlay.Refresh();
            mapView.EditOverlay.Refresh();
            layerOverlay.Refresh();

            // In case the user was in Delete Mode, remove the event handler to avoid deleting features unintentionally
            mapView.MapSingleTap -= MapView_SingleTapDeleteMode;
        }

        /// <summary>
        /// Event handler that finds the nearest feature and removes it from the shapeLayer
        /// </summary>
        private void MapView_SingleTapDeleteMode(object sender, TouchMapViewEventArgs e)
        {
            LayerOverlay shapeOverlay = (LayerOverlay)mapView.Overlays[ShapesOverlay];
            InMemoryFeatureLayer shapeLayer = (InMemoryFeatureLayer)shapeOverlay.Layers[ShapesLayer];

            // Query the layer for the closest feature within 100 meters
            Collection<Feature> closestFeatures = shapeLayer.QueryTools.GetFeaturesNearestTo(e.PointInWorldCoordinate, GeographyUnit.Meter, 1, new Collection<string>(), 100, DistanceUnit.Meter);

            // If a feature was found, remove it from the layer
            if (closestFeatures.Any())
            {
                shapeLayer.InternalFeatures.Remove(closestFeatures[0]);

                // Refresh the shapeOverlay to show the results
                shapeOverlay.Refresh();
            }
        }
    }
}