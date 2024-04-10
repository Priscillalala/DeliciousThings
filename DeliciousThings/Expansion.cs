using IvyLibrary;
using RoR2.ExpansionManagement;
using RoR2.Items;

namespace DeliciousThings;

public partial class Expansion : ExpansionDef, Delicious.IStaticContent
{
    public static Expansion Instance { get; private set; }

    public void Awake()
    {
        Instance = this;

        name = "DeliciousThings";
        nameToken = "DELICIOUSTHINGS_NAME";
        descriptionToken = "DELICIOUSTHINGS_DESCRIPTION";
    }

    public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
    {
        var texFreeItemFridayExpansionIcon = assets.LoadAssetAsync<Sprite>("texFreeItemFridayExpansionIcon");
        var texUnlockIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png");

        yield return texFreeItemFridayExpansionIcon;
        iconSprite = texFreeItemFridayExpansionIcon.asset as Sprite;
        yield return texUnlockIcon;
        disabledIconSprite = texUnlockIcon.Result;
    }
}