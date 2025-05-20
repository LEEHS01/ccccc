internal interface ISMSHandle
{
    /// <summary>
    /// sms 문자 발송 인터페이스 
    /// </summary>
    /// <param name="phoneNumber">  - 가 제거된 휴대폰 번호 </param>
    /// <param name="smsMessage">  최대 길이 130 Byte 문자열 </param>
    /// <returns>
    //                 1 : 성공
    //                -1 : 실패 
    //                     - 2 : ****
    //                     - 3 : ****
    //                     - 4 : ****
    //                     - 5 : ****
    /// </returns>

    int Send_smsMessage(string phoneNumber, string smsMessage);

}

//6월 초에 Body ? 올것이니 참고