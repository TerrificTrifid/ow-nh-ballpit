using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

[HarmonyPatch]
public class CactusHazardPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.IsInvulnerable))]
    public static bool HazardDetector_IsInvulnerable_Prefix(ref bool __result, HazardDetector __instance)
    {
        bool flag = PlayerState.IsInsideShip();
        if (flag)
        {
            foreach (HazardVolume hv in __instance._activeVolumes)
            {
                if (hv.GetFirstContactDamageType() == InstantDamageType.Puncture) flag = false;
            }
        }
        __result = __instance._isPlayerDetector && (flag || PlayerState.IsInsideTheEye());
        return false;
    }
}
