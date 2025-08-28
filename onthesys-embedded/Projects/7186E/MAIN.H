// #ifndef SENSOR_APP_H
// #define SENSOR_APP_H

// // --- Definitions ---
// #define WM300V_COM COM1
// #define BAUD_RATE 115200
// #define PDU_UD_SENSOR "3E30310D"
// #define PDU_UD_OK     "3E30320D"
// #define RESPONSE_OK   "3E79750D"
// #define ERR_SHOW_INVALID_PDU 101
// #define ERR_SHOW_SEND_FAIL    102
// #define ERR_SHOW_MODEM_FAIL   103
// #define ERR_SHOW_INTERNAL     199

// // --- Global Variables ---
// extern float sensor_values[8];
// extern char sensor_timeout[8];

// // --- Function Prototypes ---
// void ShowError(int code);
// void InitSystem(void);
// void CheckModem(void);
// int ReadPDU(char* outBuf, int maxLen);
// void SendPDU(const char* hexStr, int hexLen);
// void SendSensorPDU(void);
// void SendOKPDUAndReboot(void);
// void Loop(void);

// #endif
