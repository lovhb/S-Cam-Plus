﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using VTOLAPI;

public class CameraMode_TGP : CameraMode
{
    private TargetingMFDPage tgpMFD;

    public CameraMode_TGP(string name, string shownName) : base(name, shownName)
    {
        this.name = name;
        this.shownName = shownName;
    }

    public override void Start(FlybyCameraMFDPage mfdPage)
    {
        tgpMFD = VTAPI.GetPlayersVehicleGameObject().GetComponentInChildren<TargetingMFDPage>();
    }

    public override void LateUpdate(FlybyCameraMFDPage mfdPage)
    {
        if (tgpMFD != null)
        {
            mfdPage.flybyCam.transform.position = tgpMFD.targetingCamera.transform.position;
            mfdPage.flybyCam.transform.rotation = tgpMFD.targetingCamera.transform.rotation;
            mfdPage.flybyCam.fieldOfView = tgpMFD.targetingCamera.fieldOfView;
            mfdPage.flybyCam.nearClipPlane = 0.5f;
            shownName = "TGP";
        }
        else if (mfdPage.weaponManager.opticalTargeter != null)
        {
            mfdPage.flybyCam.transform.position = mfdPage.weaponManager.opticalTargeter.sensorTurret.pitchTransform.position;
            mfdPage.flybyCam.transform.rotation = mfdPage.weaponManager.opticalTargeter.sensorTurret.pitchTransform.rotation;
            mfdPage.flybyCam.fieldOfView = 60;
            shownName = "TGP Off";
        }
        else
        {
            shownName = "No TGP";
        }
    }
}