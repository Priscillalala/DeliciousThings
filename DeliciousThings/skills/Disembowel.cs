using HG;
using RoR2.Skills;
using IvyLibrary;
using EntityStates;

namespace DeliciousThings;

partial class DeliciousContent
{
    public partial class Disembowel : SkillDef, IStaticContent
    {
        public static Disembowel Instance { get; private set; }

        const string SECTION = "Disembowel";
        public readonly bool enabled = SkillsConfig.Bind(SECTION, string.Format(CONTENT_ENABLED_FORMAT, SECTION), true).Value;
        public readonly float damageCoefficient = SkillsConfig.Bind(SECTION, "Damage Coefficient", 2f).Value;

        public DamageAPI.ModdedDamageType superBleedOnHit;
        public GameObject crocoSuperBiteEffect;
        public UnlockableDef unlockableDef;

        public void Awake()
        {
            if (enabled)
            {
                Instance = this;

                skillName = string.Format(IDENTIFIER_FORMAT, "CrocoSuperBite");
                ((ScriptableObject)this).name = skillName;
                this.AutoPopulateTokens();
                activationState = new SerializableEntityStateType(typeof(EntityStates.Croco.SuperBite));
                activationStateMachineName = "Weapon";
                baseRechargeInterval = 10f;
                interruptPriority = InterruptPriority.PrioritySkill;
                keywordTokens = ["KEYWORD_POISON", "KEYWORD_SLAYER", "FSS_KEYWORD_BLEED", "KEYWORD_SUPERBLEED"];
                superBleedOnHit = DamageAPI.ReserveDamageType();

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
            CrocoBeatArenaFast.Instance?.SetAchievedIcon(icon);
            yield return CrocoBodySpecialFamily;
            CrocoBodySpecialFamily.Result.AddSkill(this, unlockableDef);

            crocoSuperBiteEffect = Ivyl.ClonePrefab(CrocoBiteEffect.Result, "CrocoSuperBiteEffect");
            if (crocoSuperBiteEffect.transform.TryFind("Goo", out Transform goo) && goo.TryGetComponent(out ParticleSystemRenderer gooRenderer))
            {
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

        public class Unlockable : UnlockableDef
        {
            public new void Awake()
            {
                if (Instance)
                {
                    Instance.unlockableDef = this;

                    cachedName = string.Format(UNLOCKABLE_SKILL_FORMAT, Instance.skillName);
                    nameToken = Instance.skillNameToken;

                    base.Awake();
                } 
                else DestroyImmediate(this);
            }
        }
    }
}