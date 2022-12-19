using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class BuildCities : MapAction
{
    private static int _villagesPerCity = 3;
    private static float _roughnessCityScoreCost = 1.5f; //0 to 1
    private static float _cityScoreFromWater = .25f;
    private static float _scorePerCity = 100f;
    private static float _lowMountainChanceToHaveCity = .3f;
    public override void DoAction(WorldManager world)
    {
        BuildCitiesForPlates(world);
        world.Locations.BuildGraphs();
        world.Landmasses.BuildCityGraphs();
    }

    private void BuildCitiesForPlates(WorldManager world)
    {
        var plates = world.Plates;
        void buildCitiesForPlate(Plate plate)
        {
            var plateTris = plate.Cells
                .SelectMany(c => c.Triangles);
            var landTris = plateTris.Where(t => t.Info.IsLand).ToList();
            var possCityTris = landTris.Where(TriIsGoodForCity).ToList();
            var cityScore = plateTris.Sum(GetCityScore);
            int numCities = Mathf.FloorToInt(cityScore / _scorePerCity);
            int numUrban = numCities * (1 + _villagesPerCity);
            if (numUrban > possCityTris.Count)
            {
                numCities = Mathf.FloorToInt(possCityTris.Count / (1 + _villagesPerCity));
            }

            for (int i = 0; i < numCities; i++)
            {
                var cityTri = possCityTris.GetRandomElement();
                possCityTris.Remove(cityTri);
                cityTri.Info.SetTerrain(Terrain.DenseUrban);
                world.Locations.AddLocation(cityTri);
                for (int j = 0; j < _villagesPerCity; j++)
                {
                    var villageTri = possCityTris.GetRandomElement();
                    possCityTris.Remove(villageTri);
                    villageTri.Info.SetTerrain(Terrain.LightUrban);
                    world.Locations.AddLocation(villageTri);
                }
            }
        }
        plates.Plates.ForEach(buildCitiesForPlate);
    }
    private float GetCityScore(Triangle tri)
    {
        if (tri.Info.IsWater) return _cityScoreFromWater;
        var score = 1f - _roughnessCityScoreCost * tri.Info.Roughness.OutValue;
        return Mathf.Max(0f, score);
    }

    private bool TriIsGoodForCity(Triangle tri)
    {
        return tri.Info.IsLand
               && tri.Info.TerrainType != Terrain.HighMountains
               && tri.Info.TerrainType == Terrain.LowMountains
            ? Game.Random.Randf() < _lowMountainChanceToHaveCity
            : true;
    }
}
