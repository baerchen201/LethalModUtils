using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace LethalModUtils;

public struct StaticData
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string[] toSArr<T>(IEnumerable<T>? enumerable)
    {
        return enumerable?.Select(i => i?.ToString() ?? string.Empty).ToArray() ?? [];
    }

    public static class ImportUtil
    {
        #region UnityEngine

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Keyframe ToUnity(AnimationCurve.Keyframe keyframe)
        {
            return new Keyframe(
                keyframe.time,
                keyframe.value,
                keyframe.inTangent,
                keyframe.outTangent,
                keyframe.inWeight,
                keyframe.outWeight
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationCurve.Keyframe Import(Keyframe keyframe)
        {
            return new AnimationCurve.Keyframe
            {
                time = keyframe.time,
                value = keyframe.value,
                inTangent = keyframe.inTangent,
                outTangent = keyframe.outTangent,
                inWeight = keyframe.inWeight,
                outWeight = keyframe.outWeight,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.AnimationCurve ToUnity(AnimationCurve animationCurve)
        {
            return new UnityEngine.AnimationCurve(animationCurve.keys.Select(ToUnity).ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationCurve Import(UnityEngine.AnimationCurve animationCurve)
        {
            return new AnimationCurve { keys = animationCurve.keys.Select(Import).ToArray() };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Color ToUnity(Color color)
        {
            return new UnityEngine.Color(color.r, color.g, color.b, color.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Import(UnityEngine.Color color)
        {
            return new Color
            {
                r = color.r,
                g = color.g,
                b = color.b,
                a = color.a,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToUnity(Vector3 vector3)
        {
            return new UnityEngine.Vector3(vector3.x, vector3.y, vector3.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Import(UnityEngine.Vector3 vector3)
        {
            return new Vector3
            {
                x = vector3.x,
                y = vector3.y,
                z = vector3.z,
            };
        }

        #endregion

        #region Assembly-CSharp

        public static ItemType Import(Item item)
        {
            if (!item)
                return new ItemType();
            return new ItemType
            {
                Id = item.itemId,
                Name = item.itemName,
                LockedInDemo = item.lockedInDemo,
                PrefabName = item.spawnPrefab?.ToString() ?? string.Empty,
                ItemIcon = item.itemIcon?.ToString() ?? string.Empty,
                SpawnPositionTypes =
                    item.spawnPositionTypes?.Select(i => i.itemSpawnTypeName).ToArray() ?? [],
                TwoHanded = item.twoHanded,
                TwoHandedAnimation = item.twoHandedAnimation,
                DisableHandsOnWall = item.disableHandsOnWall,
                CanBeGrabbedBeforeGameStart = item.canBeGrabbedBeforeGameStart,
                DisallowUtilitySlot = item.disallowUtilitySlot,
                Weight = item.weight,
                IsTrigger = item.itemIsTrigger,
                HoldButtonUse = item.holdButtonUse,
                SpawnsOnGround = item.itemSpawnsOnGround,
                IsConductive = item.isConductiveMetal,
                IsScrap = item.isScrap,
                Value = item.creditsWorth,
                MaxValue = item.maxValue,
                MinValue = item.minValue,
                MaxSalePercentage = item.highestSalePercentage,
                UsesBattery = item.requiresBattery,
                BatteryUsage = item.batteryUsage,
                AutomaticallySetUsingPower = item.automaticallySetUsingPower,
                GrabAnimation = item.grabAnim,
                GrabAnimationTime = item.grabAnimationTime,
                UseAnimation = item.useAnim,
                PocketAnimation = item.pocketAnim,
                ThrowAnimation = item.throwAnim,
                GrabSFX = item.grabSFX?.ToString() ?? string.Empty,
                DropSFX = item.dropSFX?.ToString() ?? string.Empty,
                PocketSFX = item.pocketSFX?.ToString() ?? string.Empty,
                ThrowSFX = item.throwSFX?.ToString() ?? string.Empty,
                SyncGrabFunction = item.syncGrabFunction,
                SyncUseFunction = item.syncUseFunction,
                SyncDiscardFunction = item.syncDiscardFunction,
                SyncInteractLRFunction = item.syncInteractLRFunction,
                SaveItemVariable = item.saveItemVariable,
                IsWeapon = item.isDefensiveWeapon,
                ToolTips = item.toolTips,
                VerticalOffset = item.verticalOffset,
                FloorYOffset = item.floorYOffset,
                DropAheadOfPlayer = item.allowDroppingAheadOfPlayer,
                RestingRotation = Import(item.restingRotation),
                RotationOffset = Import(item.rotationOffset),
                PositionOffset = Import(item.positionOffset),
                MeshOffset = item.meshOffset,
                MeshVariants = toSArr(item.meshVariants),
                MaterialVariants = toSArr(item.materialVariants),
                ClinkAudios = toSArr(item.clinkAudios),
                UsableInSpecialAnimations = false,
                CanBeInspected = false,
                _imported_from = item,
            };
        }

        public static EnemyType.Animation Import(MiscAnimation miscAnimation)
        {
            return new EnemyType.Animation
            {
                Name = miscAnimation.AnimString,
                AudioClip = miscAnimation.AnimVoiceclip?.ToString() ?? string.Empty,
                Length = miscAnimation.AnimLength,
                Priority = miscAnimation.priority,
            };
        }

        public static EnemyType Import(global::EnemyType enemyType)
        {
            if (!enemyType)
                return new EnemyType();
            return new EnemyType
            {
                Name = enemyType.enemyName,
                PrefabName = enemyType.enemyPrefab?.ToString() ?? string.Empty,
                ProbabilityCurve = Import(enemyType.probabilityCurve),
                Disabled = enemyType.spawningDisabled,
                SpawnFromWeeds = enemyType.spawnFromWeeds,
                SpawnFalloff = Import(enemyType.numberSpawnedFalloff),
                UseSpawnFalloff = enemyType.useNumberSpawnedFalloff,
                SpawnInGroups = enemyType.spawnInGroupsOf,
                RequireNestObject = enemyType.requireNestObjectsToSpawn,
                PowerLevel = enemyType.PowerLevel,
                DiversityPowerLevel = enemyType.DiversityPowerLevel,
                MaxCount = enemyType.MaxCount,
                IsOutsideEnemy = enemyType.isOutsideEnemy,
                IsDaytimeEnemy = enemyType.isDaytimeEnemy,
                IncreasedChanceInterior = enemyType.increasedChanceInterior,
                NormalizedTimeInDayToLeave = enemyType.normalizedTimeInDayToLeave,
                StunTimeMultiplier = enemyType.stunTimeMultiplier,
                DoorSpeedMultiplier = enemyType.doorSpeedMultiplier,
                StunGameDifficultyMultiplier = enemyType.stunGameDifficultyMultiplier,
                CanBeStunned = enemyType.canBeStunned,
                CanDie = enemyType.canDie,
                CanBeDestroyed = enemyType.canBeDestroyed,
                DestroyOnDeath = enemyType.destroyOnDeath,
                CanSeeThroughFog = enemyType.canSeeThroughFog,
                DisableAnimatorWhenFar = enemyType.disableAnimatorWhenFar,
                PushPlayerForce = enemyType.pushPlayerForce,
                PushPlayerDistance = enemyType.pushPlayerDistance,
                SizeLimit = (int)enemyType.SizeLimit,
                EnemySize = (int)enemyType.EnemySize,
                WaterType = (int)enemyType.WaterType,
                TimeToPlayAudio = enemyType.timeToPlayAudio,
                LoudnessMultiplier = enemyType.loudnessMultiplier,
                OverrideVentSFX = enemyType.overrideVentSFX?.ToString() ?? string.Empty,
                NestPrefabName = enemyType.nestSpawnPrefab?.ToString() ?? string.Empty,
                NestPrefabWidth = enemyType.nestSpawnPrefabWidth,
                NestDistanceFromShip = enemyType.nestDistanceFromShip,
                UseMinEnemyThresholdForNest = enemyType.useMinEnemyThresholdForNest,
                MinEnemiesToSpawnNest = enemyType.minEnemiesToSpawnNest,
                HitBodySFX = enemyType.hitBodySFX?.ToString() ?? string.Empty,
                HitEnemyVoiceSFX = enemyType.hitEnemyVoiceSFX?.ToString() ?? string.Empty,
                DeathSFX = enemyType.deathSFX?.ToString() ?? string.Empty,
                StunSFX = enemyType.stunSFX?.ToString() ?? string.Empty,
                MiscAudioClips = toSArr(enemyType.audioClips),
                MiscAnimations = enemyType.miscAnimations?.Select(Import).ToArray() ?? [],
                _imported_from = enemyType,
            };
        }

        public static Level.WeatherWithVariables Import(
            RandomWeatherWithVariables randomWeatherWithVariables
        )
        {
            return new Level.WeatherWithVariables
            {
                Weather = (int)randomWeatherWithVariables.weatherType,
                Variable1 = randomWeatherWithVariables.weatherVariable,
                Variable2 = randomWeatherWithVariables.weatherVariable2,
                Color = Import(randomWeatherWithVariables.weatherVariableColor),
            };
        }

        public static Level.LevelAmbience.WeightedAudioClip Import(RandomAudioClip randomAudioClip)
        {
            return new Level.LevelAmbience.WeightedAudioClip
            {
                AudioClip = randomAudioClip.audioClip?.ToString() ?? string.Empty,
                Rarity = randomAudioClip.chance,
            };
        }

        public static Level.LevelAmbience Import(LevelAmbienceLibrary levelAmbienceLibrary)
        {
            if (!levelAmbienceLibrary)
                return new Level.LevelAmbience();
            return new Level.LevelAmbience
            {
                insanityMusicAudios = toSArr(levelAmbienceLibrary.insanityMusicAudios),
                insideAmbience = toSArr(levelAmbienceLibrary.insideAmbience),
                insideAmbienceInsanity = toWArr(levelAmbienceLibrary.insideAmbienceInsanity),
                shipAmbience = toSArr(levelAmbienceLibrary.shipAmbience),
                shipAmbienceInsanity = toWArr(levelAmbienceLibrary.shipAmbienceInsanity),
                outsideAmbience = toSArr(levelAmbienceLibrary.outsideAmbience),
                outsideAmbienceInsanity = toWArr(levelAmbienceLibrary.outsideAmbienceInsanity),
            };

            Level.LevelAmbience.WeightedAudioClip[] toWArr(IEnumerable<RandomAudioClip>? enumerable)
            {
                return enumerable?.Select(Import).ToArray() ?? [];
            }
        }

        public static Level.Interior Import(IntWithRarity intWithRarity)
        {
            return new Level.Interior
            {
                Id = intWithRarity.id,
                Rarity = intWithRarity.rarity,
                OverridesLevelAmbience = intWithRarity.overrideLevelAmbience,
                LevelAmbience = Import(intWithRarity.overrideLevelAmbience),
            };
        }

        public static Level.InsideHazard.HazardType Import(IndoorMapHazardType indoorMapHazardType)
        {
            if (!indoorMapHazardType)
                return new Level.InsideHazard.HazardType();
            return new Level.InsideHazard.HazardType
            {
                PrefabName = indoorMapHazardType.prefabToSpawn?.ToString() ?? string.Empty,
                SpawnFacingAwayFromWall = indoorMapHazardType.spawnFacingAwayFromWall,
                SpawnFacingWall = indoorMapHazardType.spawnFacingWall,
                SpawnWithBackToWall = indoorMapHazardType.spawnWithBackToWall,
                SpawnWithBackFlushAgainstWall = indoorMapHazardType.spawnWithBackFlushAgainstWall,
                RequireDistanceBetweenSpawns = indoorMapHazardType.requireDistanceBetweenSpawns,
                DisallowSpawningNearEntrances = indoorMapHazardType.disallowSpawningNearEntrances,
                SpawnInMineshaft = indoorMapHazardType.allowInMineshaft,
            };
        }

        public static Level.InsideHazard Import(IndoorMapHazard indoorMapHazard)
        {
            return new Level.InsideHazard
            {
                Hazard = Import(indoorMapHazard.hazardType),
                SpawnAmount = Import(indoorMapHazard.numberToSpawn),
            };
        }

        public static Level.OutsideHazard.HazardType Import(
            SpawnableOutsideObject spawnableOutsideObject
        )
        {
            if (!spawnableOutsideObject)
                return new Level.OutsideHazard.HazardType();
            return new Level.OutsideHazard.HazardType
            {
                PrefabName = spawnableOutsideObject.prefabToSpawn?.ToString() ?? string.Empty,
                SpawnFacingAwayFromWall = spawnableOutsideObject.spawnFacingAwayFromWall,
                ObjectWidth = spawnableOutsideObject.objectWidth,
                DestroyTrees = spawnableOutsideObject.destroyTrees,
                SpawnableFloorTags = spawnableOutsideObject.spawnableFloorTags,
                RotationOffset = Import(spawnableOutsideObject.rotationOffset),
            };
        }

        public static Level.OutsideHazard Import(
            SpawnableOutsideObjectWithRarity spawnableOutsideObjectWithRarity
        )
        {
            return new Level.OutsideHazard
            {
                Hazard = Import(spawnableOutsideObjectWithRarity.spawnableObject),
                SpawnAmount = Import(spawnableOutsideObjectWithRarity.randomAmount),
            };
        }

        public static StaticData Import(
            int gameVersion,
            AllItemsList allItemsList,
            SelectableLevel[] selectableLevels
        )
        {
            List<ItemType> ItemTable = [];
            List<EnemyType> EnemyTable = [];
            List<Level> Levels = [];

            if (allItemsList)
                foreach (var item in allItemsList.itemsList)
                    GetOrImportItemType(item);

            foreach (var selectableLevel in selectableLevels)
                Levels.Add(
                    new Level
                    {
                        Name = selectableLevel.PlanetName,
                        SceneName = selectableLevel.sceneName,
                        LockedForDemo = selectableLevel.lockedForDemo,
                        SpawnEnemiesAndScrap = selectableLevel.spawnEnemiesAndScrap,
                        LevelDescription = selectableLevel.LevelDescription,
                        RiskLevel = selectableLevel.riskLevel,
                        LandingTime = selectableLevel.timeToArrive,
                        VideoPreview = selectableLevel.videoReel?.ToString() ?? string.Empty,
                        LevelIcon = selectableLevel.levelIconString,
                        HasTime = selectableLevel.planetHasTime,
                        OffsetFromGlobalTime = selectableLevel.OffsetFromGlobalTime,
                        DaySpeedMultiplier = selectableLevel.DaySpeedMultiplier,
                        HasStaticWeather = selectableLevel.overrideWeather,
                        StaticWeather = (int)selectableLevel.overrideWeatherType,
                        Weathers = selectableLevel.randomWeathers.Select(Import).ToArray(),
                        InteriorSizeMultiplier = selectableLevel.factorySizeMultiplier,
                        Interiors = selectableLevel.dungeonFlowTypes.Select(Import).ToArray(),
                        HasAmbienceClips = selectableLevel.levelAmbienceClips,
                        AmbienceClips = Import(selectableLevel.levelAmbienceClips),
                        SpawnableInsideObjects = selectableLevel
                            .indoorMapHazards.Select(Import)
                            .ToArray(),
                        SpawnableOutsideObjects = selectableLevel
                            .spawnableOutsideObjects.Select(Import)
                            .ToArray(),
                        CanSpawnMold = selectableLevel.canSpawnMold,
                        MoldSpreadIterations = selectableLevel.moldSpreadIterations,
                        MoldStartPosition = selectableLevel.moldStartPosition,
                        MoldType = selectableLevel.moldType,
                        SpawnableScrap = selectableLevel
                            .spawnableScrap.Select(i => new Level.SpawnableItem
                            {
                                ItemIndex = GetOrImportItemType(i.spawnableItem),
                                Rarity = i.rarity,
                            })
                            .ToArray(),
                        MinScrap = selectableLevel.minScrap,
                        MaxScrap = selectableLevel.maxScrap,
                        MinTotalScrapValue = selectableLevel.minTotalScrapValue,
                        MaxTotalScrapValue = selectableLevel.maxTotalScrapValue,
                        MaxEnemyPowerCount = selectableLevel.maxEnemyPowerCount,
                        MaxOutsideEnemyPowerCount = selectableLevel.maxOutsideEnemyPowerCount,
                        MaxDaytimeEnemyPowerCount = selectableLevel.maxDaytimeEnemyPowerCount,
                        MaxInsideDiversityPowerCount = selectableLevel.maxInsideDiversityPowerCount,
                        MaxOutsideDiversityPowerCount =
                            selectableLevel.maxOutsideDiversityPowerCount,
                        InsideEnemies = toEArr(selectableLevel.Enemies),
                        SpecialEnemyRarity = new Level.OverrideSpawnableEnemy
                        {
                            EnemyIndex = GetOrImportEnemyType(
                                selectableLevel.specialEnemyRarity.overrideEnemy
                            ),
                            Chance = selectableLevel.specialEnemyRarity.percentageChance,
                        },
                        OutsideEnemies = toEArr(selectableLevel.OutsideEnemies),
                        DaytimeEnemies = toEArr(selectableLevel.DaytimeEnemies),
                        InsideEnemySpawnChanceThroughoutDay = Import(
                            selectableLevel.enemySpawnChanceThroughoutDay
                        ),
                        OutsideEnemySpawnChanceThroughoutDay = Import(
                            selectableLevel.outsideEnemySpawnChanceThroughDay
                        ),
                        DaytimeEnemySpawnChanceThroughoutDay = Import(
                            selectableLevel.daytimeEnemySpawnChanceThroughDay
                        ),
                        EnemySpawnProbabilityRange = selectableLevel.spawnProbabilityRange,
                        DaytimeEnemySpawnProbabilityRange =
                            selectableLevel.daytimeEnemiesProbabilityRange,
                        SnowFootprints = selectableLevel.levelIncludesSnowFootprints,
                    }
                );

            return new StaticData
            {
                GameVersion = gameVersion,
                ModVersion = MyPluginInfo.PLUGIN_VERSION,
                ImportTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

                NavSizeLimitValues = enumToDict<NavSizeLimit>(),
                EnemySizeValues = enumToDict<EnemySize>(),
                EnemyWaterTypeValues = enumToDict<EnemyWaterType>(),
                LevelWeatherTypeValues = enumToDict<LevelWeatherType>(),

                ItemTable = ItemTable.ToArray(),
                EnemyTable = EnemyTable.ToArray(),
                Levels = Levels.ToArray(),
            };

            int GetOrImportItemType(Item item)
            {
                var i = 0;
                foreach (var itemType in ItemTable)
                    if (itemType._imported_from != null && item == (Item)itemType._imported_from)
                        return i;
                    else
                        ++i;
                ItemTable.Add(Import(item));
                return i;
            }

            int GetOrImportEnemyType(global::EnemyType gEnemyType)
            {
                var i = 0;
                foreach (var enemyType in EnemyTable)
                    if (
                        enemyType._imported_from != null
                        && gEnemyType == (global::EnemyType)enemyType._imported_from
                    )
                        return i;
                    else
                        ++i;
                EnemyTable.Add(Import(gEnemyType));
                return i;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            Level.SpawnableEnemy[] toEArr(IEnumerable<SpawnableEnemyWithRarity>? enumerable)
            {
                return enumerable
                        ?.Select(i => new Level.SpawnableEnemy
                        {
                            EnemyIndex = GetOrImportEnemyType(i.enemyType),
                            Rarity = i.rarity,
                        })
                        .ToArray() ?? [];
            }

            Dictionary<int, string> enumToDict<T>()
                where T : struct, Enum
            {
                return ((int[])Enum.GetValues(typeof(T))).ToDictionary(
                    v => v,
                    v => Enum.GetName(typeof(T), v)
                );
            }
        }

        #endregion
    }

    public struct AnimationCurve
    {
        public struct Keyframe
        {
            public float time;
            public float value;
            public float inTangent;
            public float outTangent;
            public float inWeight;
            public float outWeight;
        }

        public Keyframe[] keys;
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public struct ItemType
    {
        public int Id;
        public string Name;
        public bool LockedInDemo;
        public string PrefabName;
        public string ItemIcon;

        public string[] SpawnPositionTypes;

        public bool TwoHanded;
        public bool TwoHandedAnimation;
        public bool DisableHandsOnWall;
        public bool CanBeGrabbedBeforeGameStart;
        public bool DisallowUtilitySlot;

        public float Weight;
        public bool IsTrigger;
        public bool HoldButtonUse;
        public bool SpawnsOnGround;

        public bool IsConductive;
        public bool IsScrap;
        public int Value;
        public int MaxValue;
        public int MinValue;
        public int MaxSalePercentage;

        public bool UsesBattery;
        public float BatteryUsage;
        public bool AutomaticallySetUsingPower;

        /// <summary>
        ///     Player animations
        /// </summary>
        public string GrabAnimation;

        /// <summary>
        ///     Player animations
        /// </summary>
        public float GrabAnimationTime;

        /// <summary>
        ///     Player animations
        /// </summary>
        public string UseAnimation;

        /// <summary>
        ///     Player animations
        /// </summary>
        public string PocketAnimation;

        /// <summary>
        ///     Player animations
        /// </summary>
        public string ThrowAnimation;

        public string GrabSFX;
        public string DropSFX;
        public string PocketSFX;
        public string ThrowSFX;

        public bool SyncGrabFunction;
        public bool SyncUseFunction;
        public bool SyncDiscardFunction;
        public bool SyncInteractLRFunction;

        public bool SaveItemVariable;

        public bool IsWeapon;

        public string[] ToolTips;
        public float VerticalOffset;
        public int FloorYOffset;
        public bool DropAheadOfPlayer;
        public Vector3 RestingRotation;
        public Vector3 RotationOffset;
        public Vector3 PositionOffset;
        public bool MeshOffset;
        public string[] MeshVariants;
        public string[] MaterialVariants;

        public string[] ClinkAudios;
        public bool UsableInSpecialAnimations;
        public bool CanBeInspected;

        [JsonIgnore]
        internal object? _imported_from;
    }

    public struct EnemyType
    {
        public struct Animation
        {
            public string Name;
            public string AudioClip;
            public float Length;
            public int Priority;
        }

        public string Name;
        public string PrefabName;

        public AnimationCurve ProbabilityCurve;
        public bool Disabled;
        public bool SpawnFromWeeds;

        /// <summary>
        ///     X axis is the number of this enemy type that have spawned, divided by 10; Y axis is a multiplier to
        ///     probabilityCurve.
        /// </summary>
        public AnimationCurve SpawnFalloff;

        public bool UseSpawnFalloff;

        public int SpawnInGroups;
        public bool RequireNestObject;

        /// <summary>
        ///     Adds to a global counter determining how many enemies can spawn.
        /// </summary>
        public float PowerLevel;

        /// <summary>
        ///     Adds to a global (inside or outside) counter, only once for this enemy type / species.
        /// </summary>
        public int DiversityPowerLevel;

        /// <summary>
        ///     An individual counter determining how many of this enemy can spawn, regardless of how many other enemies there are.
        /// </summary>
        public int MaxCount;

        public bool IsOutsideEnemy;
        public bool IsDaytimeEnemy;

        public int IncreasedChanceInterior;

        public float NormalizedTimeInDayToLeave;

        public float StunTimeMultiplier;
        public float DoorSpeedMultiplier;
        public float StunGameDifficultyMultiplier;
        public bool CanBeStunned;
        public bool CanDie;
        public bool CanBeDestroyed;
        public bool DestroyOnDeath;
        public bool CanSeeThroughFog;
        public bool DisableAnimatorWhenFar;

        public float PushPlayerForce;
        public float PushPlayerDistance;

        /// <summary>
        ///     This determines where the enemy can navigate to and spawn at
        /// </summary>
        public int SizeLimit;

        public int EnemySize;
        public int WaterType;

        /// <summary>
        ///     Vent Properties
        /// </summary>
        public float TimeToPlayAudio;

        /// <summary>
        ///     Vent Properties
        /// </summary>
        public float LoudnessMultiplier;

        public string OverrideVentSFX;

        public string NestPrefabName;
        public float NestPrefabWidth;
        public float NestDistanceFromShip;

        /// <summary>
        ///     If false, nest objects will be spawned for each instance of this enemy that gets spawned. If true, they will all
        ///     share one nest object.
        /// </summary>
        public bool UseMinEnemyThresholdForNest;

        public int MinEnemiesToSpawnNest;

        public string HitBodySFX;
        public string HitEnemyVoiceSFX;
        public string DeathSFX;
        public string StunSFX;
        public string[] MiscAudioClips;
        public Animation[] MiscAnimations;

        [JsonIgnore]
        internal object? _imported_from;
    }

    public struct Level
    {
        public struct WeatherWithVariables
        {
            public int Weather;
            public int Variable1;
            public int Variable2;

            /// <remarks>Fog color</remarks>
            public Color Color;
        }

        public struct LevelAmbience
        {
            public struct WeightedAudioClip
            {
                public string AudioClip;

                /// <remarks>0 - 100</remarks>
                public int Rarity;
            }

            public string[] insanityMusicAudios;
            public string[] insideAmbience;
            public WeightedAudioClip[] insideAmbienceInsanity;
            public string[] shipAmbience;
            public WeightedAudioClip[] shipAmbienceInsanity;
            public string[] outsideAmbience;
            public WeightedAudioClip[] outsideAmbienceInsanity;
        }

        public struct Interior
        {
            public int Id;

            /// <remarks>0 - 300</remarks>
            public int Rarity;

            public bool OverridesLevelAmbience;
            public LevelAmbience LevelAmbience;
        }

        public struct InsideHazard
        {
            public struct HazardType
            {
                public string PrefabName;
                public bool SpawnFacingAwayFromWall;
                public bool SpawnFacingWall;
                public bool SpawnWithBackToWall;
                public bool SpawnWithBackFlushAgainstWall;
                public bool RequireDistanceBetweenSpawns;
                public bool DisallowSpawningNearEntrances;
                public bool SpawnInMineshaft;
            }

            public HazardType Hazard;

            /// <summary>
            ///     Y Axis is the amount to be spawned; X axis should be from 0 to 1 and is randomly picked from.
            /// </summary>
            public AnimationCurve SpawnAmount;
        }

        public struct OutsideHazard
        {
            public struct HazardType
            {
                public string PrefabName;
                public bool SpawnFacingAwayFromWall;
                public int ObjectWidth;
                public bool DestroyTrees;
                public string[] SpawnableFloorTags;
                public Vector3 RotationOffset;
            }

            public HazardType Hazard;

            /// <summary>
            ///     Y Axis is the amount to be spawned; X axis should be from 0 to 1 and is randomly picked from.
            /// </summary>
            public AnimationCurve SpawnAmount;
        }

        public struct SpawnableItem
        {
            public int ItemIndex;

            /// <remarks>0 - 100</remarks>
            public int Rarity;
        }

        public struct SpawnableEnemy
        {
            public int EnemyIndex;

            /// <remarks>0 - 200</remarks>
            public int Rarity;
        }

        public struct OverrideSpawnableEnemy
        {
            public int EnemyIndex;

            /// <remarks>0f - 1f</remarks>
            public float Chance;
        }

        public string Name;
        public string SceneName;
        public bool LockedForDemo;
        public bool SpawnEnemiesAndScrap;

        public string LevelDescription;
        public string RiskLevel;
        public float LandingTime;
        public string VideoPreview;
        public string LevelIcon;

        public bool HasTime;
        public float OffsetFromGlobalTime;
        public float DaySpeedMultiplier;

        public bool HasStaticWeather;
        public int StaticWeather;
        public WeatherWithVariables[] Weathers;

        public float InteriorSizeMultiplier;
        public Interior[] Interiors;
        public bool HasAmbienceClips;
        public LevelAmbience AmbienceClips;

        public InsideHazard[] SpawnableInsideObjects;
        public OutsideHazard[] SpawnableOutsideObjects;
        public bool CanSpawnMold;
        public int MoldSpreadIterations;
        public int MoldStartPosition;
        public int MoldType;

        public SpawnableItem[] SpawnableScrap;
        public int MinScrap;
        public int MaxScrap;
        public int MinTotalScrapValue;
        public int MaxTotalScrapValue;

        public int MaxEnemyPowerCount;
        public int MaxOutsideEnemyPowerCount;
        public int MaxDaytimeEnemyPowerCount;

        public int MaxInsideDiversityPowerCount;
        public int MaxOutsideDiversityPowerCount;

        public SpawnableEnemy[] InsideEnemies;
        public OverrideSpawnableEnemy SpecialEnemyRarity;
        public SpawnableEnemy[] OutsideEnemies;
        public SpawnableEnemy[] DaytimeEnemies;

        public AnimationCurve InsideEnemySpawnChanceThroughoutDay;
        public AnimationCurve OutsideEnemySpawnChanceThroughoutDay;
        public AnimationCurve DaytimeEnemySpawnChanceThroughoutDay;

        public float EnemySpawnProbabilityRange;
        public float DaytimeEnemySpawnProbabilityRange;
        public bool SnowFootprints;
    }

    public int GameVersion;
    public string ModVersion;
    public long ImportTime;

    public Dictionary<int, string> NavSizeLimitValues;
    public Dictionary<int, string> EnemySizeValues;
    public Dictionary<int, string> EnemyWaterTypeValues;
    public Dictionary<int, string> LevelWeatherTypeValues;

    public ItemType[] ItemTable;
    public EnemyType[] EnemyTable;
    public Level[] Levels;

    public static StaticData Deserialize(JsonReader reader)
    {
        return JsonSerializer.Create().Deserialize<StaticData>(reader);
    }

    public void Serialize(JsonWriter writer)
    {
        JsonSerializer.Create(new JsonSerializerSettings()).Serialize(writer, this);
    }
}

[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
internal static class StartOfRound_Awake
{
    private static void Postfix(ref StartOfRound __instance)
    {
        try
        {
            if (LethalModUtils.Instance.exportStaticData.Value)
                LethalModUtils.Instance.ExportStaticData(__instance);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
