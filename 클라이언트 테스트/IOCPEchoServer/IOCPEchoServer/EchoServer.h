#define BUFSIZE 1024
#define PKTMSGSIZE 100
#define PKTLENGTH 4

#include <winsock2.h>

//소켓정보 구조체
typedef struct
{
	SOCKET hClntSock;
	SOCKADDR_IN clntAddr;
	DWORD clientNum;
	char characterNum[3];
	char clientName[20];
} CLIENT_DATA, *LP_CLIENT_DATA;


//소켓 데이터 전송정보 구조체
typedef struct
{
	OVERLAPPED overlapped;
	char buffer[BUFSIZE];
	WSABUF wsaBuf;
} IO_DATA, *LP_IO_DATA;


//서버 메시지 구조체
typedef struct
{
	char messageLength[PKTLENGTH];
	char message[PKTMSGSIZE];
} PacketData, *LPPacketData;
