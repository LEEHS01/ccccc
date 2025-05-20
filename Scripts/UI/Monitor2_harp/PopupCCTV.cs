//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Onthesys;
//using System.Collections.Generic;
//using System.Linq;
//using DG.Tweening;
//using System;
//using System.Net;
//using UnityEngine.Networking;
//using System.Text;
//using System.Net.Http;
//using System.Threading.Tasks;
//using UMP;
//using Unity.VisualScripting;

//internal class PopupCCTV : MonoBehaviour
//{
//    internal TMP_Text txtName;
//    internal UniversalMediaPlayer video;
//    internal GameObject loading;
//    internal ARVideoCanvasHelper gui;
//    private ObsData data;

//    internal GameObject btnPlay = null;
//    internal GameObject btnPause = null;
//    private bool isEventRegistered = false;

//    private void OnEnable()
//    {
//        // 팝업 열릴 때 버튼 상태 초기화
//        if(btnPlay) btnPlay?.SetActive(true);
//        if(btnPause) btnPause?.SetActive(false);
//    }

//    void Start()
//    {
//        this.gui.gameObject.SetActive(false);
//        this.loading.SetActive(false);

//        // 버퍼링 시 로딩 표시
//        this.video.AddBufferingEvent((progress) =>
//        {
//            bool isBuffering = progress < 100;
//            this.loading.SetActive(isBuffering);
//            this.gui.gameObject.SetActive(isBuffering);
//        });
//    }

//    internal void OnSelectCCTV(ObsData area)
//    {
//        this.data = area;
//        this.txtName.text = area.areaName + " - " + area.obsName;
//        this.gui.gameObject.SetActive(true);
//    }


//    internal void OnVideoUp() {
//        Debug.Log("up");
//        SendPTZCommandAsync("up", 2, 1000);
//    }

//    internal void OnVideoDown() {
//        Debug.Log("down");
//        SendPTZCommandAsync("down", 2, 1000);
//    }

//    internal void OnVideoLeft() {
//        Debug.Log("left");
//        SendPTZCommandAsync("left", 2, 1000);
//    }

//    internal void OnVideoRight() {
//        Debug.Log("right");
//        SendPTZCommandAsync("right", 2, 1000);
//    }

//    internal void OnVideoIn() {
//        Debug.Log("in");
//        SendPTZCommandAsync("zoomin", 6, 1000);
//    }

//    internal void OnVideoOut() {
//        Debug.Log("out");
//        SendPTZCommandAsync("zoomout", 6, 1000);
//    }

//    internal void OnVideoEvent() {
//        //Debug.Log("MediaPlayer " + mp.name + " generated event: " + eventType.ToString());
        
//    }

//    private async Task SendPTZCommandAsync(string direction, int speed, int timeout)
//    {
//        //string requestUrl = $"http://115.91.85.42:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
//        string requestUrl = $"http://192.168.1.109:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
//        //url2 = $"http://192.168.1.108:50080/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
//        HttpClient client = new HttpClient();
        
//            // 사용자 이름과 비밀번호 설정
//            var byteArray = Encoding.ASCII.GetBytes("admin:HNS_qhdks_!Q@W3");
//            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

//            try
//            {
//                // GET 요청
//                HttpResponseMessage response = await client.GetAsync(requestUrl);

//                // 예외 발생
//                response.EnsureSuccessStatusCode();

//                // 응답 본문 읽기
//                string responseBody = await response.Content.ReadAsStringAsync();
//                Debug.Log(responseBody);

//                // 요청 성공 메시지
//                //MessageBox.Show($"Command {direction} sent successfully.");
//            }
//            catch (HttpRequestException httpEx)
//            {
//                Debug.Log($"HttpRequestException: {httpEx.Message}");
//                Console.WriteLine($"HttpRequestException: {httpEx.Message}");
//            }
//            catch (Exception ex)
//            {
//                Debug.Log($"Error sending PTZ command: {ex.Message}");
//                Console.WriteLine($"Exception: {ex.Message}");
//            }
        
//        /*
//        string requestUrl = $"http://115.91.85.42:50081/httpapi/SendPTZ?action=sendptz&PTZ_CHANNEL=1&PTZ_MOVE={direction},{speed}&PTZ_TIMEOUT={timeout}";
//        UnityWebRequest www = UnityWebRequest.Get(requestUrl);
//        www.SetRequestHeader("Authorization", "admin:HNS_qhdks_!Q@W3");
//        www.SendWebRequest();

//        var byteArray = Encoding.ASCII.GetBytes("admin:HNS_qhdks_!Q@W3");
//                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

//        */
//    }


//}
