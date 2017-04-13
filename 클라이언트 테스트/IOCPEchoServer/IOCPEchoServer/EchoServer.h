#define BUFSIZE 1024

#include <winsock2.h>

//소켓정보 구조체
typedef struct
{
	SOCKET hClntSock;
	SOCKADDR_IN clntAddr;
} CLIENT_DATA, *LP_CLIENT_DATA;


//소켓버퍼정보 구조체
typedef struct
{
	OVERLAPPED overlapped;
	char buffer[BUFSIZE];
	WSABUF wsaBuf;
} IO_DATA, *LP_IO_DATA;
