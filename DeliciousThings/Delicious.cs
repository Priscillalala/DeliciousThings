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
    public static AsyncOperationHandle<IDictionary<string, ItemDisplayRuleSet>> IDRS { get; private set; }
    public static Dictionary<EquipmentDef, Func<EquipmentSlot, bool>> EquipmentActivationFunctions { get; private set; } = [];

    public string identifier => GUID;

    public AssetBundleCreateRequest assetBundleCreateRequest;
    public ContentPack contentPack;
    public AchievementDef[] achievements;

    public void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Awake!!");

        ArtifactsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Artifacts.cfg"), false);
        EquipmentConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Equipment.cfg"), false);
        ItemsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Items.cfg"), false);
        SkillsConfig = IvyLibrary.Ivyl.CreateConfigFile(this, System.IO.Path.ChangeExtension(Config.ConfigFilePath, ".Skills.cfg"), false);

        assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "freeitemfridayassets"));

        contentPack = new ContentPack
        {
            identifier = identifier,
        };

        contentPack.expansionDefs.Add([ScriptableObject.CreateInstance<Expansion>()]);

        IEnumerable<Type> exportedTypes = typeof(Delicious).Assembly.ExportedTypes;

        ItemDef[] items = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(ItemDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<ItemDef>()
            .ToArray();
        foreach (ItemDef item in items)
        {
            item.requiredExpansion = Expansion.Instance;
        }
        contentPack.itemDefs.Add(items);

        EquipmentDef[] equipments = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(EquipmentDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<EquipmentDef>()
            .ToArray();
        foreach (EquipmentDef equipment in equipments)
        {
            equipment.requiredExpansion = Expansion.Instance;
        }
        contentPack.equipmentDefs.Add(equipments);

        if (EquipmentActivationFunctions.Count > 0)
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, equipmentDef) =>
            {
                if (EquipmentActivationFunctions.TryGetValue(equipmentDef, out Func<EquipmentSlot, bool> activationFunction))
                {
                    return activationFunction != null && activationFunction(self);
                }
                return orig(self, equipmentDef);
            };
        }
        else EquipmentActivationFunctions = null;

        SkillDef[] skills = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(SkillDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<SkillDef>()
            .ToArray();
        contentPack.skillDefs.Add(skills);

        UnlockableDef[] unlockables = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(UnlockableDef)) && !x.IsAbstract)
            .Select(ScriptableObject.CreateInstance)
            .OfType<UnlockableDef>()
            .ToArray();
        contentPack.unlockableDefs.Add(unlockables);

        contentPack.entityStateTypes.Add(exportedTypes.Where(x => x.IsSubclassOf(typeof(EntityStates.EntityState)) && !x.IsAbstract).ToArray());
        
        ContentManager.collectContentPackProviders += add => add(this);

        achievements = exportedTypes
            .Where(x => x.IsSubclassOf(typeof(AchievementDef)) && !x.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<AchievementDef>()
            .Where(x => x.unlockableRewardIdentifier != null)
            .ToArray();
        SaferAchievementManager.OnCollectAchievementDefs += (identifiers, identifierToAchievementDef, achievementDefs) =>
        {
            foreach (AchievementDef achievement in achievements)
            {
                identifiers.Add(achievement.identifier);
                identifierToAchievementDef.Add(achievement.identifier, achievement);
                achievementDefs.Add(achievement);
            }
        };
    }

    public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
    {
        IDRS = IvyLibrary.Ivyl.ToAssetDictionary<ItemDisplayRuleSet>(Addressables.LoadResourceLocationsAsync((IEnumerable<object>)["ContentPack:RoR2.BaseContent", "ContentPack:RoR2.DLC1"], Addressables.MergeMode.Union, typeof(ItemDisplayRuleSet)));
        
        while (!assetBundleCreateRequest.isDone) yield return null;
        AssetBundle assets = assetBundleCreateRequest.assetBundle;

        IEnumerable<object> content = [Expansion.Instance, .. contentPack.itemDefs, .. contentPack.equipmentDefs, .. contentPack.skillDefs, .. contentPack.unlockableDefs, .. achievements];

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

        contentPack.effectDefs.Add(content.OfType<IEffectPrefabProvider>().SelectMany(x => x.EffectPrefabs).Select(x => new EffectDef(x)).ToArray());
        contentPack.networkedObjectPrefabs.Add(content.OfType<INetworkedObjectPrefabProvider>().SelectMany(x => x.NetworkedObjectPrefabs).ToArray());

        Dictionary<string, IEnumerable<KeyValuePair<string, string>>> language = new()
        {
            { "en", content.OfType<English>().SelectMany(x => x.Language) }
        };
        On.RoR2.Language.LoadStrings += (orig, self) =>
        {
            orig(self);
            if (language.TryGetValue(self.name, out var tokenPairs))
            {
                self.SetStringsByTokens(tokenPairs);
            }
        };
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

    public interface IEffectPrefabProvider
    {
        public IEnumerable<GameObject> EffectPrefabs { get; }
    }

    public interface INetworkedObjectPrefabProvider
    {
        public IEnumerable<GameObject> NetworkedObjectPrefabs { get; }
    }
}