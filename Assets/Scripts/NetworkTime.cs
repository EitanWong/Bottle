using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

namespace NTP
{
    public class NetworkTime {

        private static NetworkTime instance;

        private static readonly object locker = new object();

        private NetworkTime(){
        }

        public static NetworkTime GetInstance(){
            if (instance == null) {
                lock (locker) {
                    if (instance == null) {
                        instance = new NetworkTime ();
                    }
                }
            }

            return instance;
        }

        public void InitTime(){
            GetNetworkTime ();
        }
        public string TIME_SERVER_URL = "ntp7.aliyun.com";
        public DateTime initTime,initLocalTime;
        public static DateTime InsteadLocalTime;
        public static string InsteadLocalTime_st;
        public DateTime GetNetworkTime()
        {

            try
            {
                //ntp服务器地址
                string server = TIME_SERVER_URL;

                var ntpData = new byte[48];
                ntpData[0] = 0x1B;
                //网络链接
                var addresses = Dns.GetHostEntry(server).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123); // https port : 443
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.ReceiveTimeout = 5000;
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();

                ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
                ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

                var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                var networkDataTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local)).AddMilliseconds((long)milliseconds);

                TimeZone localzone = TimeZone.CurrentTimeZone;
                TimeSpan currentOffset = localzone.GetUtcOffset(networkDataTime);
                initTime = networkDataTime + currentOffset;
                initLocalTime = DateTime.Now;
                return initTime;

            }
            catch
            {

                InsteadLocalTime = DateTime.UtcNow;

                TimeZone localzone_ = TimeZone.CurrentTimeZone;
                TimeSpan currentOffset_ = localzone_.GetUtcOffset(DateTime.UtcNow);
                initTime = DateTime.UtcNow + currentOffset_;//기준 시에 +,- 로컬과 시간차이 더하면 현지로컬시간이 나옴!
                return initTime;

            }

        }

        public DateTime GetDay(int day = 0){

            return GetCurrentTime().AddDays(day).Date;
        }

        public DateTime GetCurrentTime(){
            DateTime d = DateTime.Now;
            return initTime + (d - initLocalTime);
        }

        public void CompareGameTime(){
            //时间的比较，一般用于连续签到系统等
            string SavedAfterDayTime = PlayerPrefs.GetString("SavedAfterDayTime", "11/30/2018 00:00:00 AM");
            //转化成第一个时间点（即领取后的第一天）
            DateTime SavedAfterDayTime_T = Convert.ToDateTime(SavedAfterDayTime);
            //当前时间与时间点相比较，当前时间超过存档钱则等于1，相等则等于0，当前时间未超过存档点的时间则等于-1
            int compare1 = DateTime.Compare(GetDay(), SavedAfterDayTime_T.Date);

            if(compare1 < 0){
                //开启倒计时
            }else if(compare1 == 0){
                //待领取，计时器结束
            }else{
                //重置状态

            }

        }
            
    }
}