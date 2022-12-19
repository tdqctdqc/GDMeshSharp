using Godot;
using System;

public class PolygonBrushUi : Node
{
    private PolygonBrush _brush;
    private Label _brushTerrainName;
    private MenuButton _brushTerrainMenu;
    public void Setup(PolygonBrush brush)
    {
        _brush = brush;
        _brushTerrainMenu = (MenuButton) FindNode("BrushTerrainMenu");
        _brushTerrainName = (Label) FindNode("BrushTerrainName");
        _brushTerrainName.Text = brush.PaintingTerrain.Name;
        _brushTerrainMenu.GetPopup().Connect("index_pressed", this, nameof(SelectedBrushTerrain));
        var terrains = Terrain.Terrains;
        for (int i = 0; i < terrains.Count; i++)
        {
            var brushTerrain = terrains[i];
            _brushTerrainMenu.GetPopup().AddItem(brushTerrain.Name);
        }
    }

    private void SelectedBrushTerrain(int index)
    {
        var terrain = Terrain.Terrains[index];
        _brush.SetTerrain(terrain);
        _brushTerrainName.Text = terrain.Name;
    }
}
