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
	printf("Ŭ���̾�Ʈ ����Ʈ �ʱ�ȭ\n");
}

BOOL AddToClientList(CLIENT_DATA connection, CLIENT_LIST * list)
{
	int i = 0;
	if (IsClientFull(list) == 1)
	{
		printf("�ִ��ο�\n");
		return FALSE;
	}
	else {
		//�߰���ġ�� Ŭ���̾�Ʈ�� �߰��Ǹ� �ش� ��ġ ���� �κ��� �ڷ� �о
		for (i = list->size - 1; i >= list->cursor; i--) {
			list->connectionList[i + 1] = list->connectionList[i];
		}

		//���� ��ġ�� �� �Է�
		list->connectionList[list->cursor] = connection;
		//Ŭ���̾�Ʈ �� ����
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
		printf("Ŭ���̾�Ʈ�� �����ϴ�\n");
		return FALSE;
	}
	//������ ���϶��� �Ѱ� ���� : ���� ��ġ���� ������ �ʿ� ����
	else if (list->cursor == list->size) {
		list->size--;
		list->cursor--;
	}
	else {
		//�߰� ���� ���� ��� ���� ������ ������
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


//���� ���� �������� Ȯ��
BOOL IsClientFull(CLIENT_LIST * list)
{
	if (list->size == MAXSIZE)
		return TRUE;
	else
		return FALSE;
}


//���� ����ִ� �������� Ȯ��
BOOL IsClientEmpty(CLIENT_LIST * list)
{
	if (list->size == 0)
		return TRUE;
	else
		return FALSE;
}
