using Godot;
using System;
using System.Linq;

public class TriUnderMouse : Control
{
    private Label _id, _roughness, _polyId, _longestEdge, 
        _polyIsLand, _triIsLand, _triFaction, _landmass;
    private VBoxContainer _holder;
    public override void _Ready()
    {
        _holder = (VBoxContainer)FindNode("VBoxContainer");
        _id = new Label();
        _holder.AddChild(_id);
        _roughness = new Label();
        _holder.AddChild(_roughness);
        _polyId = new Label();
        _holder.AddChild(_polyId);
        _polyIsLand = new Label();
        _holder.AddChild(_polyIsLand);
        _triIsLand = new Label();
        _holder.AddChild(_triIsLand);
        _longestEdge = new Label();
        _holder.AddChild(_longestEdge);

        _triFaction = new Label();
        _holder.AddChild(_triFaction);

        _landmass = new Label();
        _holder.AddChild(_landmass);
    }

    public void Draw(Triangle tri, WorldManager world)
    {
        var polygons = world.GeologyPolygons;
        var geometry = world.Geometry;
        _id.Text = "ID: " + tri.Id.ToString();
        if (polygons.TrianglePolygons.ContainsKey(tri))
        {
            var poly = polygons.TrianglePolygons[tri];
            _polyId.Text = "Polygon: " + poly.Id;
            _polyIsLand.Text = poly.Info.IsLand ? "Land Poly" : "Water Poly";
        }
        else
        {
            _polyId.Text = "Polygon: None";
            _polyIsLand.Text = "";
        }

        var roughness = tri.Info.Roughness.OutValue.ToString();
        if (roughness.Length > 4) roughness = roughness.Remove(4);
        _roughness.Text = "Roughness: " + roughness;
        var length = tri.GetLongestEdgeLength(geometry).ToString();
        if (length.Length > 4) length = length.Remove(4);
        _longestEdge.Text = "Longest Edge: \n" + length;
        _triIsLand.Text = tri.Info.IsLand ? "Land Tri" : "Water Tri";


        if (tri.Info.Faction != null) _triFaction.Text = tri.Info.Faction.Name;
        else _triFaction.Text = "Neutral";


        if (world.Landmasses.LandMasses.Count == 0)
        {
            
        }
        else if (tri.Info.IsLand)
        {        
            var landmass = world.Landmasses.LandMasses
                .Where(l => l.Contains(tri))
                .First();
            _landmass.Text = $"Landmass {world.Landmasses.LandMasses.IndexOf(landmass)}";
        }
        else
        {        
            var landmass = world.Landmasses.WaterMasses
                .Where(l => l.Contains(tri))
                .First();
            _landmass.Text = $"Watermass {world.Landmasses.LandMasses.IndexOf(landmass)}";
        }
    }
    public void Clear()
    {
        _id.Text = "";
        _roughness.Text = "";
        _polyId.Text = "";
        _longestEdge.Text = "";
        _polyIsLand.Text = "";
        _triIsLand.Text = "";
        _landmass.Text = "";
    }
}
