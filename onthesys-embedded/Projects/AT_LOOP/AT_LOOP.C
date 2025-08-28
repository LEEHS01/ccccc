#include "..\LIB\7186e.h"
#include <string.h>

#define BUF_SIZE 256

void InitSystem(void)
{
    InitLib();  
    InstallCom1(115200, 8, 0, 1);  
    SetCom1Timeout(50);            
}

void SendATCommand(void)
{
    ToCom1Str("AT\r"); 
}

void ReadATResponse(void)
{
    char buffer[BUF_SIZE];
    int len = 0;

    Delay(100);  

    while (DataSizeInCom1() > 0 && len < BUF_SIZE - 1)
    {
        buffer[len++] = ReadCom1();
    }

    if (len > 0)
    {
        buffer[len] = '\0';  
        Print("Response: %s\n", buffer);
    }
}

void main(void)
{
    InitSystem();

    while (1)
    {
        SendATCommand();
        ReadATResponse();
        Delay(1000); 
    }
}
