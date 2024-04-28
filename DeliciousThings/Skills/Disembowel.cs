using HG;
using RoR2.Skills;
using IvyLibrary;
using EntityStates;
using DeliciousThings.Achievements;

namespace DeliciousThings.Skills;

public partial class Disembowel : SkillDef, Delicious.IStaticContent, Delicious.IUnlockableDefProvider
{
    public static ConfigFile Config => Delicious.SkillsConfig;

    public UnlockableDef UnlockableDef { get; set; }

    const string SECTION = "Disembowel";
    public readonly bool enabled = Config.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, SECTION), true).Value;
    public readonly float damageCoefficient = Config.Bind(SECTION, "Damage Coefficient", 2f).Value;

    public AchievementDef crocoBeatArenaFast;
    public DamageAPI.ModdedDamageType superBleedOnHit;
    public GameObject crocoSuperBiteEffect;

    public void Awake()
    {
        if (enabled)
        {
            skillName = string.Format(Delicious.IDENTIFIER_FORMAT, "CrocoSuperBite");
            ((ScriptableObject)this).name = skillName;
            this.AutoPopulateTokens();
            activationState = new SerializableEntityStateType(typeof(EntityStates.Croco.SuperBite));
            activationStateMachineName = "Weapon";
            baseRechargeInterval = 10f;
            interruptPriority = InterruptPriority.PrioritySkill;
            keywordTokens = ["KEYWORD_POISON", "KEYWORD_SLAYER", "FSS_KEYWORD_BLEED", "KEYWORD_SUPERBLEED"];
            superBleedOnHit = DamageAPI.ReserveDamageType();

            UnlockableDef = CreateInstance<UnlockableDef>();
            UnlockableDef.cachedName = string.Format(Delicious.UNLOCKABLE_SKILL_FORMAT, skillName);
            UnlockableDef.nameToken = skillNameToken;

            crocoBeatArenaFast = new AchievementDef
            {
                // Match achievement identifiers from 1.6.1
                identifier = "FSS_CrocoBeatArenaFast",
                unlockableRewardIdentifier = UnlockableDef.cachedName,
                prerequisiteAchievementIdentifier = "BeatArena",
                type = typeof(CrocoBeatArenaFastAchievement),
                serverTrackerType = typeof(BurnMultipleEnemiesAchievement.ServerAchievement),
            };
            crocoBeatArenaFast.AutoPopulateTokens();
            crocoBeatArenaFast.Register();

            UnlockableDef.PopulateUnlockStrings(crocoBeatArenaFast);

            Events.GlobalEventManager.onHitEnemyAcceptedServer += GlobalEventManager_onHitEnemyAcceptedServer;
        }
        else DestroyImmediate(this);
    }

    private void GlobalEventManager_onHitEnemyAcceptedServer(DamageInfo damageInfo, GameObject victim, uint? dotMaxStacksFromAttacker)
    {
        if (damageInfo.HasModdedDamageType(superBleedOnHit) && victim)
        {
            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.SuperBleed, 15f * damageInfo.procCoefficient, 1f, dotMaxStacksFromAttacker);
        }
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var texCrocoSuperBiteIcon = assets.LoadAssetAsync<Sprite>("texCrocoSuperBiteIcon");
        var CrocoBodySpecialFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Croco/CrocoBodySpecialFamily.asset");

        var CrocoBiteEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBiteEffect.prefab");
        var matCrocoGooSmall2 = Addressables.LoadAssetAsync<Material>("RoR2/Base/Croco/matCrocoGooSmall2.mat");
        var texRampPoison = Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampPoison.png");

        yield return texCrocoSuperBiteIcon;
        icon = (Sprite)texCrocoSuperBiteIcon.asset;
        crocoBeatArenaFast.SetAchievedIcon(icon);
        yield return CrocoBodySpecialFamily;
        CrocoBodySpecialFamily.Result.AddSkill(this, UnlockableDef);

        yield return CrocoBiteEffect;
        crocoSuperBiteEffect = Ivyl.ClonePrefab(CrocoBiteEffect.Result, "CrocoSuperBiteEffect");
        if (crocoSuperBiteEffect.transform.TryFind("Goo", out Transform goo) && goo.TryGetComponent(out ParticleSystemRenderer gooRenderer))
        {
            yield return matCrocoGooSmall2;
            gooRenderer.sharedMaterial = matCrocoGooSmall2.Result;
        }
        const float SCALE_MULTIPLIER = 1.2f;
        if (crocoSuperBiteEffect.transform.TryFind("SwingTrail", out Transform swingTrail))
        {
            swingTrail.localScale *= SCALE_MULTIPLIER;
            if (swingTrail.TryGetComponent(out ParticleSystemRenderer swingTrailRenderer))
            {
                swingTrailRenderer.sharedMaterial = new Material(swingTrailRenderer.sharedMaterial);
                swingTrailRenderer.sharedMaterial.SetColor("_TintColor", new Color32(121, 255, 107, 255));
                yield return texRampPoison;
                swingTrailRenderer.sharedMaterial.SetTexture("_RemapTex", texRampPoison.Result);
            }
        }
        if (crocoSuperBiteEffect.transform.TryFind("SwingTrail, Distortion", out Transform swingTrailDistortion))
        {
            swingTrailDistortion.localScale *= SCALE_MULTIPLIER;
        }
        if (crocoSuperBiteEffect.transform.TryFind("Flash", out Transform flash))
        {
            flash.localScale *= SCALE_MULTIPLIER;
        }
    }
}