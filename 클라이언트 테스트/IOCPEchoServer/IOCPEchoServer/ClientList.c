#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "ClientList.h"
#include <stdio.h>
#include <string.h>
#include <conio.h>
#include <stdlib.h>
#include <winsock2.h>


#pragma region [ Client List ]

void ClientListInit(CLIENT_LIST * list, int roomNumber)
{
	list->roomNumber = roomNumber;
	list->start = 0;
	list->size = 0;
	list->cursor = 0;
	printf("Init Client List\n");
}

BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST * list)
{
	int i = 0;
	if (IsClientFull(list) == 1)
	{
		printf("Max Clients\n");
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
		list->cursor++;
		printf("Added Client\n");
	}

	return TRUE;
}

void SetClientReady(CLIENT_LIST * list, int clientNumber, int ready) {
	for (int i = 0; i < list->size; i++) {
		LP_CLIENT_DATA targetClient = &(list->connectionList[i]);
		if (targetClient->clientNum == clientNumber) {
			targetClient->ready = ready;
			break;
		}
	}
}

void SetClient(CLIENT_LIST * list, int clientNumber, char *clientName, char *characterNum, int ready) {

	for (int i = 0; i < list->size; i++) {

		LP_CLIENT_DATA targetClient = &(list->connectionList[i]);
		if (targetClient->clientNum == clientNumber) {

			memcpy(targetClient->clientName, clientName, 20);
			memcpy(targetClient->characterNum, characterNum, 3);
			targetClient->ready = ready;
			break;
		}
	}
}

BOOL RemoveAtClientList(int clientNumber, CLIENT_LIST * list)
{
	for (int i = 0; i < list->size; i++) {
		if (list->connectionList[i].clientNum == clientNumber) {
			list->cursor = i;
		}
	}

	if (IsClientEmpty(list) == 1)
	{
		printf("No Client\n");
		return FALSE;
	}
	//마지막 값일때는 한개 삭제 : 외의 수치값들 조정할 필요 없음
	else if (list->cursor == list->size) {
		list->size--;
		list->cursor--;
	}
	else {
		//중간 값을 뺐을 경우 뒤쪽 값들을 땡겨줌
		for (int i = list->cursor; i < list->size - 1; i++) {
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

#pragma endregion


#pragma region [ Room List ]

void RoomListInit(ROOM_LIST * list)
{
	list->size = 0;
	list->cursor = 0;
	printf("Init Room List\n");
}

BOOL AddToRoomList(CLIENT_LIST roomList, ROOM_LIST * list)
{
	int i = 0;
	if (IsRoomFull(list) == 1)
	{
		printf("Max Room\n");
		return FALSE;
	}
	else {
		//중간위치에 클라이언트가 추가되면 해당 위치 뒤쪽 부분을 뒤로 밀어냄
		for (i = list->size - 1; i >= list->cursor; i--) {
			list->room[i + 1] = list->room[i];
		}

		//지정 위치에 값 입력
		list->room[list->cursor] = roomList;
		//클라이언트 수 증가
		list->size++;
		list->cursor++;
		printf("Added Room\n");
	}

	return TRUE;
}

BOOL RemoveAtRooomList(int roomNumber, ROOM_LIST * list)
{
	printf("Removing Room number %d\n", roomNumber);
	for (int i = 0; i < list->size; i++) {
		if (list->room[i].roomNumber == roomNumber) {
			printf("Remove Client index %d\n", i);
			list->cursor = i;
		}
	}

	if (IsRoomEmpty(list) == 1)
	{
		printf("No Room\n");
		return FALSE;
	}
	//마지막 값일때는 한개 삭제 : 외의 수치값들 조정할 필요 없음
	else if (list->cursor == list->size) {
		list->size--;
		list->cursor--;
	}
	else {
		//중간 값을 뺐을 경우 뒤쪽 값들을 땡겨줌
		for (int i = list->cursor; i < list->size - 1; i++) {
			list->room[i] = list->room[i + 1];
		}
		list->size--;
	}
	return TRUE;
}

void ClearRoomList(ROOM_LIST * list)
{

}


//방이 꽉찬 상태인지 확인
BOOL IsRoomFull(ROOM_LIST * list)
{
	if (list->size == MAXSIZE)
		return TRUE;
	else
		return FALSE;
}


//방이 비어있는 상태인지 확인
BOOL IsRoomEmpty(ROOM_LIST * list)
{
	if (list->size == 0)
		return TRUE;
	else
		return FALSE;
}

#pragma endregion