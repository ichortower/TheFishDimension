using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ichortower.TheFishDimension
{
    internal sealed class ModEntry : Mod
    {
        private const int category_fish = -4;
        private const double chance_fishing = 0.1f / 0.39f;
        private static IMonitor Mon;

        public override void Entry(IModHelper helper)
        {
            Mon = this.Monitor;
            var harmony = new Harmony(this.ModManifest.UniqueID);
            MethodInfo UtilityGRIFS = typeof(Utility).GetMethod("getRandomItemFromSeason",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] {typeof(Season), typeof(bool), typeof(Random)},
                    null);
            MethodInfo UtilityGQOTD = typeof(Utility).GetMethod("getQuestOfTheDay",
                    BindingFlags.Public | BindingFlags.Static);
            MethodInfo UtilityPCATT = typeof(Utility).GetMethod("possibleCropsAtThisTime",
                    BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(UtilityGRIFS,
                    postfix: new HarmonyMethod(typeof(ModEntry),
                        "getRandomItemFromSeason_Postfix"));
            harmony.Patch(UtilityGQOTD,
                    postfix: new HarmonyMethod(typeof(ModEntry),
                        "getQuestOfTheDay_Postfix"));
            harmony.Patch(UtilityPCATT,
                    postfix: new HarmonyMethod(typeof(ModEntry),
                        "possibleCropsAtThisTime_Postfix"));
        }

        public static void getRandomItemFromSeason_Postfix(
                Season season,
                bool forQuest,
                Random random,
                ref string __result)
        {
            if (!forQuest) {
                return;
            }
            List<string> possibleItems = GetPossibleFish(season);
            string dbg = "";
            foreach (string i in possibleItems) {
                dbg += (dbg == "" ? "" : " ") + i;
            }
            Mon.Log($"Choosing from these items: [{dbg}]", LogLevel.Warn);
            __result = random.ChooseFrom(possibleItems);
            Mon.Log($"(Chose {__result})", LogLevel.Warn);
        }

        public static void getQuestOfTheDay_Postfix(
                ref Quest __result)
        {
            if (__result is null) {
                return;
            }
            if (!(__result is FishingQuest) && !(__result is ItemDeliveryQuest)) {
                double d = Utility.CreateDaySaveRandom(1f, 39f, 23675f).NextDouble();
                if (d < chance_fishing) {
                    Mon.Log($"Replacing original quest with FishingQuest", LogLevel.Warn);
                    __result = new FishingQuest();
                }
                else {
                    Mon.Log($"Replacing original quest with ItemDeliveryQuest", LogLevel.Warn);
                    __result = new ItemDeliveryQuest();
                }
            }
        }

        public static void possibleCropsAtThisTime_Postfix(
                Season season,
                ref List<string> __result)
        {
            __result = GetPossibleFish(season);
        }

        private static List<string> GetPossibleFish(Season season)
        {
            List<string> fish = new();
            var odd = (ObjectDataDefinition)ItemRegistry.GetTypeDefinition(
                    ItemRegistry.type_object);
            bool allowDesertFish = Utility.doesAnyFarmerHaveMail("ccVault");
            string seasonTag = $"season_{Utility.getSeasonKey(season)}";
            foreach (string id in odd.GetAllIds()) {
                Item it = ItemRegistry.Create(id);
                if (it.Category == category_fish && it.HasContextTag("!fish_mines") &&
                        (it.HasContextTag(seasonTag) || it.HasContextTag("season_all") &&
                        it.HasContextTag((allowDesertFish ? "" : "!") + "fish_desert"))) {
                    fish.Add(it.ItemId);
                }
            }
            return fish;
        }
    }
}
