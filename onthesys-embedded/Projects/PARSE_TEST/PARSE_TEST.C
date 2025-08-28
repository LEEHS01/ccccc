#include "..\LIB\7186e.h"
#include <string.h>

#define BUF_SIZE 128

float sensor_values[8] = { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8 };
char sensor_timeout[8] = { 'T','T','T','T','T','T','T','F' };


void InitSystem(void)
{
    InitLib();  
    InstallCom1(115200, 8, 0, 1);  
    SetCom1Timeout(50);            
}

int ExtractMsgIndex(char *buf)
{
    char *start, *comma;
    start = strstr(buf, "+CMTI:");
    if (!start) return -1;

    comma = strchr(start, ',');
    if (!comma) return -1;

    return atoi(comma + 1); 
}

int ExtractPDU(char *buf, char *outPDU)
{
    int len = 0;
    char *start, *end;
    char cmd[128];

    start = strstr(buf, "\r\n0791");
    if (!start) {
        sprintf(cmd, "ExtractPDU Error: PDU start not found.\n");
        ToCom1Str(cmd);
        return 0;
    }

    start += 2; // skip \r\n

    end = strstr(start, "\r\n");
    if (!end) {
        sprintf(cmd, "ExtractPDU Error: PDU end marker not found.\n");
        ToCom1Str(cmd);
        return 0;
    }

    len = end - start;
    if (len >= 255) {
        sprintf(cmd, "ExtractPDU Error: PDU too long (len=%d)\n", len);
        ToCom1Str(cmd);
        return 0;
    }

    strncpy(outPDU, start, len);
    outPDU[len] = '\0';

    sprintf(cmd, "ExtractPDU Succeed: %s\n", outPDU);
    ToCom1Str(cmd);

    return 1;
}

int ExtractUD(const char *pdu, char *outUD)
{
    int smscLen, smscEnd;
    int senderLen, senderBytes;
    int offset, udl, udhLen, udhTotalLen;
    const char *udStart;
    int pduLen = strlen(pdu);
    char cmd[128];

    smscLen = (int)strtol(pdu, 0, 16);
    smscEnd = 2 + smscLen * 2;
    if (smscEnd + 2 >= pduLen) {
        sprintf(cmd, "ExtractUD Error: Invalid SMSC length or buffer overrun.\n");
        ToCom1Str(cmd);
        return 0;
    }

    senderLen = (int)strtol(pdu + smscEnd + 2, 0, 16);
    senderBytes = (senderLen + 1) / 2;

    offset = smscEnd + 2 + 2 + senderBytes * 2 + 2 + 2 + 14;
    if (offset + 2 >= pduLen) {
        sprintf(cmd, "ExtractUD Error: offset overrun (offset=%d, pduLen=%d)\n", offset, pduLen);
        ToCom1Str(cmd);
        return 0;
    }

    udl = (int)strtol(pdu + offset, 0, 16);
    udStart = pdu + offset + 2;

    if ((udStart - pdu) + 2 > pduLen) {
        sprintf(cmd, "ExtractUD Error: UD start beyond PDU length.\n");
        ToCom1Str(cmd);
        return 0;
    }

    udhLen = (int)strtol(udStart, 0, 16);
    udhTotalLen = (udhLen + 1) * 2;

    if ((udStart - pdu) + udhTotalLen >= pduLen) {
        sprintf(cmd, "ExtractUD Error: UDH overflow or malformed.\n");
        ToCom1Str(cmd);
        return 0;
    }

    if ((udl * 2 - udhTotalLen) >= 255) {
        sprintf(cmd, "ExtractUD Error: UD payload too large.\n");
        ToCom1Str(cmd);
        return 0;
    }

    strncpy(outUD, udStart + udhTotalLen, udl * 2 - udhTotalLen);
    outUD[udl * 2 - udhTotalLen] = '\0';

    sprintf(cmd, "ExtractUD Succeed: %s\n", outUD);
    ToCom1Str(cmd);

    return 1;
}



void ProcessCMTI(void)
{
    char buf[256];
    char cmd[32];
    int len = 0;
    int msgIdx = 0;
    char pdu[256];
    char ud[256];

    while (DataSizeInCom1() > 0 && len < 255)
        buf[len++] = ReadCom1();
    buf[len] = '\0';

    msgIdx = ExtractMsgIndex(buf);
    if (msgIdx < 0)
        return;

    sprintf(cmd, "AT+CMGR=%d\r", msgIdx);
    ToCom1Str(cmd);

    Delay(1000); 

    len = 0;
    while (DataSizeInCom1() > 0 && len < 255)
        buf[len++] = ReadCom1();
    buf[len] = '\0';

    sprintf(cmd, "ExtractPDU Started!\n");
    ToCom1Str(cmd);

    if (!ExtractPDU(buf, pdu))
        return;

    sprintf(cmd, "ExtractPDU Succeed!\n");
    ToCom1Str(cmd);

    if (!ExtractUD(pdu, ud))
        return;

    sprintf(cmd, "ExtractUD Succeed!\n");
    ToCom1Str(cmd);

    sprintf(cmd, "UD: %s\n", ud);
    ToCom1Str(cmd);
}


void main(void)
{
    InitSystem();

    while (1)
    {
        ProcessCMTI();

        Delay(200);
    }
}
