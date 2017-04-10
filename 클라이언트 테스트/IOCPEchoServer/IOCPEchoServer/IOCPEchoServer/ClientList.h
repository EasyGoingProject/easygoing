#define MAXSIZE 4
#define TRUE 1
#define FALSE 0
#define BOOL int

#include <winsock2.h>

typedef struct {
	//EchoServer���� ������ ���� ������ ����
	CLIENT_DATA connectionList[MAXSIZE];
	//Ŭ���̾�Ʈ ����Ʈ�� �� ũ��
	int size;
	//Ŭ���̾�Ʈ ����Ʈ�� �Է�, ������ ������ ��ġ
	int cursor;
} CLIENT_LIST, *LP_CLIENT_LIST;

void ListInit(CLIENT_LIST *list);
BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST *list);
BOOL RemoveAtClientList(CLIENT_LIST *list);
void ClearClientList(CLIENT_LIST *list);

BOOL IsClientFull(CLIENT_LIST *list);
BOOL IsClientEmpty(CLIENT_LIST *list);

