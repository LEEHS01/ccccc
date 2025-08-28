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

int CheckCMTI(void)
{
    char buf[BUF_SIZE];
    int len = 0;

    while (DataSizeInCom1() > 0 && len < BUF_SIZE - 1)
    {
        buf[len++] = ReadCom1();
    }
    buf[len] = '\0';

    if (len > 0 && strstr(buf, "CMTI") != 0)
        return 1;
    return 0;
}

void SendSensorDataPacket(void)
{
    unsigned char packet[42];
    int i, offset;
    float val;
    union {
        float f;
        unsigned char b[4];
    } u;

    offset = 0;

    for (i = 0; i < 8; i++)
    {
        // 1 byte: T/F
        packet[offset++] = (unsigned char)sensor_timeout[i];

        // 4 bytes: IEEE754 little-endian float
        val = sensor_values[i];
        u.f = val;

        packet[offset++] = u.b[0];
        packet[offset++] = u.b[1];
        packet[offset++] = u.b[2];
        packet[offset++] = u.b[3];
    }

    packet[offset++] = 0x0D; // CR

    ToCom1Bufn((char *)packet, 42);
}


void main(void)
{
    InitSystem();

    while (1)
    {
        if (CheckCMTI())
        {
            SendSensorDataPacket();
        }

        Delay(200); // 버퍼 확인 주기
    }
}
