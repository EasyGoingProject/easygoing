#define BUFSIZE 1024

#include <winsock2.h>

//�������� ����ü
typedef struct
{
	SOCKET hClntSock;
	SOCKADDR_IN clntAddr;
} CLIENT_DATA, *LP_CLIENT_DATA;


//���Ϲ������� ����ü
typedef struct
{
	OVERLAPPED overlapped;
	char buffer[BUFSIZE];
	WSABUF wsaBuf;
} IO_DATA, *LP_IO_DATA;
