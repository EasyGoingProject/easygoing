#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "EchoServer.h"
#include "ClientList.h"
#include <stdio.h>
#include <string.h>
#include <conio.h>
#include <stdlib.h>
#include <winsock2.h>

void ListInit(CLIENT_LIST * list)
{
	list->size = 0;
	list->cursor = 0;
	printf("클라이언트 리스트 초기화\n");
}

BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST * list)
{
	int i = 0;
	if (IsClientFull(list) == 1)
	{
		printf("최대인원\n");
		return FALSE;
	}
	else {
		//중간위치에 클라이언트가 추가되면 해당 위치 뒤쪽 부분을 뒤로 밀어냄
		for (i = list->size - 1; i >= list->cursor; i--) {
			list->connectionList[i + 1] = list->connectionList[i];
		}

		//지정 위치에 값 입력
		list->connectionList[list->cursor] = connection;
		//클라이언트 수 증가
		list->size++;
	}

	/*for (i = 0; i < list->size; i++) {
		printf("%s\n", inet_ntoa(list->connectionList[i].clntAddr.sin_addr));
	}*/

	return TRUE;
}

BOOL RemoveAtClientList(CLIENT_LIST * list)
{
	int i = 0;

	if (IsClientEmpty(list) == 1)
	{
		printf("클라이언트가 없습니다\n");
		return FALSE;
	}
	//마지막 값일때는 한개 삭제 : 외의 수치값들 조정할 필요 없음
	else if (list->cursor == list->size) {
		list->size--;
		list->cursor--;
	}
	else {
		//중간 값을 뺐을 경우 뒤쪽 값들을 땡겨줌
		for (i = list->cursor; i < list->size - 1; i++) {
			list->connectionList[i] = list->connectionList[i + 1];
		}
		list->size--;
	}
	return TRUE;
}

void ClearClientList(CLIENT_LIST * list)
{

}


//방이 꽉찬 상태인지 확인
BOOL IsClientFull(CLIENT_LIST * list)
{
	if (list->size == MAXSIZE)
		return TRUE;
	else
		return FALSE;
}


//방이 비어있는 상태인지 확인
BOOL IsClientEmpty(CLIENT_LIST * list)
{
	if (list->size == 0)
		return TRUE;
	else
		return FALSE;
}
