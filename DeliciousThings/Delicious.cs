global using System;
global using System.Collections;
global using System.Collections.Generic;
global using UnityEngine;
global using UnityEngine.Networking;
global using UnityEngine.AddressableAssets;
global using UnityEngine.ResourceManagement.AsyncOperations;
global using BepInEx;
global using BepInEx.Configuration;
global using MonoMod.Cil;
global using Mono.Cecil.Cil;
global using R2API;
global using RoR2;
global using Object = UnityEngine.Object;

using System.Security.Permissions;
using System.Security;
using RoR2.ContentManagement;
using BepInEx.Logging;
using System.Linq;
using HG;
using HG.Coroutines;
using ShaderSwapper;
using RoR2BepInExPack.VanillaFixes;
using System.Reflection;
using RoR2.Skills;
using DeliciousThings.Permutations;
using DeliciousThings.Weather;
using Mono.Cecil;
using DeliciousThings.Items;
using RoR2.ExpansionManagement;


[module: UnverifiableCode]
#pragma warning disable
[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace DeliciousThings;

[BepInPlugin(GUID, NAME, VERSION)]
public partial class Delicious : BaseUnityPlugin, IContentPackProvider
{
    public const string
        NAME = "DeliciousThings",
        GUID = "groovesalad." + NAME,
        VERSION = "1.0.0",
        CONTENT_ENABLED_FORMAT = "Enable {0}?",
        IDENTIFIER_FORMAT = NAME + ".{0}",
        UNLOCKABLE_ITEM_FORMAT = "Items.{0}",
        UNLOCKABLE_SKILL_FORMAT = "Skills.{0}";

    public static new ManualLogSource Logger { get; private set; }
    public static ConfigFile ArtifactsConfig { get; private set; }
    public static ConfigFile EquipmentConfig { get; private set; }
    public static ConfigFile ItemsConfig { get; private set; }
    public static ConfigFile SkillsConfig { get; private set; }
    public static ConfigFile PermutationsConfig { get; private set; }
    public static AsyncOperationHandle<IDictionary<string, ItemDisplayRuleSet>> IDRS { get; private set; }

    public string identifier => GUID;

    public AssetBundleCreateRequest assetBundleCreateRequest;
    public ContentPack contentPack;
    public ExpansionDef expansionDef;
    public object[] content;

    public void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Awake!!");

        ArtifactsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Artifacts.cfg"), false);
        EquipmentConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Equipment.cfg"), false);
        ItemsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Items.cfg"), false);
        SkillsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Skills.cfg"), false);
        PermutationsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".StagePermutations.cfg"), false);

        assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "freeitemfridayassets"));

        contentPack = new ContentPack
        {
            identifier = identifier,
        };

        expansionDef = ScriptableObject.CreateInstance<Expansion>();
        contentPack.expansionDefs.Add([expansionDef]);

        IEnumerable<Type> exportedTypes = typeof(Delicious).Assembly.ExportedTypes;

        contentPack.itemDefs.Add(exportedTypes
            .Where(x => x.IsSubclassOf(typeof(ItemDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<ItemDef>()
            .ToArray());
        foreach (ItemDef itemDef in contentPack.itemDefs)
        {
            itemDef.requiredExpansion = expansionDef;
        }

        contentPack.equipmentDefs.Add(exportedTypes
            .Where(x => x.IsSubclassOf(typeof(EquipmentDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<EquipmentDef>()
            .ToArray());
        foreach (EquipmentDef equipmentDef in contentPack.equipmentDefs)
        {
            equipmentDef.requiredExpansion = expansionDef;
        }

        contentPack.skillDefs.Add(exportedTypes
            .Where(x => x.IsSubclassOf(typeof(SkillDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<SkillDef>()
            .ToArray());

        contentPack.unlockableDefs.Add(exportedTypes
            .Where(x => x.IsSubclassOf(typeof(UnlockableDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<UnlockableDef>()
            .ToArray());

        contentPack.entityStateTypes.Add(exportedTypes
            .Where(x => x.IsSubclassOf(typeof(EntityStates.EntityState)) && !x.IsAbstract)
            .ToArray());
        
        ContentManager.collectContentPackProviders += add => add(this);

        /*AchievementDef[] achievements = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(AchievementDef)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<AchievementDef>()
            .Where(x => x.unlockableRewardIdentifier != null)
            .ToArray();*/

        PermutationDef[] permutations = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(PermutationDef)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<PermutationDef>()
            .Where(x => x.enabled)
            .ToArray();
        PermutationManager.SetPermutationDefs(permutations);

        WeatherDef[] weathers = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(WeatherDef)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<WeatherDef>()
            .Where(x => x.enabled)
            .ToArray();
        WeatherManager.SetWeatherDefs(weathers);

        content = [
            expansionDef, 
            .. contentPack.itemDefs, 
            .. contentPack.equipmentDefs, 
            .. contentPack.skillDefs, 
            .. contentPack.unlockableDefs, 
            .. permutations, 
            .. weathers
            ];

        /*SaferAchievementManager.OnCollectAchievementDefs += (identifiers, identifierToAchievementDef, achievementDefs) =>
        {
            foreach (AchievementDef achievement in content.OfType<IAchievementDefProvider>().Select(x => x.AchievementDef))
            {
                identifiers.Add(achievement.identifier);
                identifierToAchievementDef.Add(achievement.identifier, achievement);
                achievementDefs.Add(achievement);
            }
        };*/

        On.RoR2.Language.LoadStrings += (orig, self) =>
        {
            if (!self.stringsLoaded)
            {
                switch (self.name)
                {
                    case "en":
                        self.SetStringsByTokens(content.OfType<English>().SelectMany(x => x.TokenPairs));
                        break;
                }
            }
            orig(self);
        };
    }

    public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
    {
        IDRS = IvyLibrary.Ivyl.ToAssetDictionary<ItemDisplayRuleSet>(Addressables.LoadResourceLocationsAsync((IEnumerable<object>)["ContentPack:RoR2.BaseContent", "ContentPack:RoR2.DLC1"], Addressables.MergeMode.Union, typeof(ItemDisplayRuleSet)));

        while (!assetBundleCreateRequest.isDone) yield return null;
        AssetBundle assets = assetBundleCreateRequest.assetBundle;

        ParallelProgressCoroutine parallelProgressCoroutine = new ParallelProgressCoroutine(new ReadableProgress<float>(args.ReportProgress));
        foreach (IStaticContent staticContent in content.OfType<IStaticContent>())
        {
            static IEnumerator SafeCoroutineWrapper(IEnumerator coroutine)
            {
                while (coroutine.MoveNext())
                {
                    object current = coroutine.Current;
                    if (current is IEnumerator inner)
                    {
                        while (inner.MoveNext()) yield return inner.Current;
                    }
                    else if (current is AsyncOperation asyncOperation)
                    {
                        while (!asyncOperation.isDone) yield return null;
                    }
                    else yield return current;
                }
            }
            ReadableProgress<float> progressReceiver = new();
            parallelProgressCoroutine.Add(SafeCoroutineWrapper(staticContent.LoadAsync(progressReceiver, assets)), progressReceiver);
        }
        while (parallelProgressCoroutine.MoveNext()) yield return parallelProgressCoroutine.Current;

        contentPack.equipmentDefs.Add(content.OfType<IEquipmentDefProvider>().SelectMany(x => x.EquipmentDefs).ToArray());
        contentPack.unlockableDefs.Add(content.OfType<IUnlockableDefProvider>().Select(x => x.UnlockableDef).ToArray());
        contentPack.effectDefs.Add(content.OfType<IEffectPrefabProvider>().SelectMany(x => x.EffectPrefabs).Select(x => new EffectDef(x)).ToArray());
        contentPack.networkedObjectPrefabs.Add(content.OfType<INetworkedObjectPrefabProvider>().SelectMany(x => x.NetworkedObjectPrefabs).ToArray());
    }

    public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
    {
        ContentPack.Copy(contentPack, args.output);
        yield break;
    }

    public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
    {
        IEnumerator upgradeStubbedShaders = assetBundleCreateRequest.assetBundle.UpgradeStubbedShadersAsync();
        while (upgradeStubbedShaders.MoveNext()) yield return upgradeStubbedShaders.Current;

        assetBundleCreateRequest.assetBundle?.Unload(false);
    }

    public interface IStaticContent
    {
        public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets);
    }

    public interface IEquipmentDefProvider
    {
        public IEnumerable<EquipmentDef> EquipmentDefs { get; }
    }

    public interface IUnlockableDefProvider
    {
        public UnlockableDef UnlockableDef { get; }
    }

    public interface IEffectPrefabProvider
    {
        public IEnumerable<GameObject> EffectPrefabs { get; }
    }

    public interface INetworkedObjectPrefabProvider
    {
        public IEnumerable<GameObject> NetworkedObjectPrefabs { get; }
    }
}