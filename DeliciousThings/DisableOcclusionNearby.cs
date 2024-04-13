namespace DeliciousThings;

public class DisableOcclusionNearby : MonoBehaviour
{
    public List<CameraRigController> cameraRigControllers = [];
    public HashSet<CameraRigController> affectedCameraRigControllers = [];
    public float radius;

    public void OnEnable()
    {
        foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
        {
            OnCameraEnableGlobal(cameraRigController);
        }
        CameraRigController.onCameraEnableGlobal += OnCameraEnableGlobal;
        CameraRigController.onCameraDisableGlobal += OnCameraDisableGlobal;
    }

    public void OnDisable()
    {
        CameraRigController.onCameraEnableGlobal -= OnCameraEnableGlobal;
        CameraRigController.onCameraDisableGlobal -= OnCameraDisableGlobal;
        cameraRigControllers.Clear();
        affectedCameraRigControllers.Clear();
    }

    public void Update()
    {
        foreach (CameraRigController cameraRigController in cameraRigControllers) 
        {
            if ((cameraRigController.transform.position - transform.position).sqrMagnitude <= radius * radius) 
            {
                if (affectedCameraRigControllers.Add(cameraRigController) && cameraRigController.sceneCam)
                {
                    cameraRigController.sceneCam.useOcclusionCulling = false;
                }
            }
            else
            {
                if (affectedCameraRigControllers.Remove(cameraRigController) && cameraRigController.sceneCam)
                {
                    cameraRigController.sceneCam.useOcclusionCulling = true;
                }
            }
        }
    }

    private void OnCameraEnableGlobal(CameraRigController cameraRigController)
    {
        if (cameraRigController.sceneCam && cameraRigController.sceneCam.useOcclusionCulling)
        {
            cameraRigControllers.Add(cameraRigController);
        }
    }

    private void OnCameraDisableGlobal(CameraRigController cameraRigController)
    {
        cameraRigControllers.Remove(cameraRigController);
    }
}
