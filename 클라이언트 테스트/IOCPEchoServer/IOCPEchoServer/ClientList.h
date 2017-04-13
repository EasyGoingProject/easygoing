#define MAXSIZE 4
#define TRUE 1
#define FALSE 0
#define BOOL int

#include <winsock2.h>

typedef struct {
	//EchoServer에서 선언한 소켓 데이터 구조
	CLIENT_DATA connectionList[MAXSIZE];
	//클라이언트 리스트의 총 크기
	int size;
	//클라이언트 리스트에 입력, 삭제를 실행할 위치
	int cursor;
} CLIENT_LIST, *LP_CLIENT_LIST;

void ListInit(CLIENT_LIST *list);
BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST *list);
BOOL RemoveAtClientList(CLIENT_LIST *list);
void ClearClientList(CLIENT_LIST *list);

BOOL IsClientFull(CLIENT_LIST *list);
BOOL IsClientEmpty(CLIENT_LIST *list);

