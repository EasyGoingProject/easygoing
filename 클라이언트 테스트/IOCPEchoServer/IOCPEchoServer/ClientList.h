#define MAXSIZE 4
#define TRUE 1
#define FALSE 0
//#define BOOL int

#include <winsock2.h>

//�������� ����ü
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
	//EchoServer���� ������ ���� ������ ����
	CLIENT_DATA connectionList[MAXSIZE];
	//Ŭ���̾�Ʈ ����Ʈ�� �� ũ��
	int size;
	//Ŭ���̾�Ʈ ����Ʈ�� �Է�, ������ ������ ��ġ
	int cursor;
	int start;
	int roomNumber;
} CLIENT_LIST, *LP_CLIENT_LIST;


typedef struct {
	CLIENT_LIST room[MAXSIZE];
	//����Ʈ�� �� ũ��
	int size;
	//����Ʈ�� �Է�, ������ ������ ��ġ
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
