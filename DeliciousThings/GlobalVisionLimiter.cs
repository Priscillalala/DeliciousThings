namespace DeliciousThings;

public class GlobalVisionLimiter : MonoBehaviour
{
    public float visionDistance;

    public void OnEnable()
    {
        On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
    }

    public void OnDisable()
    {
        On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
    }

    private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
    {
        orig(self);
        if (!self.isPlayerControlled) 
        {
            self.visionDistance = Mathf.Min(self.visionDistance, visionDistance);
        }
    }
}
