using Godot;
using System;

public class TriChangedFactionEvent
{
    public Triangle Tri { get; private set; }
    public Faction GainingFaction { get; private set; }
    public Faction LosingFaction { get; private set; }

    public TriChangedFactionEvent(Triangle tri, Faction gainingFaction, Faction losingFaction)
    {
        Tri = tri;
        GainingFaction = gainingFaction;
        LosingFaction = losingFaction;
    }
}
