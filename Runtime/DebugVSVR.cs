using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class DebugVSVR
{


    public class StatisticsSummaryItem
    {
        public double video_packets_total { get; set; }
        public double video_packets_per_sec { get; set; }
        public double video_mbytes_total { get; set; }
        public double video_mbits_per_sec { get; set; }
        public double total_latency_ms { get; set; }
        public double network_latency_ms { get; set; }
        public double encode_latency_ms { get; set; }
        public double decode_latency_ms { get; set; }
        public double packets_lost_total { get; set; }
        public double packets_lost_per_sec { get; set; }
        public double client_fps { get; set; }
        public double server_fps { get; set; }
        public double battery_hmd { get; set; }
        public bool hmd_plugged { get; set; }
    }


    

    public static bool NeedStop, IsRunning;

    public static StatisticsSummaryItem LiveStatistics;
    public static List<StatisticsSummaryItem> AvgStatistics;


    public static DateTime lastMessageTime = DateTime.UtcNow;
    public static bool timeoutEventFired = false;

    public static void Init()
    {
        NeedStop = false;
        IsRunning = true;


        AvgStatistics = new List<StatisticsSummaryItem>();

        Task.Run(async () =>
        {
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                Uri serverUri = new Uri("ws://localhost:8082/api/events");
                webSocket.Options.SetRequestHeader("X-ALVR", "true");
                await webSocket.ConnectAsync(serverUri, CancellationToken.None);

                byte[] receiveBuffer = new byte[1024];
                var bufferString = "";


                while (webSocket.State == WebSocketState.Open && !NeedStop)
                {
                    try
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            lastMessageTime = DateTime.UtcNow;
                            timeoutEventFired = false; 

                            bufferString += Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                            var tSplitted = SplitBuffer(bufferString);
                            if (tSplitted.Count > 1)
                            {
                                var tData = JObject.Parse(tSplitted[0]);

                                if (tData["event_type"]["id"].ToString() == "GraphStatistics")
                                {
                                    //var tGraphData = JsonConvert.DeserializeObject<VSVRLineItem.GraphStatisticItem>(tData["event_type"]["data"].ToString());
                                }
                                else if (tData["event_type"]["id"].ToString() == "Tracking")
                                {
                                }
                                else if (tData["event_type"]["id"].ToString() == "StatisticsSummary")
                                {
                                 

                                    LiveStatistics = JsonConvert.DeserializeObject<StatisticsSummaryItem>(tData["event_type"]["data"].ToString());
                                    AvgStatistics.Add(LiveStatistics);

                                    if (LiveStatistics.network_latency_ms >13)
                                        Debug.Log(@" /!\ Network Latency Alert '"+ LiveStatistics.network_latency_ms + @"' /!\ ");


                                }

                                bufferString = tSplitted[1];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                 

                }
                IsRunning = false;
            }
        });
    }




    public static List<string> SplitBuffer(string Input)
    {
        return Input.Split(new string[] { "{\"timestamp\"" }, StringSplitOptions.None).Where(w => !string.IsNullOrEmpty(w)).Select(s => "{\"timestamp\"" + s).ToList();
    }



    public static StatisticsSummaryItem CalculerMoyenne(List<StatisticsSummaryItem> valeurs)
    {

        StatisticsSummaryItem value = new StatisticsSummaryItem();

        if (valeurs.Count >= 600)
            valeurs.RemoveAt(0);

        if (valeurs != null && valeurs.Count > 0)
        {
            value.total_latency_ms = valeurs.Average( l => l.total_latency_ms);
            value.client_fps = valeurs.Average(l => l.client_fps);
            value.decode_latency_ms = valeurs.Average(l => l.decode_latency_ms);
            value.encode_latency_ms = valeurs.Average(l => l.encode_latency_ms);
            value.network_latency_ms = valeurs.Average(l => l.network_latency_ms);
       
        }
        return value;
    }


}
