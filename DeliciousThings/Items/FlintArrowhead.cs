using DeliciousThings.Achievements;
using HG;
using IvyLibrary;
using RoR2.Items;

namespace DeliciousThings.Items;

public partial class FlintArrowhead : ItemDef, Delicious.IStaticContent, Delicious.IEffectPrefabProvider, Delicious.IUnlockableDefProvider
{
    public static ConfigFile Config => Delicious.ItemsConfig;

    const string SECTION = "Flint Arrowhead";
    public readonly bool enabled = Config.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, SECTION), true).Value;
    public readonly float damage = Config.Bind(SECTION, "Flat Damage", 3f).Value;
    public readonly float damagePerStack = Config.Bind(SECTION, "Flat Damage Per Stack", 3f).Value;

    public AchievementDef burnMultipleEnemies;
    public static DamageColorIndex strongerBurn;
    public GameObject impactArrowhead;
    public GameObject impactArrowheadStronger;

    public IEnumerable<GameObject> EffectPrefabs => [impactArrowhead, impactArrowheadStronger];
    public UnlockableDef UnlockableDef => unlockableDef;

    public void Awake()
    {
        if (enabled)
        {
            name = string.Format(Delicious.IDENTIFIER_FORMAT, "Arrowhead");
            AutoPopulateTokens();
            deprecatedTier = ItemTier.Tier1;
            tags = [ItemTag.Damage];
            strongerBurn = ColorsAPI.RegisterDamageColor(new Color32(244, 113, 80, 255));

            unlockableDef = CreateInstance<UnlockableDef>();
            unlockableDef.cachedName = string.Format(Delicious.UNLOCKABLE_ITEM_FORMAT, name);
            unlockableDef.nameToken = nameToken;

            burnMultipleEnemies = new AchievementDef
            {
                // Match achievement identifiers from FreeItemFriday
                identifier = "BurnMultipleEnemies",
                unlockableRewardIdentifier = unlockableDef.cachedName,
                type = typeof(BurnMultipleEnemiesAchievement),
                serverTrackerType = typeof(BurnMultipleEnemiesAchievement.ServerAchievement),
            };
            burnMultipleEnemies.AutoPopulateTokens();
            burnMultipleEnemies.Register();

            unlockableDef.PopulateUnlockStrings(burnMultipleEnemies);

            Events.GlobalEventManager.onHitEnemyAcceptedServer += GlobalEventManager_onHitEnemyAcceptedServer;
        }
        else DestroyImmediate(this);
    }

    [SystemInitializer(typeof(DotController))]
    private static void InitAfterDotCatalog()
    {
        if (strongerBurn != default)
        {
            DotController.dotDefs[(int)DotController.DotIndex.StrongerBurn].damageColorIndex = strongerBurn;
        }
    }

    private void GlobalEventManager_onHitEnemyAcceptedServer(DamageInfo damageInfo, GameObject victim, uint? dotMaxStacksFromAttacker)
    {
        if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasItem(this, out int stack) && Util.CheckRoll(100f * damageInfo.procCoefficient, attackerBody.master))
        {
            InflictDotInfo inflictDotInfo = new InflictDotInfo
            {
                attackerObject = damageInfo.attacker,
                dotIndex = DotController.DotIndex.Burn,
                victimObject = victim,
                totalDamage = Ivyl.StackScaling(damage, damagePerStack, stack),
            };
            StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref inflictDotInfo);
            DotController.DotDef dotDef = DotController.GetDotDef(inflictDotInfo.dotIndex);
            if (dotDef != null)
            {
                DamageInfo burnDamageInfo = new DamageInfo();
                burnDamageInfo.attacker = inflictDotInfo.attackerObject;
                burnDamageInfo.crit = false;
                burnDamageInfo.damage = (float)inflictDotInfo.totalDamage;
                burnDamageInfo.force = Vector3.zero;
                burnDamageInfo.inflictor = inflictDotInfo.attackerObject;
                burnDamageInfo.position = damageInfo.position;
                burnDamageInfo.procCoefficient = 0f;
                burnDamageInfo.damageColorIndex = dotDef.damageColorIndex;
                burnDamageInfo.damageType = DamageType.DoT | DamageType.Silent;
                burnDamageInfo.dotIndex = inflictDotInfo.dotIndex;
                if (inflictDotInfo.victimObject && inflictDotInfo.victimObject.TryGetComponent(out CharacterBody victimBody) && victimBody.healthComponent)
                {
                    victimBody.healthComponent.TakeDamage(burnDamageInfo);
                    EffectManager.SpawnEffect(inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? impactArrowhead : impactArrowheadStronger, new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation(-damageInfo.force),
                        scale = inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? 1.5f : 2.5f
                    }, true);
                }
            }
        }
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var texArrowheadIcon = assets.LoadAssetAsync<Sprite>("texArrowheadIcon");
        var PickupArrowhead = assets.LoadAssetAsync<GameObject>("PickupArrowhead");
        var DisplayArrowhead = assets.LoadAssetAsync<GameObject>("DisplayArrowhead");

        var texBurnMultipleEnemiesIcon = assets.LoadAssetAsync<Sprite>("texBurnMultipleEnemiesIcon");

        var OmniExplosionVFXQuick = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab");
        var matOmniHitspark3Gasoline = Addressables.LoadAssetAsync<Material>("RoR2/Base/IgniteOnKill/matOmniHitspark3Gasoline.mat");

        yield return texArrowheadIcon;
        pickupIconSprite = (Sprite)texArrowheadIcon.asset;
        yield return PickupArrowhead;
        pickupModelPrefab = (GameObject)PickupArrowhead.asset;
        Ivyl.SetupModelPanelParameters(pickupModelPrefab, new ModelPanelParams(Vector3.zero, 1, 8));

        yield return DisplayArrowhead;
        GameObject displayModelPrefab = (GameObject)DisplayArrowhead.asset;
        Ivyl.SetupItemDisplay(displayModelPrefab);
        ItemDisplaySpec itemDisplay = new ItemDisplaySpec(this, displayModelPrefab);
        yield return Delicious.IDRS;
        var idrs = Delicious.IDRS.Result;
        idrs["idrsCommando"].AddDisplayRule(itemDisplay, "Pelvis", new Vector3(-0.162F, -0.09F, -0.053F), new Vector3(7.522F, 244.056F, 358.818F), new Vector3(0.469F, 0.469F, 0.469F));
        idrs["idrsHuntress"].AddDisplayRule(itemDisplay, "Arrow", new Vector3(0.343F, 0F, 0F), new Vector3(87.415F, 144.866F, 55.112F), new Vector3(0.388F, 0.388F, 0.388F));
        idrs["idrsBandit2"].AddDisplayRule(itemDisplay, "Chest", new Vector3(0.153F, -0.144F, 0.066F), new Vector3(355.538F, 89.398F, 170.59F), new Vector3(0.507F, 0.507F, 0.507F));
        idrs["idrsToolbot"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.925F, 2.842F, 1.601F), new Vector3(45.327F, 331.491F, 198.947F), new Vector3(3.118F, 3.118F, 3.118F));
        idrs["idrsEngi"].AddDisplayRule(itemDisplay, "Pelvis", new Vector3(0.205F, 0.05F, -0.102F), new Vector3(0F, 114.381F, 354.036F), new Vector3(0.523F, 0.523F, 0.523F));
        idrs["idrsEngiTurret"].AddDisplayRule(itemDisplay, "Head", new Vector3(0.681F, 1.016F, -0.988F), new Vector3(0.775F, 180F, 202.127F), new Vector3(1.588F, 1.588F, 1.588F));
        idrs["idrsEngiWalkerTurret"].AddDisplayRule(itemDisplay, "Head", new Vector3(0.566F, 1.491F, -0.94F), new Vector3(7.103F, 180F, 204.769F), new Vector3(1.588F, 1.588F, 1.588F));
        idrs["idrsMage"].AddDisplayRule(itemDisplay, "Pelvis", new Vector3(-0.159F, -0.085F, -0.09F), new Vector3(356.235F, 252.299F, 344.311F), new Vector3(0.46F, 0.46F, 0.46F));
        idrs["idrsMerc"].AddDisplayRule(itemDisplay, "UpperArmL", new Vector3(0.161F, -0.006F, 0.001F), new Vector3(29.587F, 212.128F, 321.824F), new Vector3(0.493F, 0.493F, 0.493F));
        idrs["idrsTreebot"].AddDisplayRule(itemDisplay, "PlatformBase", new Vector3(1.062F, 0.782F, 0.174F), new Vector3(337.728F, 201.301F, 224.188F), new Vector3(1.056F, 1.056F, 1.056F));
        idrs["idrsLoader"].AddDisplayRule(itemDisplay, "MechUpperArmL", new Vector3(0.037F, 0.053F, -0.154F), new Vector3(335.055F, 244.872F, 293.27F), new Vector3(0.547F, 0.547F, 0.547F));
        idrs["idrsCroco"].AddDisplayRule(itemDisplay, "Head", new Vector3(1.926F, -0.053F, -0.112F), new Vector3(45.85F, 17.71F, 113.992F), new Vector3(5.36F, 5.36F, 5.36F));
        idrs["idrsCaptain"].AddDisplayRule(itemDisplay, "ClavicleL", new Vector3(0.021F, 0.136F, -0.226F), new Vector3(52.975F, 287.284F, 287.388F), new Vector3(0.587F, 0.587F, 0.587F));
        idrs["idrsRailGunner"].AddDisplayRule(itemDisplay, "Pelvis", new Vector3(0.155F, 0.079F, -0.029F), new Vector3(10.264F, 100.904F, 358.845F), new Vector3(0.434F, 0.434F, 0.434F));
        idrs["idrsVoidSurvivor"].AddDisplayRule(itemDisplay, "ShoulderL", new Vector3(0.063F, 0.289F, 0.052F), new Vector3(13.815F, 321.452F, 169.227F), new Vector3(0.597F, 0.597F, 0.597F));
        idrs["idrsScav"].AddDisplayRule(itemDisplay, "Weapon", new Vector3(3.037F, 8.08F, 2.629F), new Vector3(45.304F, 318.616F, 106.156F), new Vector3(5.5F, 5.5F, 5.5F));

        yield return texBurnMultipleEnemiesIcon;
        burnMultipleEnemies.SetAchievedIcon((Sprite)texBurnMultipleEnemiesIcon.asset);

        yield return OmniExplosionVFXQuick;
        impactArrowhead = Ivyl.ClonePrefab(OmniExplosionVFXQuick.Result, "ImpactArrowhead");
        if (impactArrowhead.TryGetComponent(out EffectComponent effectComponent))
        {
            effectComponent.soundName = "Play_item_proc_strengthenBurn";
        }
        if (impactArrowhead.TryGetComponent(out VFXAttributes vFXAttributes))
        {
            vFXAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
        }
        if (impactArrowhead.TryGetComponent(out OmniEffect omniEffect))
        {
            for (int i = omniEffect.omniEffectGroups.Length - 1; i >= 0; i--)
            {
                switch (omniEffect.omniEffectGroups[i].name)
                {
                    case "Scaled Smoke":
                    case "Smoke Ring":
                    case "Area Indicator Ring":
                    case "Unscaled Smoke":
                    case "Flames":
                        ArrayUtils.ArrayRemoveAtAndResize(ref omniEffect.omniEffectGroups, i);
                        break;
                }
            }
        }

        impactArrowheadStronger = Ivyl.ClonePrefab(impactArrowhead, "ImpactArrowHeadStronger");
        yield return matOmniHitspark3Gasoline;
        impactArrowheadStronger.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = matOmniHitspark3Gasoline.Result;
    }
}