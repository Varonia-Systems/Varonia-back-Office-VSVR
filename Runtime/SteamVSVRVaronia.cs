using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using VaroniaBackOffice;
using static DebugVSVR;

public class SteamVSVRVaronia : MonoBehaviour
{


    [BoxGroup("Infos")] public bool Left_Hand_Ready;
    [BoxGroup("Infos")] public bool Right_Hand_Ready;
    [BoxGroup("Infos")] public bool HMD_Ready;
    [BoxGroup("Infos")] public bool HMD_HasActivity;
    [BoxGroup("Infos")] public float HMD_Battery;
    [BoxGroup("Infos")] public TrackingState LeftCtrlState;
    [BoxGroup("Infos")] public TrackingState RightCtrlState;

    [BoxGroup("Infos")] public int BigLagCount;


    [BoxGroup("Debug")] public Text liveStats, avgStats;


    private float lastXpos = 0;

    private string Info;


    RealTimeLatencyChart realTimeLatencyChart;


    IEnumerator Start()
    {

        realTimeLatencyChart = GetComponent<RealTimeLatencyChart>();

        DebugVSVR.Init();



        while (Config.VaroniaConfig == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1);


        try
        {

            var HeadSetName = SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_ModelNumber_String, 0);
            // Debug.Log(HeadSetName);

            if (!HeadSetName.Contains("Miramar") && !HeadSetName.Contains("Oculus Quest2"))
                Destroy(gameObject);
        }
        catch (System.Exception)
        {
            Destroy(gameObject);
        }





        while (SteamVR.instance != null)
        {


            if (DebugVSVR.LiveStatistics != null)
            {
                LiveStats();
                AVGStats();
            }



            Info = "";

            for (int i = 0; i < SteamVR.connected.Length; ++i)
            {
                try
                {

                    var A = "";
                    float B = 0f;
                    var C = "";
                    EDeviceActivityLevel E = new EDeviceActivityLevel();



                    if (OpenVR.System != null) A = SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_ModelNumber_String, (uint)i);
                    if (OpenVR.System != null) B = SteamVR.instance.GetFloatProperty(Valve.VR.ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, (uint)i);
                    if (OpenVR.System != null) C = SteamVR.instance.GetStringProperty(Valve.VR.ETrackedDeviceProperty.Prop_RenderModelName_String, (uint)i);
                    if (OpenVR.System != null) E = OpenVR.System.GetTrackedDeviceActivityLevel((uint)i);

                    VRControllerState_t state1 = new VRControllerState_t();
                    TrackedDevicePose_t pose1 = new TrackedDevicePose_t();

                    var D = false;

                    if (OpenVR.System != null)
                        D = OpenVR.System.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, (uint)i, ref state1, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)), ref pose1);



                    if (C.Contains("hmd"))
                    {
                        HMD_Battery = B;

                        HMD_Ready = D;

                        string Activity = "";

                        if (E == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction)
                        {
                            // Activity = "HasActive";
                            HMD_HasActivity = true;

                        }
                        else
                        {
                            Activity = "NoActive";
                            HMD_HasActivity = false;
                        }
                        Info += "<color=white>----- VSVR -----</color>\n";

                        if (B > 0.25f)
                            Info += "Casque = Battery: <color=green>" + Math.Round(B * 100) + "%</color> Active: " + D + " " + Activity + "\n";
                        else
                            Info += "Casque = Battery: <color=red>" + Math.Round(B * 100) + "%</color> Active: " + D + " " + Activity + "\n";
                    }
                    else
                      if (i == 3)
                    {
                        Info += "Hand Left : " + D + " " + "\n";
                    }
                    else
                      if (i == 4)
                    {
                        Info += "Hand Right : " + D + " " + "\n";
                    }
                    else
                      if (A.Contains("Vive Tracker") && D)
                    {
#if VBO_Input
                       
                        Info += "GUN TRACKER : " + D+ " Tracking : " + VaroniaInput.Instance.HasWeaponTracking + "\n";
#endif
                    }
                    else
                    if (C.Contains("Invalid") || i == 1 || i == 2)
                    {

                    }
                    else if (!A.Contains("unknown") && !A.Contains("Vive Tracker"))
                    {

                        Info += A + " - " + B + " - " + C + " - " + D + " " + "\n";


                    }
                }
                catch (System.Exception)
                {
                }




            }

#if VBO_Input

            try
            {

            

                if (lastXpos != VaroniaInput.Instance.Tracking.localPosition.x && !VaroniaInput.Instance.HasWeaponTracking)
                    VaroniaInput.Instance.OnWeaponHasTracking.Invoke();
            else if (lastXpos == VaroniaInput.Instance.Tracking.localPosition.x && VaroniaInput.Instance.HasWeaponTracking)
                    VaroniaInput.Instance.OnWeaponLostTracking.Invoke();


            lastXpos = VaroniaInput.Instance.Tracking.localPosition.x;

            }
            catch (Exception)
            {

            }

#endif


            yield return new WaitForSeconds(0.1f);

        }
    }


    private void LateUpdate()
    {
        DebugVaronia.Instance.TextDebugInfo.text += Info;
    }


    private void Update()
    {
        if (!timeoutEventFired && (DateTime.UtcNow - lastMessageTime).TotalSeconds > 0.1 && Time.time >5)
        {
            timeoutEventFired = true;

            OnWebsocketTimeout();
        }


    }



    static void OnWebsocketTimeout()
    {

        Debug.Log(@" /!\ Lost Streaming connection /!\ ");

        StatisticsSummaryItem temp = new StatisticsSummaryItem();

       temp.total_latency_ms = -1;
        temp.network_latency_ms = -1;
        temp.encode_latency_ms = -1;
        temp.decode_latency_ms = -1;

        LiveStatistics = temp;
        // AvgStatistics.Add(LiveStatistics);
    }


    void LiveStats()
    {
        string Log_Network_Latency = ReturnTextColor(DebugVSVR.LiveStatistics.network_latency_ms, 7, 13);
        string Log_Encode_Latency = ReturnTextColor(DebugVSVR.LiveStatistics.encode_latency_ms, 8, 13);
        string Log_Decode_Latency = ReturnTextColor(DebugVSVR.LiveStatistics.decode_latency_ms, 10, 14);
        string Log_Total_Latency = ReturnTextColor(DebugVSVR.LiveStatistics.total_latency_ms, 85, 95);


        if (DebugVSVR.LiveStatistics.network_latency_ms > 60 || DebugVSVR.LiveStatistics.network_latency_ms == -1)
            BigLagCount++;

        DebugVaronia.Instance.Latency.text = $"T.Lat : {Log_Total_Latency} ms \nNet.Lat : {Log_Network_Latency} ms  \nEnc.Lat :  {Log_Encode_Latency} ms  \nDec.Lat : {Log_Decode_Latency} ms";
        liveStats.text = $"B.Lag Count :  <color=red>{BigLagCount.ToString()}</color>        T.Lat : {Log_Total_Latency} ms Net.Lat : {Log_Network_Latency} ms  Enc.Lat :  {Log_Encode_Latency} ms  Dec.Lat : {Log_Decode_Latency} ms";


        realTimeLatencyChart.AddLatencyValue((float)DebugVSVR.LiveStatistics.network_latency_ms);

    }



    void AVGStats()
    {
        try
        {
            var AVG = DebugVSVR.CalculerMoyenne(DebugVSVR.AvgStatistics);
            string Log_Network_Latency = ReturnTextColor(AVG.network_latency_ms, 8, 13);
            string Log_Encode_Latency = ReturnTextColor(AVG.encode_latency_ms, 8, 13);
            string Log_Decode_Latency = ReturnTextColor(AVG.decode_latency_ms, 10, 14);
            string Log_Total_Latency = ReturnTextColor(AVG.total_latency_ms, 85, 95);

            avgStats.text = $"AVG. T.Lat : {Log_Total_Latency} ms AVG. Net.Lat : {Log_Network_Latency} ms  AVG. Enc.Lat :  {Log_Encode_Latency} ms  AVG. Dec.Lat : {Log_Decode_Latency} ms";

        }
        catch (Exception)
        {


        }

    }


    string ReturnTextColor(double value, int good, int bad)
    {
        string returnvalue = "";

        switch (value)
        {
            case double a when a < 0:
                returnvalue = "<color=grey>" + value.ToString("N1") + "</color>";
                break;
            case double a when a <= good:
                returnvalue = "<color=green>" + value.ToString("N1") + "</color>";
                break;
            case double a when a > (good) && a < bad:
                returnvalue = "<color=orange>" + value.ToString("N1") + "</color>";
                break;
            case double a when a > bad:
                returnvalue = "<color=red>" + value.ToString("N1") + "</color>";
                break;
            default:
                returnvalue = "<color=grey>" + value.ToString("N1") + "</color>";
                break;
        }

        return returnvalue;
    }

}
