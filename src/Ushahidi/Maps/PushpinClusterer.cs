using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls.Maps;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;

namespace Ushahidi.Maps
{
  /// <summary>
  /// A utility class that manages the clustering of Pushpins
  /// </summary>
  public class PushpinClusterer : FrameworkElement
  {
    private Map _map;

    private List<Pushpin> _pins;

    private double DistanceThreshold = 50.0;
    
    public DataTemplate ClusterTemplate { get; private set; }

    /// <summary>
    /// Clusters the given pins on the supplied map
    /// </summary>
    public PushpinClusterer(Map map, List<Pushpin> pins, DataTemplate clusterTemplate)
    {
      _map = map;
      _pins = pins;
      ClusterTemplate = clusterTemplate;

      _map.ViewChangeEnd += (s, e) => RenderPins();
    }


    /// <summary>
    /// Re-render the pushpins based on the current zoom level
    /// </summary>
    private void RenderPins()
    {
      List<PushpinContainer> pinsToAdd = new List<PushpinContainer>();

      // consider each pin in turn
      foreach (var pin in _pins)
      {
        var newPinContainer = new PushpinContainer(pin,
          _map.LocationToViewportPoint(pin.Location));

        bool addNewPin = true;

        // determine how close they are to existing pins
        foreach(var pinContainer in pinsToAdd)
        {
          double distance = ComputeDistance(pinContainer.ScreenLocation, newPinContainer.ScreenLocation);

          // if the distance threshold is exceeded, do not add this pin, instead
          // add it to a cluster
          if (distance < DistanceThreshold)
          {
            pinContainer.Merge(newPinContainer);            
            addNewPin = false;
            break;
          }
        }

        if (addNewPin)
        {
          pinsToAdd.Add(newPinContainer);
        }
      }

      // asynchronously update the map
      _map.Dispatcher.BeginInvoke(() =>
        {
          _map.Children.Clear();
          foreach (var projectedPin in pinsToAdd.Where(pin => PointIsVisibleInMap(pin.ScreenLocation, _map)))
          {
            _map.Children.Add(projectedPin.GetElement(ClusterTemplate));
          }
        });

    }

    /// <summary>
    /// Gets whether the given point is within the map bounds
    /// </summary>
    private static bool PointIsVisibleInMap(Point point, Map map)
    {
      return point.X > 0 && point.X < map.ActualWidth &&
              point.Y > 0 && point.Y < map.ActualHeight;
    }

    /// <summary>
    /// Computes the cartesian distance between points
    /// </summary>
    private double ComputeDistance(Point p1, Point p2)
    {
      return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }
  }
}
