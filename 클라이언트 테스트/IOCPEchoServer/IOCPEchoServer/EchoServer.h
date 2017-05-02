#define BUFSIZE 1024
#define PKTMSGSIZE 100
#define PKTLENGTH 4

#include <winsock2.h>

//�������� ����ü
typedef struct
{
	SOCKET hClntSock;
	SOCKADDR_IN clntAddr;
	DWORD clientNum;
	char characterNum[3];
	char clientName[20];
} CLIENT_DATA, *LP_CLIENT_DATA;


//���� ������ �������� ����ü
typedef struct
{
	OVERLAPPED overlapped;
	char buffer[BUFSIZE];
	WSABUF wsaBuf;
} IO_DATA, *LP_IO_DATA;


//���� �޽��� ����ü
typedef struct
{
	char messageLength[PKTLENGTH];
	char message[PKTMSGSIZE];
} PacketData, *LPPacketData;
