using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Onthesys.WebBuild
{
    public static class Option
    {
        public static int TREND_TIME_INTERVAL = 1;
        public static int TREND_TIME_RANGE = 1440;

        public static string URL = "http://192.168.10.236:8080";
    }

    public static class  DateTimeKst
    {
        public static DateTime Now => DateTime.UtcNow.AddHours(9);

        public static DateTime Parse(string dateTime)
        {
            DateTime dt = DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified); // 명시적으로 Unspecified
            //dt = dt.AddHours(9); // KST 기준으로 보정
            return dt;
        }

        public static DateTime ParseRaw(string dateTime)
        {
            // 문자열을 "숫자 그대로" DateTime으로 해석, 타임존 무시
            DateTime dt = DateTime.ParseExact(
                dateTime.Replace("Z", ""),                     // Z 제거
                "yyyy-MM-ddTHH:mm:ss.fff",                     // 정확한 포맷 지정
                CultureInfo.InvariantCulture,
                DateTimeStyles.None                            // UTC 변환 안 함
            );

            // 무조건 Unspecified → 로컬 타임존 간섭 방지
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        }

    }
}
