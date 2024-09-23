using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMode_FreeCam : CameraMode
{
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction cycleFovAction;
    private InputAction speedBoostAction;

    public CameraMode_FreeCam(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;

        // Initialize input actions
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        lookAction = new InputAction("Look", binding: "<Mouse>/delta");
        cycleFovAction = new InputAction("CycleFov", binding: "<Mouse>/leftButton");
        speedBoostAction = new InputAction("SpeedBoost", binding: "<Keyboard>/leftShift");

        moveAction.Enable();
        lookAction.Enable();
        cycleFovAction.Enable();
        speedBoostAction.Enable();
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {
        SCamPlus.position = VTMapManager.WorldToGlobalPoint(mfdPage.flybyCam.transform.position);
    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (cycleFovAction.triggered)
        {
            mfdPage.CycleFovs();
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        SCamPlus.rotation.y += lookInput.x * SCamPlus.sensitivity / 60f * mfdPage.flybyCam.fieldOfView;
        SCamPlus.rotation.x += lookInput.y * -SCamPlus.sensitivity / 60f * mfdPage.flybyCam.fieldOfView;

        SCamPlus.rotation.x = Mathf.Clamp(SCamPlus.rotation.x, -90, 90);
        mfdPage.flybyCam.transform.eulerAngles = (Vector2)SCamPlus.rotation;

        float speedFactor = speedBoostAction.ReadValue<float>() > 0 ? 1000 : 10;
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        SCamPlus.position += (mfdPage.flybyCam.transform.forward * moveInput.y + mfdPage.flybyCam.transform.right * moveInput.x) * Time.deltaTime * speedFactor;
        mfdPage.flybyCam.transform.position = VTMapManager.GlobalToWorldPoint(SCamPlus.position);
        mfdPage.behaviorText.text = "FreeCam";
    }
}