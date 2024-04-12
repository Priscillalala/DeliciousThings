namespace DeliciousThings;

public class SetSceneObjectsActive : MonoBehaviour
{
    public List<GameObject> objectsToActivate = [];
    public List<GameObject> objectsToDeactivate = [];

    public void OnEnable()
    {
        foreach (GameObject toActivate in  objectsToActivate) 
        {
            toActivate?.SetActive(true);
        }
        foreach (GameObject toDeactivate in objectsToDeactivate)
        {
            toDeactivate?.SetActive(false);
        }
    }
}
