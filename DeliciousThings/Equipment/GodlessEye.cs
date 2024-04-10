using IvyLibrary;

namespace DeliciousThings.Equipment;

public partial class GodlessEye : EquipmentDef, Delicious.IStaticContent, Delicious.INetworkedObjectPrefabProvider
{
    public static GodlessEye Instance { get; private set; }
    public static ConfigFile Config => Delicious.EquipmentConfig;

    const string SECTION = "Godless Eye";
    public readonly bool enabled = Config.Bind(SECTION, string.Format(Delicious.CONTENT_ENABLED_FORMAT, SECTION), true).Value;
    public readonly float range = Config.Bind(SECTION, "Range", 200f).Value;
    public readonly float duration = Config.Bind(SECTION, "Duration", 2f).Value;
    public readonly float maxConsecutiveEnemies = Config.Bind(SECTION, "Maximum Consecutive Enemies", 10).Value;

    public Consumed consumedDef;
    public GameObject delayedDeathHandler;

    public IEnumerable<GameObject> NetworkedObjectPrefabs => [delayedDeathHandler];

    public void Awake()
    {
        if (enabled)
        {
            Instance = this;

            name = string.Format(Delicious.IDENTIFIER_FORMAT, "DeathEye");
            AutoPopulateTokens();
            canDrop = true;
            isLunar = true;
            colorIndex = ColorCatalog.ColorIndex.LunarItem;
            cooldown = 60f;
            appearsInSinglePlayer = false;
            canBeRandomlyTriggered = false;
            enigmaCompatible = false;

            Delicious.EquipmentActivationFunctions[this] = FireDeathEye;
        }
        else DestroyImmediate(this);
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var PickupDeathEye = assets.LoadAssetAsync<GameObject>("PickupDeathEye");
        var texDeathEyeIcon = assets.LoadAssetAsync<Sprite>("texDeathEyeIcon");
        var matMSObeliskLightning = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSObeliskLightning.mat");
        var matMSObeliskHeart = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSObeliskHeart.mat");
        var matMSStarsLink = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSStarsLink.mat");
        var matJellyfishLightning = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matJellyfishLightning.mat");

        var MSObelisk = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/mysteryspace/MSObelisk.prefab");

        yield return texDeathEyeIcon;
        pickupIconSprite = (Sprite)texDeathEyeIcon.asset;
        yield return PickupDeathEye;
        pickupModelPrefab = (GameObject)PickupDeathEye.asset;

        MeshRenderer modelRenderer = pickupModelPrefab.transform.Find("mdlDeathEye").GetComponent<MeshRenderer>();
        Material[] sharedMaterials = modelRenderer.sharedMaterials;
        yield return matMSObeliskLightning;
        sharedMaterials[1] = matMSObeliskLightning.Result;
        modelRenderer.sharedMaterials = sharedMaterials;

        consumedDef.pickupModelPrefab = Ivyl.ClonePrefab(pickupModelPrefab, "PickupDeathEyeConsumed");
        DestroyImmediate(consumedDef.pickupModelPrefab.transform.Find("EyeBallFX").gameObject);

        yield return matMSObeliskHeart;
        yield return matMSStarsLink;
        yield return matJellyfishLightning;
        pickupModelPrefab.transform.Find("EyeBallFX/Weird Sphere").GetComponent<ParticleSystemRenderer>().sharedMaterial = matMSObeliskHeart.Result;
        pickupModelPrefab.transform.Find("EyeBallFX/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().trailMaterial = matMSStarsLink.Result;
        pickupModelPrefab.transform.Find("EyeBallFX/Lightning").GetComponent<ParticleSystemRenderer>().sharedMaterial = matJellyfishLightning.Result;

        Ivyl.SetupModelPanelParameters(pickupModelPrefab, new ModelPanelParams(Vector3.zero, 3, 10));

        GameObject displayModelPrefab = Ivyl.CreatePrefab("DisplayDeathEye");
        displayModelPrefab.AddComponent<ItemDisplay>();
        ItemFollower itemFollower = displayModelPrefab.AddComponent<ItemFollower>();
        itemFollower.targetObject = displayModelPrefab;
        itemFollower.followerPrefab = pickupModelPrefab;
        itemFollower.distanceDampTime = 0.005f;
        itemFollower.distanceMaxSpeed = 200f;

        GameObject consumedDisplayModelPrefab = Ivyl.ClonePrefab(displayModelPrefab, "DisplayDeathEyeConsumed");
        ItemFollower consumedItemFollower = consumedDisplayModelPrefab.GetComponent<ItemFollower>();
        consumedItemFollower.targetObject = consumedDisplayModelPrefab;
        consumedItemFollower.followerPrefab = consumedDef.pickupModelPrefab;

        yield return Delicious.IDRS;
        static void AddDisplayRules(ItemDisplaySpec itemDisplay)
        {
            var idrs = Delicious.IDRS.Result;
            idrs["idrsCommando"].AddDisplayRule(itemDisplay, "Head", new Vector3(0.001F, 0.545F, -0.061F), new Vector3(0F, 90F, 0F), new Vector3(0.069F, 0.069F, 0.069F));
            idrs["idrsHuntress"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.002F, 0.486F, -0.158F), new Vector3(359.97F, 89.949F, 345.155F), new Vector3(0.067F, 0.067F, 0.067F));
            idrs["idrsBandit2"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.001F, 0.367F, -0.002F), new Vector3(0F, 89.995F, 0.001F), new Vector3(0.066F, 0.066F, 0.066F));
            idrs["idrsToolbot"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.002F, 4.049F, 3.237F), new Vector3(0.708F, 89.264F, 50.748F), new Vector3(0.111F, 0.111F, 0.111F));
            idrs["idrsEngi"].AddDisplayRule(itemDisplay, "Chest", new Vector3(-0.001F, 1.049F, 0.174F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F));
            idrs["idrsMage"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.002F, 0.328F, 0.003F), new Vector3(0F, 90F, 0F), new Vector3(0.055F, 0.055F, 0.055F));
            idrs["idrsMerc"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.002F, 0.452F, -0.01F), new Vector3(0F, 90F, 0F), new Vector3(0.06F, 0.06F, 0.06F));
            idrs["idrsTreebot"].AddDisplayRule(itemDisplay, "Chest", new Vector3(0.157F, 3.44F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.148F, 0.148F, 0.148F));
            idrs["idrsLoader"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.002F, 0.442F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.089F, 0.089F, 0.089F));
            idrs["idrsCroco"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.036F, -0.134F, 3.731F), new Vector3(0.141F, 89.889F, 298.828F), new Vector3(0.152F, 0.152F, 0.152F));
            idrs["idrsCaptain"].AddDisplayRule(itemDisplay, "Base", new Vector3(-0.03F, 0.199F, -1.281F), new Vector3(0F, 90F, 90F), new Vector3(0.062F, 0.062F, 0.062F));
            idrs["idrsRailGunner"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.001F, 0.363F, -0.089F), new Vector3(0F, 90F, 0F), new Vector3(0.056F, 0.056F, 0.056F));
            idrs["idrsVoidSurvivor"].AddDisplayRule(itemDisplay, "Head", new Vector3(-0.006F, 0.322F, -0.217F), new Vector3(357.745F, 91.815F, 321.156F), new Vector3(0.069F, 0.069F, 0.069F));
            idrs["idrsEquipmentDrone"].AddDisplayRule(itemDisplay, "HeadCenter", new Vector3(0F, 0F, 1.987F), new Vector3(0F, 90F, 90F), new Vector3(0.351F, 0.351F, 0.351F));
        }
        AddDisplayRules(new ItemDisplaySpec(this, displayModelPrefab));
        AddDisplayRules(new ItemDisplaySpec(consumedDef, consumedDisplayModelPrefab));

        delayedDeathHandler = Ivyl.ClonePrefab(MSObelisk.Result.transform.Find("Stage1FX").gameObject, "DelayedDeathHandler");
        delayedDeathHandler.SetActive(true);
        delayedDeathHandler.AddComponent<NetworkIdentity>();
        delayedDeathHandler.AddComponent<DelayedDeathEye>();
        delayedDeathHandler.AddComponent<DestroyOnTimer>().duration = duration;
        DestroyImmediate(delayedDeathHandler.transform.Find("LongLifeNoiseTrails, Bright").gameObject);
        DestroyImmediate(delayedDeathHandler.transform.Find("PersistentLight").gameObject);
    }

    public bool FireDeathEye(EquipmentSlot equipmentSlot)
    {
        if (!equipmentSlot.healthComponent || !equipmentSlot.healthComponent.alive)
        {
            return false;
        }
        Vector3 position = equipmentSlot.characterBody?.corePosition ?? equipmentSlot.transform.position;
        DelayedDeathEye delayedDeathEye = Instantiate(delayedDeathHandler, position, Quaternion.identity).GetComponent<DelayedDeathEye>();

        TeamMask teamMask = TeamMask.allButNeutral;
        if (equipmentSlot.teamComponent)
        {
            teamMask.RemoveTeam(equipmentSlot.teamComponent.teamIndex);
        }
        delayedDeathEye.cleanupTeams = teamMask;

        List<DelayedDeathEye.DeathGroup> deathGroups = new List<DelayedDeathEye.DeathGroup>();
        int consecutiveEnemies = 0;
        BodyIndex currentBodyIndex = BodyIndex.None;
        List<CharacterBody> currentVictims = new List<CharacterBody>();
        foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
        {
            if (teamMask.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - position).sqrMagnitude <= range * range)
            {
                if (body.bodyIndex != currentBodyIndex || consecutiveEnemies >= maxConsecutiveEnemies)
                {
                    currentBodyIndex = body.bodyIndex;
                    consecutiveEnemies = 0;
                    if (currentVictims.Count > 0)
                    {
                        deathGroups.Add(new DelayedDeathEye.DeathGroup
                        {
                            victimBodies = new List<CharacterBody>(currentVictims),
                        });
                    }
                    currentVictims.Clear();
                }
                currentVictims.Add(body);
                consecutiveEnemies++;
            }
        }
        if (currentVictims.Count > 0)
        {
            deathGroups.Add(new DelayedDeathEye.DeathGroup
            {
                victimBodies = new List<CharacterBody>(currentVictims),
            });
        }
        currentVictims.Clear();
        deathGroups.Add(new DelayedDeathEye.DeathGroup
        {
            victimBodies = new List<CharacterBody>() { equipmentSlot.characterBody }
        });
        if (deathGroups.Count > 0)
        {
            float durationBetweenDeaths = duration / deathGroups.Count;
            for (int i = 0; i < deathGroups.Count; i++)
            {
                DelayedDeathEye.DeathGroup group = deathGroups[i];
                group.time = Run.FixedTimeStamp.now + (durationBetweenDeaths * i);
                delayedDeathEye.EnqueueDeath(group);
            }
        }
        NetworkServer.Spawn(delayedDeathEye.gameObject);

        if (equipmentSlot.characterBody?.inventory)
        {
            CharacterMasterNotificationQueue.SendTransformNotification(equipmentSlot.characterBody.master, equipmentSlot.characterBody.inventory.currentEquipmentIndex, consumedDef.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            equipmentSlot.characterBody.inventory.SetEquipmentIndex(consumedDef.equipmentIndex);
        }
        return true;
    }

    public class Consumed : EquipmentDef, Delicious.IStaticContent
    {
        public void Awake()
        {
            if (Instance)
            {
                Instance.consumedDef = this;

                name = string.Format(Delicious.IDENTIFIER_FORMAT, "DeathEyeConsumed");
                AutoPopulateTokens();
                canDrop = false;
                isLunar = true;
                colorIndex = ColorCatalog.ColorIndex.Unaffordable;
                appearsInSinglePlayer = false;
                canBeRandomlyTriggered = false;
                enigmaCompatible = false;
            }
            else DestroyImmediate(this);
        }

        public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
        {
            var texDeathEyeConsumedIcon = assets.LoadAssetAsync<Sprite>("texDeathEyeConsumedIcon");

            yield return texDeathEyeConsumedIcon;
            pickupIconSprite = (Sprite)texDeathEyeConsumedIcon.asset;
        }
    }

    public class Unlockable : UnlockableDef
    {
        public new void Awake()
        {
            if (Instance)
            {
                Instance.unlockableDef = this;

                cachedName = string.Format(Delicious.UNLOCKABLE_ITEM_FORMAT, Instance.name);
                nameToken = Instance.nameToken;

                base.Awake();
            }
            else DestroyImmediate(this);
        }
    }

    public class DelayedDeathEye : MonoBehaviour
    {
        public struct DeathGroup
        {
            public Run.FixedTimeStamp time;
            public List<CharacterBody> victimBodies;
        }

        public Queue<DeathGroup> deathQueue = new Queue<DeathGroup>();
        public TeamMask cleanupTeams = TeamMask.none;
        private bool hasRunCleanup;
        private GameObject destroyEffectPrefab;

        public void EnqueueDeath(DeathGroup death)
        {
            deathQueue.Enqueue(death);
        }

        public void Awake()
        {
            destroyEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BrittleDeath.prefab").WaitForCompletion();
        }

        public void Start()
        {
            Util.PlaySound("Play_vagrant_R_explode", base.gameObject);
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            if (deathQueue.Count <= 0)
            {
                RunCleanup();
                enabled = false;
                return;
            }
            if (deathQueue.Peek().time.hasPassed)
            {
                List<CharacterBody> victimBodies = deathQueue.Dequeue().victimBodies;
                foreach (CharacterBody victim in victimBodies)
                {
                    DestroyVictim(victim);
                }
            }
        }

        public void RunCleanup()
        {
            if (hasRunCleanup)
            {
                return;
            }
            if (CharacterBody.readOnlyInstancesList.Count > 0)
            {
                for (int i = CharacterBody.readOnlyInstancesList.Count - 1; i >= 0; i--)
                {
                    CharacterBody body = CharacterBody.readOnlyInstancesList[i];
                    if (body.teamComponent && cleanupTeams.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - transform.position).sqrMagnitude <= Instance.range * Instance.range)
                    {
                        DestroyVictim(body);
                    }
                }
            }
            hasRunCleanup = true;
        }

        public void DestroyVictim(CharacterBody victim)
        {
            if (!victim)
            {
                return;
            }
            if (victim.master)
            {
                if (victim.master.destroyOnBodyDeath)
                {
                    Destroy(victim.master.gameObject, 1f);
                }
                victim.master.preventGameOver = false;
            }
            EffectManager.SpawnEffect(destroyEffectPrefab, new EffectData
            {
                origin = victim.corePosition,
                scale = victim.radius
            }, true);
            Destroy(victim.gameObject);
        }
    }
}