using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using HarmonyLib;
using System.Reflection;
using ModLoader;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using VTOLAPI;

[ItemId("CHEESE.SCAMPLUS")]
public class SCamPlus : VtolMod
{
    public enum SpectatorBehaviorsPlus
    {
        //Stock Behaviours
        Stationary,
        FlyAlong,
        Fixed,
        Chase,
        PresetViews,
        SmoothLook,
        Camcorder,
        //Custom Behaviours
        //StabilisedCamcorder
        //CanopyCamcorder
        AceCombat,
        //AceMissile
        TGP,
        SmoothTGP,
        HUD,
        ExtCam,
        //SmoothZoom
        //GroundTrack2 --- Ground view that tracks 2 aircraft
        //OverShoulder
        //FreeCam
        //FreeCamParented
        //SmartRandom
    }

    public enum UpType
    {
        Stock,
        WorldUp,
        AircraftUp,
        GUp,
        HeadUp,
        Itself
    }

    public static FlybyCameraMFDPage sCam;
    public static Traverse sCamTraverse;
    //public static int ammountOfModes;

    public static SpectatorBehaviorsPlus currentBehaviour;

    public static TargetingMFDPage tgpMFD;
    public static ExternalCamManager extCamManager;
    public static float lastRenderTime;


    public UnityAction<bool> playerVisibility_changed;
    public static bool playerVisibility = true;

    public UnityAction<int> up_changed;
    public static UpType upType = UpType.Stock;
    public static Vector3 lastUp;

    public static Actor player;
    public static VTOLVehicles aircrarftType;
    public static Actor targetActor;
    public static float targetChangeTimer;
    public static Vector3D lastTargetPositon;
    public static Vector3 lastTargetVelocity;
    
    public static Transform head;

    public static float[] fovList;

    public static int normalBitmask;
    public static int fpvBitmask;
    public static int headBitmask;

    public UnityAction<float> trackMinTime_changed;
    public static float trackMinTime = 1;


    public UnityAction<float> aceMinFov_changed;
    public UnityAction<float> aceMaxFov_changed;
    public static MinMax aceFovRange = new MinMax(5, 60);
    public static float aceMaxDistance;

    public UnityAction<float> acePosLerp_changed;
    public static float acePosLerp = 5;

    public UnityAction<float> aceRotLerp_changed;
    public static float aceRotLerp = 5;

    public UnityAction<float> aceZoomLead_changed;
    public static float aceZoomLead = 2;

    public UnityAction<float> aceZoomLerp_changed;
    public static float aceZoomLerp = 0.5f;

    //smoothing variables
    public static Vector3 lastOffset;
    public static Quaternion lastRotation;
    public static float lastFov = 60;

    //freecam settings
    public static Vector2 rotation = new Vector2(0, 0);
    public static float sensitivity = 2;
    public static Vector3D position;

    public static List<CameraMode> cameraModes;
    //public static Settings settings;
    
    public string ModFolder;
    private void Awake()
    {
        ModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        VTAPI.SceneLoaded += SceneLoaded;
        VTAPI.MissionReloaded += MissionReloaded;

        //ammountOfModes = Enum.GetValues(typeof(SpectatorBehaviorsPlus)).Length;

        cameraModes = new List<CameraMode>();
        //settings = new Settings(this);
        /*settings.CreateCustomLabel("S-Cam+ Settings");

        settings.CreateCustomLabel("");

        up_changed += up_Setting;
        settings.CreateIntSetting("Camera Up Vector: ", up_changed, (int)upType, 0, 4);
        settings.CreateCustomLabel("0 = Unmodified/Same as Stock");
        settings.CreateCustomLabel("1 = World Up");
        settings.CreateCustomLabel("2 = Aircraft Up");
        settings.CreateCustomLabel("3 = G-Force Up");
        settings.CreateCustomLabel("4 = Head Up");
        settings.CreateCustomLabel("5 = Itself / No Up Vector");

        settings.CreateCustomLabel("Note: Camera is still upright in preview.");

        settings.CreateCustomLabel("");

        playerVisibility_changed += playerVisibility_Setting;
        settings.CreateBoolSetting("Player Model Visibility", playerVisibility_changed, playerVisibility);

        settings.CreateCustomLabel("");

        settings.CreateCustomLabel("Auto-Tracking Settings");

        trackMinTime_changed += trackMinTime_Setting;
        settings.CreateCustomLabel("Minimum Tracking Time:");
        settings.CreateCustomLabel("(How long a target must be tracked");
        settings.CreateCustomLabel("before changing for closer/locked)");
        settings.CreateCustomLabel("(Also how long its tracked after death)");
        settings.CreateFloatSetting("(Default = 1)", trackMinTime_changed, trackMinTime, 0, 10);

        settings.CreateCustomLabel("");

        settings.CreateCustomLabel("AceCombat Camera Settings");

        aceMinFov_changed += aceMinFov_Setting;
        settings.CreateCustomLabel("Minimum FOV:");
        settings.CreateFloatSetting("(Default = 5)", aceMinFov_changed, aceFovRange.min, 5, 100);

        aceMinFov_changed += aceMaxFov_Setting;
        settings.CreateCustomLabel("Maximum FOV:");
        settings.CreateFloatSetting("(Default = 60)", aceMaxFov_changed, aceFovRange.max, 5, 100);

        acePosLerp_changed += acePosLerp_Setting;
        settings.CreateCustomLabel("Position Lerp:");
        settings.CreateFloatSetting("(Default = 5)", acePosLerp_changed, acePosLerp, 0, 100);

        aceRotLerp_changed += aceRotLerp_Setting;
        settings.CreateCustomLabel("Rotation Lerp:");
        settings.CreateFloatSetting("(Default = 5)", aceRotLerp_changed, aceRotLerp, 0, 100);

        aceZoomLead_changed += aceZoomLead_Setting;
        settings.CreateCustomLabel("Zoom Lead (in secconds):");
        settings.CreateFloatSetting("(Default = 2)", aceZoomLead_changed, aceZoomLead, 0, 10);

        aceZoomLerp_changed += aceZoomLerp_Setting;
        settings.CreateCustomLabel("Zoom Lerp:");
        settings.CreateFloatSetting("(Default = 0.5f)", aceZoomLerp_changed, aceZoomLerp, 0, 100);

        settings.CreateCustomLabel("");

        settings.CreateCustomLabel("Please feel free to @ me on the discord if");
        settings.CreateCustomLabel("you think of any more features I could add!");
        VTOLAPI.CreateSettingsMenu(settings);*/

        AddNewCameraMode(new CameraMode_AceCombat("ace", "AceCombat"));
        AddNewCameraMode(new CameraMode_TGP("tgp", "TGP"));
        AddNewCameraMode(new CameraMode_SmoothTGP("smoothTgp", "SmoothTGP"));
        AddNewCameraMode(new CameraMode_HUD("hud", "HUD"));
        AddNewCameraMode(new CameraMode_ExtCam("ext", "ExtCam"));
        //AddNewCameraMode(new CameraMode_FreeCam("free", "FreeCam"));//scrapped cause it doesnt work
        AddNewCameraMode(new CameraMode_SmoothZoom("zoom", "SmoothZoom"));
    }
    
    public override void UnLoad()
    {
        // Unloaded
    }

    public void trackMinTime_Setting(float newval)
    {
        trackMinTime = newval;
    }

    public void up_Setting(int newval)
    {
        upType = (UpType)newval;
    }

    public void playerVisibility_Setting(bool visible)
    {
        playerVisibility = visible;
    }

    public void aceMinFov_Setting(float newval)
    {
        aceFovRange.min = newval;
    }

    public void aceMaxFov_Setting(float newval)
    {
        aceFovRange.max = newval;
    }

    public void acePosLerp_Setting(float newval)
    {
        acePosLerp = newval;
    }

    public void aceRotLerp_Setting(float newval)
    {
        aceRotLerp = newval;
    }

    public void aceZoomLead_Setting(float newval)
    {
        aceZoomLead = newval;
    }

    public void aceZoomLerp_Setting(float newval)
    {
        aceZoomLerp = newval;
    }

    //This function is called every time a scene is loaded. this behaviour is defined in Awake().
    void SceneLoaded(VTScenes scene)
    {
        switch (scene)
        {
            case VTScenes.Akutan:
            case VTScenes.CustomMapBase:
                StartCoroutine("SetupScene");
                Cursor.visible = false;
                break;
            default:
                Cursor.visible = true;
                break;
        }
    }

    private void MissionReloaded()
    {
        StartCoroutine("SetupScene");
    }

    private IEnumerator SetupScene()
    {
        while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady || FlightSceneManager.instance.switchingScene)
        {
            yield return null;
        }

        aircrarftType = VTAPI.GetPlayersVehicleEnum();

        tgpMFD = VTAPI.GetPlayersVehicleGameObject().GetComponentInChildren<TargetingMFDPage>();
        player = VTAPI.GetPlayersVehicleGameObject().GetComponent<Actor>();

        aceMaxDistance = Mathf.Tan((90 - aceFovRange.min) * Mathf.Deg2Rad) * 30;
    }

    private void FixedUpdate() {
        if (TargetManager.instance != null) {
            if (player == null)
            {
                if (FlightSceneManager.instance != null) {
                    player = FlightSceneManager.instance.playerActor;
                }
            }
            else
            {
                Actor newTarget = null;

                if (sCam != null) {
                    if (sCam.weaponManager.tsc != null && sCam.weaponManager.tsc.GetCurrentSelectionActor() != null)
                    {
                        newTarget = sCam.weaponManager.tsc.GetCurrentSelectionActor();
                    }
                    if (sCam.weaponManager.lockingRadar != null && sCam.weaponManager.lockingRadar.IsLocked())
                    {
                        newTarget = sCam.weaponManager.lockingRadar.currentLock.actor;
                    }
                    if (sCam.weaponManager.opticalTargeter != null && sCam.weaponManager.opticalTargeter.lockedActor != null)
                    {
                        newTarget = sCam.weaponManager.opticalTargeter.lockedActor;
                    }
                }

                if (FlightSceneManager.instance != null && newTarget == null)
                {
                    float distance = float.MaxValue;
                    foreach (Actor actor in TargetManager.instance.allActors)
                    {
                        if (actor != null)
                        {
                            if (actor != player && ValidTrackingRole(actor) && (actor.transform.position - player.transform.position).magnitude < distance)
                            {
                                distance = (actor.transform.position - player.transform.position).magnitude;
                                newTarget = actor;
                            }
                        }
                    }
                }

                if (newTarget != targetActor)
                {
                    targetChangeTimer += Time.fixedDeltaTime;
                    if (targetChangeTimer > trackMinTime) {
                        targetActor = newTarget;
                        targetChangeTimer = 0;
                    }
                }
                else {
                    targetChangeTimer = 0;
                }
            }
        }
    }

    private void Update()
    {
        if (targetActor != null)
        {
            lastTargetPositon = VTMapManager.WorldToGlobalPoint(targetActor.transform.position);
            lastTargetVelocity = targetActor.velocity;
        }
        else {
            lastTargetPositon += lastTargetVelocity * Time.deltaTime;
        }
    }

    public static void AddNewCameraMode(CameraMode cameraMode) {
        cameraModes.Add(cameraMode);
    }

    public static Vector3 GetTargetPos() {
        if (targetActor != null) {
            return targetActor.transform.position;
        }
        return VTMapManager.GlobalToWorldPoint(lastTargetPositon);
    }

    public static Vector3 GetTargetVel()
    {
        if (targetActor != null)
        {
            return targetActor.velocity;
        }
        return lastTargetVelocity;
    }

    public bool ValidTrackingRole(Actor actor) {
        switch (actor.role) {
            case Actor.Roles.Air:
                return true;
            case Actor.Roles.Missile:
                return actor.team == Teams.Enemy;
            default:
                return false;
        }
    }

    public static void UpdateBehaviourText(FlybyCameraMFDPage __instance)
    {
        Debug.Log("Updating behaviour text!");
        if ((bool)SCamPlus.sCamTraverse.Field("randomModes").GetValue())
        {
            __instance.behaviorText.text = "Random";
            return;
        }
        __instance.behaviorText.text = ((SCamPlus.SpectatorBehaviorsPlus)currentBehaviour).ToString();
        return;
    }

    public static Vector3 GetUpVector()
    {
        switch (upType) {
            case UpType.Stock:
            case UpType.WorldUp:
                return Vector3.up;
            case UpType.AircraftUp:
                if (player != null)
                {
                    return player.transform.up;
                }
                break;
            case UpType.GUp:
                if (player != null)
                {
                    return (player.flightInfo.acceleration + Physics.gravity).normalized;
                }
                break;
            case UpType.HeadUp:
                if (head != null)
                {
                    return head.up;
                }
                else
                {
                    head = VRHead.instance.transform;
                }
                break;
            case UpType.Itself:
                return lastUp;
            default:
                return Vector3.up;
        }
        return Vector3.up;
    }
}