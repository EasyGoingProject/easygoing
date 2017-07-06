#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#include "EchoServer.h"
#include "ClientList.h"
#include <stdio.h>
#include <stdlib.h>
#include <winsock2.h>
#include <process.h>

void Send_ClientNumber(CLIENT_LIST *clients, LP_CLIENT_DATA clientData);
void Send_ClientList(CLIENT_LIST *clients);
unsigned int __stdcall CompletionThread(LPVOID pComPort);
void ErrorHandling(char *message);


ROOM_LIST *roomList;
CLIENT_LIST *clientList;


int main(int argc, char** argv)
{
	//Winsock ���̺귯��
	WSADATA wsaData;
	
	//IOCP
	HANDLE hCompletionPort;
	
	//�ý��� ���� : CPU ���� ȣ���
	SYSTEM_INFO SystemInfo;
	
	//���� ����
	SOCKADDR_IN servAddr;
	SOCKET hServSock;

	//����� ������ ����
	int RecvBytes;
	int i, Flags;

	int clientIndex = 0;

	//==============  IOCP ���� ���� ================

	//������ ���̺귯�� ȣ�� (Winsock 2.2 DLL)
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
		ErrorHandling("Winsock ȣ�� ����");
	else
		printf("Winsock ȣ�� ����\n");


	//Completion Port ����.
	hCompletionPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
	GetSystemInfo(&SystemInfo);


	//Completion Port���� ����� �ϷḦ ����ϴ� �����带 CPU ������ŭ ����
	for (i = 0; i < (int)SystemInfo.dwNumberOfProcessors; i++)
		_beginthreadex(NULL, 0, (LPTHREAD_START_ROUTINE)CompletionThread, (LPVOID)hCompletionPort, 0, NULL);


	//���� ���� ����
	hServSock = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	servAddr.sin_port = htons(atoi("2738"));


	//������ ������ ������������ ���
	if (bind(hServSock, (SOCKADDR*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
		ErrorHandling("���� ���� ��� ����");
	else
		printf("���� ���� ��� ����\n");


	//�ݶ��̾�Ʈ ���Ŵ�⿭ ����
	if (listen(hServSock, 5) == SOCKET_ERROR)
		ErrorHandling("Ŭ���̾�Ʈ �����¸�� ���� ����");
	else
		printf("Ŭ���̾�Ʈ ������ ���� ����\n");

	// �� ��� �޸� �Ҵ�
	roomList = (ROOM_LIST*)malloc(sizeof(ROOM_LIST));
	// �� ����Ʈ �ʱ�ȭ
	RoomListInit(roomList);

	//Ŭ���̾�Ʈ ��� �޸��Ҵ�
	clientList = (CLIENT_LIST*)malloc(sizeof(CLIENT_LIST));
	//Ŭ���̾�Ʈ ��� �ʱ�ȭ
	ClientListInit(clientList, 0);


	printf("==> IOCP ���� �غ� �Ϸ�\n\n");

	while (TRUE)
	{
		//Ŭ���̾�Ʈ ���ӿ� ���� ���� ���
		SOCKET hClntSock;
		SOCKADDR_IN clntAddr;
		int addrLen = sizeof(clntAddr);
		int isReady = 0;
		int readyLen = sizeof(isReady);

		//Ŭ���̾�Ʈ ���ٽ� ���� ����
		hClntSock = accept(hServSock, (SOCKADDR*)&clntAddr, &addrLen);

		if (hClntSock == INVALID_SOCKET)
			ErrorHandling("Ŭ���̾�Ʈ ���� ���� ����");
		else
			printf("[Client %s ���� ����]\n", inet_ntoa(clntAddr.sin_addr));


		//����� Ŭ���̾�Ʈ�� �����ڵ������� �ּ�����������
		LP_IO_DATA lpIoData;
		LP_CLIENT_DATA lpClientData;

		lpClientData = (LP_CLIENT_DATA)malloc(sizeof(CLIENT_DATA));
		lpClientData->hClntSock = hClntSock;
		lpClientData->clientNum = clientIndex;
		memcpy(&(lpClientData->clntAddr), &clntAddr, addrLen);
		memcpy(&(lpClientData->ready), &isReady, readyLen);

		clientIndex++;

		//Ŭ���̾�Ʈ ��Ͽ� �߰�
		if (!IsClientFull(clientList)) {
			AddToClientList(*lpClientData, clientList);
		}
		else
		{
			printf("\n[ Full Room ]\n");
		}

		Send_ClientNumber(clientList, lpClientData);

		printf("\n[������ �÷��̾� ��Ȳ]\n");

		for (int i = 0; i < clientList->size; i++) {
			printf("�÷��̾� %d : Address[%s]  Port[%d]\n", 
				clientList->connectionList[i].clientNum,
				inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr),
				ntohs(clientList->connectionList[i].clntAddr.sin_port));
		}

		printf("\n");

		//Overlapped ���ϰ� CompletionPort�� ����
		CreateIoCompletionPort((HANDLE)hClntSock, hCompletionPort, (DWORD)lpClientData, 0);


		//����� Ŭ���̾�Ʈ�� ���� ���ۼ���
		//OVERLAPPED����ü ���� �ʱ�ȭ
		lpIoData = (LP_IO_DATA)malloc(sizeof(IO_DATA));
		memset(&(lpIoData->overlapped), 0, sizeof(OVERLAPPED));
		lpIoData->wsaBuf.len = BUFSIZE;
		lpIoData->wsaBuf.buf = lpIoData->buffer;
		Flags = 0;


		//��ø�ȵ������Է�.
		WSARecv(
			lpClientData->hClntSock,	//���� ������ ����Ű�� ���� ���� ��ȣ
			&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
			1,							//����ü�� ����
			(LPDWORD)&RecvBytes,		//������ �Է��� �Ϸ�� ���, ���� �������� ����Ʈ ũ�� output
			(LPDWORD)&Flags,			//WSARecv �Լ��� ȣ����
			&lpIoData->overlapped,		//Overlapped ����ü�� ������
			NULL						//������ �Է��� �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
		);
	}
	return 0;
}

void Send_ClientNumber(CLIENT_LIST *clients, LP_CLIENT_DATA clientData) {
	char buffer[64];
	snprintf(buffer, sizeof(buffer), "Server-ClientNumber-%d,%d",
		clients->size,
		clientData->clientNum);

	printf(buffer);
	printf("\n");

	int sendBytes = send(clientData->hClntSock, buffer, sizeof(buffer), 0);
}

void Send_ClientList(CLIENT_LIST *clients) {
	char buffer[255] = "";
	strcat(buffer, "Server-ClientList-");

	printf("Client List Count %d\n", clients->size);
	
	for (int i = 0; i < clients->size; i++) {

		CLIENT_DATA client = clients->connectionList[i];

		printf("Client-%d : %d : [%d]\n", 
			i, 
			client.clientNum,
			ntohs(client.clntAddr.sin_port));
		
		char clientIndexStr[6];
		char clientNumberStr[6];
		char clientNameStr[20];
		char clientCharacterStr[5];
		char clientReady[3];

		sprintf(&clientIndexStr, "%d", i);
		sprintf(&clientNumberStr, "%d", client.clientNum);
		sprintf(&clientNameStr, "%s", client.clientName);
		sprintf(&clientCharacterStr, "%s", client.characterNum);
		sprintf(&clientReady, "%d", client.ready);
		
		strcat(buffer, clientIndexStr);
		strcat(buffer, "=");
		strcat(buffer, clientNumberStr);
		strcat(buffer, "=");
		strcat(buffer, clientNameStr);
		strcat(buffer, "=");
		strcat(buffer, clientCharacterStr);
		strcat(buffer, "=");
		strcat(buffer, clientReady);
		strcat(buffer, ",");
	}

	printf(buffer);
	printf("\n");

	for (int i = 0; i < clients->size + 1; i++) {
		int sendBytes = send(clients->connectionList[i].hClntSock, buffer, sizeof(buffer), 0);
	}
}

//������� ����ϴ� �������� �ൿ ����
unsigned int __stdcall CompletionThread(LPVOID pComPort)
{
	//IOCP
	HANDLE hCompletionPort = (HANDLE)pComPort;
	//���۹��� ����Ʈ
	DWORD BytesTransferred;
	//����µ� ������
	LP_IO_DATA lpIoData;
	DWORD flags;

	LP_CLIENT_DATA lpClientData;
	LP_CLIENT_DATA lpTargetClient;

	char serverDataBuf[BUFSIZE];


	//������ �ൿ ����
	while (TRUE)
	{
		//������� �Ϸ�� ������ ���� Ȯ��
		if (GetQueuedCompletionStatus(
			hCompletionPort,			// Completion Port
			&BytesTransferred,			// ���۵ȹ���Ʈ��
			(LPDWORD)&lpClientData,
			(LPOVERLAPPED*)&lpIoData,	// OVERLAPPED ����ü������.
			INFINITE) == 0)
		{
			printf("Error - GetQueuedCompletionStatus\n");

			RemoveAtClientList(lpClientData->clientNum, clientList);

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			
			Send_ClientList(clientList);

			return 1;
		}

		//EOF ���۽�
		if (BytesTransferred == 0)
		{
			printf("Ŭ���̾�Ʈ ���� : Address[%s]  Port[%d]\n",
				inet_ntoa(lpClientData->clntAddr.sin_addr),
				ntohs(lpClientData->clntAddr.sin_port));

			RemoveAtClientList(lpClientData->clientNum, clientList);

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			Send_ClientList(clientList);

			continue;
		}
		else {
			//�Է� ���� ������ ó��
			if (strstr(lpIoData->wsaBuf.buf, "Client") != NULL) {
				if (strstr(lpIoData->wsaBuf.buf, "Connected") != NULL) {
					char *ptr = strtok(lpIoData->wsaBuf.buf, ",");

					int ptrIndex = 0;
					char *ptrArray[6];

					while (ptr != NULL)
					{
						ptrArray[ptrIndex++] = ptr;
						ptr = strtok(NULL, ",");
					}

					printf("Connected:%s-%s-%s-%s\n", ptrArray[1], ptrArray[2], ptrArray[3], ptrArray[4]);
					
					SetClient(clientList, atoi(ptrArray[1]), ptrArray[2], ptrArray[3], atoi(ptrArray[4]));

					Send_ClientList(clientList);
				}
				if (strstr(lpIoData->wsaBuf.buf, "Ready") != NULL) {
					char *ptr = strtok(lpIoData->wsaBuf.buf, ",");

					int ptrIndex = 0;
					char *ptrArray2[4];

					while (ptr != NULL)
					{
						ptrArray2[ptrIndex++] = ptr;
						ptr = strtok(NULL, ",");
					}

					printf("Ready:%s-%s\n", ptrArray2[1], ptrArray2[2]);

					SetClientReady(clientList, atoi(ptrArray2[1]), atoi(ptrArray2[2]));

					Send_ClientList(clientList);
				}
			}
			else {


				//�Է¹��� �����͸� Ŭ���̾�Ʈ�� ���� (����)
				lpIoData->wsaBuf.len = BytesTransferred;

				for (int i = 0; i < clientList->size; i++) {
					lpTargetClient = &clientList->connectionList[i];

					printf("[%d : %s:%d]�� ����\n",
						clientList->connectionList[i].clientNum,
						inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr),
						ntohs(clientList->connectionList[i].clntAddr.sin_port));

					if (WSASend(
						lpTargetClient->hClntSock,	//���� ������ ����Ű�� ���� ���� ��ȣ
						&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
						1,							//����ü�� ����
						lpIoData->wsaBuf.buf,		//�Լ��� ȣ��� ���۵� �������� ����Ʈ ũ��
						0,							//WSASend �Լ��� ȣ����
						NULL,						//Overlapped ����ü�� ������
						NULL						//������ ������ �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
					) == SOCKET_ERROR)
					{
						if (WSAGetLastError() != WSA_IO_PENDING) {
							printf("WSASend Error : %d\n", WSAGetLastError());
							continue;
						}
					}
				}
			}

			//�޸� �ʱ�ȭ �� ������ ��Ȯ��
			memset(&(lpIoData->overlapped), 0, sizeof(OVERLAPPED));
			lpIoData->wsaBuf.len = BUFSIZE;
			lpIoData->wsaBuf.buf = lpIoData->buffer;

			flags = 0;

			//���ú� ó��
			if (WSARecv(
				lpClientData->hClntSock,	//���� ������ ����Ű�� ���� ���� ��ȣ
				&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
				1,							//����ü�� ����
				lpIoData->wsaBuf.buf,		//������ �Է��� �Ϸ�� ���, ���� �������� ����Ʈ ũ�� output
				&flags,						//WSARecv �Լ��� ȣ����
				&lpIoData->overlapped,		//Overlapped ����ü�� ������
				NULL						//������ �Է��� �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
			) == SOCKET_ERROR) {
				if (WSAGetLastError() != WSA_IO_PENDING) {
					printf("WSARecv Error : %d\n", WSAGetLastError());
				}
			}
		}
	}
	return 0;
}

//���� ó�� : ���� �޽��� ��� �� ����
void ErrorHandling(char *message)
{
	fputs(message, stderr);
	fputc('\n', stderr);
	exit(1);

}

char * toArray(int number)
{
	int n = log10(number) + 1;
	int i;
	char *numberArray = calloc(n, sizeof(char));
	for (i = 0; i < n; ++i, number /= 10)
	{
		numberArray[i] = number % 10;
	}
	return numberArray;
}
