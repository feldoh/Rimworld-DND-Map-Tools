using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace DND_Utilities
{
    [StaticConstructorOnStartup]
    public static class RWUtils
    {
        [DebugAction(category: "General", name: "Set time to day", requiresRoyalty: false, requiresIdeology: false, requiresBiotech: false, requiresAnomaly: false,
            displayPriority: 0, hideInSubMenu: false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 501)]
        public static void SetDaytime()
        {
            Find.TickManager.DebugSetTicksGame(newTicksGame: Find.TickManager.TicksGame + (60000 - Find.TickManager.TicksGame % 60000) + 15000);
        }

        [DebugAction(category: "General", name: "Fill area with plants (rect)", requiresRoyalty: false, requiresIdeology: false, requiresBiotech: false, requiresAnomaly: false,
            displayPriority: 0, hideInSubMenu: false, actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 100)]
        public static List<DebugActionNode> PlantArea()
        {
            List<DebugActionNode> list = [];

            List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;

            foreach (var plant in allDefsListForReading)
            {
                if (plant.category != ThingCategory.Plant) continue;

                DebugActionNode node = new(label: plant.defName);

                ThingDef plant1 = plant;
                node.childGetter = () => selectChance(arg: plant1);

                list.Add(item: node);
            }

            return list;
        }

        public static Func<ThingDef, List<DebugActionNode>> selectChance = delegate(ThingDef plant)
        {
            List<DebugActionNode> list = [];

            for (float i = 0.05f; i <= 1f; i += 0.05f)
            {
                float chance = i;
                list.Add(new DebugActionNode((int) (chance * 100) + "% chance", DebugActionType.Action, delegate
                {
                    DebugToolsGeneral.GenericRectTool("Fill area", delegate(CellRect rect)
                    {
                        rect.ClipInsideMap(Find.CurrentMap);

                        foreach (IntVec3 item in rect)
                        {
                            if (!plant.CanEverPlantAt(item, Find.CurrentMap, true) || !Rand.Chance(chance)) continue;

                            Plant newPlant = (Plant) GenSpawn.Spawn(plant, item, Find.CurrentMap);
                            newPlant.Growth = Rand.Range(0.75f, 1f);
                            newPlant.Age = newPlant.def.plant.LimitedLifespan ? Rand.Range(0, Mathf.Max(newPlant.def.plant.LifespanTicks - 2500, 0)) : 0;
                        }
                    });
                }));
            }

            return list;
        };
    }
}
