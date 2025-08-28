#include "..\..\LIB\7186e.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include "MAIN.H"

#define WM300V_COM COM1
#define BAUD_RATE 115200
#define PDU_UD_SENSOR "3E30310D"
#define PDU_UD_OK     "3E30320D"
#define RESPONSE_OK   "3E79750D"
#define ERR_SHOW_INVALID_PDU 101
#define ERR_SHOW_SEND_FAIL    102
#define ERR_SHOW_MODEM_FAIL   103
#define ERR_SHOW_INTERNAL     199

float sensor_values[8] = { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8 };
char sensor_timeout[8] = { 'T','T','T','T','T','T','T','F' };

void EncodePhoneNumberToBCD(const char* phone, char* bcd, int* bcdLen) {
    int len = strlen(phone);
    int i, j = 0;

    for (i = 0; i < len; i += 2) {
        char high = phone[i];
        char low = (i + 1 < len) ? phone[i + 1] : 'F';  // padding with 'F'
        bcd[j++] = ((low >= '0' && low <= '9' ? low - '0' : 0xF) << 4)
                 | (high - '0');
    }
    *bcdLen = j;
}

int BuildResponsePDU(const char* destPhone, const char* udHex, char* outHexPDU) {
    char pdu[512];
    int idx = 0;
    int i;
    int phoneLen = strlen(destPhone);
    // Phone number (BCD)
    char bcd[16];
    int bcdLen;
    int udLen = strlen(udHex) / 2;
    char byteStr[3];

    // SMSC info length = 00 (use default)
    pdu[idx++] = 0x00;

    // PDU-Type = SMS-SUBMIT
    pdu[idx++] = 0x11;

    // Message reference
    pdu[idx++] = 0x00;

    // Phone number length
    pdu[idx++] = phoneLen;

    // TON/NPI
    pdu[idx++] = 0xA1;

    EncodePhoneNumberToBCD(destPhone, bcd, &bcdLen);
    for (i = 0; i < bcdLen; i++) {
        pdu[idx++] = bcd[i];
    }

    // PID
    pdu[idx++] = 0x00;

    // DCS
    pdu[idx++] = 0x04;

    // Validity Period (optional): 00
    pdu[idx++] = 0x00;

    // User Data Length (bytes)
    pdu[idx++] = udLen;

    // UD 삽입 (hex string → 바이트로 변환)
    for (i = 0; i < udLen; i++) {
        byteStr[0] = udHex[i * 2];
        byteStr[1] = udHex[i * 2 + 1];
        byteStr[2] = '\0';
        pdu[idx++] = (char)strtol(byteStr, NULL, 16);
    }

    // 최종 PDU 길이
    for (i = 0; i < idx; i++) {
        sprintf(&outHexPDU[i * 2], "%02X", (unsigned char)pdu[i]);
    }

    return idx - 1;  // CMGS에 넣을 바이트 수 (SMSC 제외)
}


void ShowError(int code) {
    Init5DigitLed();
    Show5DigitLed(0, code / 10000 % 10);
    Show5DigitLed(1, code / 1000 % 10);
    Show5DigitLed(2, code / 100 % 10);
    Show5DigitLed(3, code / 10 % 10);
    Show5DigitLed(4, code % 10);
    LedOn();
}

void InitSystem() {
    InitLib();
    InstallCom1(BAUD_RATE, 8, 0, 1);
    DelayMs(500);
    ClearCom1();
    ClearTxBuffer1();
    Disable5DigitLed();
    LedOff();
}

void CheckModem() {
    char buf[128];

    printCom1("AT\r");
    DelayMs(500);

    if (DataSizeInCom1() > 0) {
        memset(buf, 0, sizeof(buf));
        ReadCom1n(buf, sizeof(buf) - 1);
        Print("Modem Response: %s\n", buf);
    }
    else {
        ShowError(ERR_SHOW_MODEM_FAIL);
        Print("No response from WM-300V\n");
    }
}

int ReadPDU(char* outBuf, int maxLen) {
    int len;
    if (DataSizeInCom1() == 0) return 0;
    len = ReadCom1n(outBuf, maxLen);
    if (len <= 0 || len >= maxLen) {
        ShowError(ERR_SHOW_INVALID_PDU);
        return 0;
    }
    outBuf[len] = '\0';
    return len;
}

void SendPDU(const char* hexStr, int hexLen) {
    int pduLen;

    printCom1("AT+CSCS=\"IRA\"\r");
    DelayMs(500);
    printCom1("AT+CMGF=0\r");
    DelayMs(500);
    printCom1("AT+CSMP=17,167,0,240\r");
    DelayMs(500);

    pduLen = hexLen / 2 - 1;
    if (pduLen <= 0) {
        ShowError(ERR_SHOW_SEND_FAIL);
        return;
    }

    printCom1("AT+CMGS=%d\r", pduLen);
    DelayMs(1000);
    if (ToCom1Bufn((char*)hexStr, hexLen) != hexLen) {
        ShowError(ERR_SHOW_SEND_FAIL);
        return;
    }
    ToCom1(0x1A);
    DelayMs(1000);
    Print("Sent PDU\n");
}

void SendSensorPDU(const char* serverPhone) {
    unsigned char ud[42];
    char udHex[85];
    char pduHex[512];
    float val;
    int idx = 0;
    int i;
    int pduLen = 0;

    ud[idx++] = 'T';

    for (i = 0; i < 8; i++) {
        unsigned char* bytes = (unsigned char*)&sensor_values[i];
        ud[idx++] = bytes[0];
        ud[idx++] = bytes[1];
        ud[idx++] = bytes[2];
        ud[idx++] = bytes[3];
        ud[idx++] = sensor_timeout[i];
    }

    ud[idx++] = 0x0D;

    for (i = 0; i < idx; i++) {
        sprintf(&udHex[i * 2], "%02X", ud[i]);
    }

    // 전체 PDU 생성
    pduLen = BuildResponsePDU(serverPhone, udHex, pduHex);

    // 전송
    SendPDU(pduHex, pduLen * 2);
}


void SendOKPDUAndReboot() {
    SendPDU(RESPONSE_OK, strlen(RESPONSE_OK));
    DelayMs(1000);

    EnableWDT();
    while (1);
}

void Loop() {
    char rxBuf[512];
    int len;
    char ud[9];

    while (1) {
        DelayMs(1000);

        if (ReadPDU(rxBuf, sizeof(rxBuf) - 1)) {
            Print("Received PDU: %s\n", rxBuf);

            len = strlen(rxBuf);
            if (len < 8) {
                ShowError(ERR_SHOW_INVALID_PDU);
                continue;
            }

            memset(ud, 0, sizeof(ud));
            strncpy(ud, &rxBuf[len - 8], 8);

            if (strcmp(ud, PDU_UD_SENSOR) == 0) {
                SendSensorPDU("01220414524");
            }
            else if (strcmp(ud, PDU_UD_OK) == 0) {
                SendOKPDUAndReboot();
            }
            else {
                ShowError(ERR_SHOW_INVALID_PDU);
                Print("Unknown UD: %s\n", ud);
            }
        }
    }
}



void main(int argc, char *argv[]){
    InitSystem();
    CheckModem();
    Loop();
}
