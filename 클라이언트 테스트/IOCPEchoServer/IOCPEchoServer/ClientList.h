#define MAXSIZE 4
#define TRUE 1
#define FALSE 0
//#define BOOL int

#include <winsock2.h>

//소켓정보 구조체
typedef struct
{
	SOCKET hClntSock;
	SOCKADDR_IN clntAddr;
	DWORD clientNum;
	char characterNum[5];
	char clientName[20];
	int ready;
} CLIENT_DATA, *LP_CLIENT_DATA;


typedef struct {
	//EchoServer에서 선언한 소켓 데이터 구조
	CLIENT_DATA connectionList[MAXSIZE];
	//클라이언트 리스트의 총 크기
	int size;
	//클라이언트 리스트에 입력, 삭제를 실행할 위치
	int cursor;
	int start;
	int roomNumber;
} CLIENT_LIST, *LP_CLIENT_LIST;


typedef struct {
	CLIENT_LIST room[MAXSIZE];
	//리스트의 총 크기
	int size;
	//리스트에 입력, 삭제를 실행할 위치
	int cursor;
} ROOM_LIST, *LP_ROOM_LIST;



void ClientListInit(CLIENT_LIST *list, int roomNumber);
BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST *list);
void SetClientReady(CLIENT_LIST * list, int clientNumber, int ready);
void SetClient(CLIENT_LIST * list, int clientNumber, char *clientName, char *characterNum, int ready);
BOOL RemoveAtClientList(int connection, CLIENT_LIST *list);
void ClearClientList(CLIENT_LIST *list);

BOOL IsClientFull(CLIENT_LIST *list);
BOOL IsClientEmpty(CLIENT_LIST *list);


void RoomListInit(ROOM_LIST *list);
BOOL AddToRoomList(CLIENT_LIST roomList, ROOM_LIST *list);
BOOL RemoveAtRooomList(int roomNumber, ROOM_LIST *list);
void ClearRoomList(ROOM_LIST *list);

BOOL IsRoomFull(ROOM_LIST *list);
BOOL IsRoomEmpty(ROOM_LIST *list);
