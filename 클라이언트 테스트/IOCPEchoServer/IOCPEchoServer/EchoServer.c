#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "EchoServer.h"
#include "ClientList.h"
#include <stdio.h>
#include <stdlib.h>
#include <winsock2.h>
#include <process.h>


unsigned int __stdcall CompletionThread(LPVOID pComPort);
void ErrorHandling(char *message);

CLIENT_LIST *clientList;

static int threadNum = 0;


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
	LP_IO_DATA lpIoData;
	LP_CLIENT_DATA lpClientData;
	int RecvBytes;
	int i, Flags;


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

	//Ŭ���̾�Ʈ ��� �޸��Ҵ�
	clientList = (CLIENT_LIST*)malloc(sizeof(CLIENT_LIST));

	//Ŭ���̾�Ʈ ��� �ʱ�ȭ
	ListInit(clientList);


	printf("==> IOCP ���� �غ� �Ϸ�\n\n");


	while (TRUE)
	{
		//Ŭ���̾�Ʈ ���ӿ� ���� ���� ���
		SOCKET hClntSock;
		SOCKADDR_IN clntAddr;
		int addrLen = sizeof(clntAddr);


		//Ŭ���̾�Ʈ ���ٽ� ���� ����
		hClntSock = accept(hServSock, (SOCKADDR*)&clntAddr, &addrLen);

		if (hClntSock == INVALID_SOCKET)
			ErrorHandling("Ŭ���̾�Ʈ ���� ���� ����");
		else
			printf("[Client %s ���� ����]\n", inet_ntoa(clntAddr.sin_addr));


		//����� Ŭ���̾�Ʈ�� �����ڵ������� �ּ�����������
		lpClientData = (LP_CLIENT_DATA)malloc(sizeof(CLIENT_DATA));
		lpClientData->hClntSock = hClntSock;
		memcpy(&(lpClientData->clntAddr), &clntAddr, addrLen);


		//Ŭ���̾�Ʈ ��Ͽ� �߰�
		AddToClientList(*lpClientData, clientList);

		printf("\n[������ �÷��̾� ��Ȳ]\n");

		for (int i = 0; i < clientList->size; i++) {
			printf("�÷��̾� %d : Address[%s]  Port[%d]\n", 
				(i + 1), 
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
			&lpIoData->wsaBuf,		//����ü �迭�� ������, ����ũ��
			1,							//����ü�� ����
			(LPDWORD)&RecvBytes,		//������ �Է��� �Ϸ�� ���, ���� �������� ����Ʈ ũ�� output
			(LPDWORD)&Flags,			//WSARecv �Լ��� ȣ����
			&(lpIoData->overlapped),	//Overlapped ����ü�� ������
			NULL						//������ �Է��� �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
		);
	}
	return 0;
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
	LP_CLIENT_DATA lpClientData;
	DWORD flags;

	
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

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			RemoveAtClientList(clientList);
			return 1;
		}

		//EOF ���۽�
		if (BytesTransferred == 0)
		{
			printf("Ŭ���̾�Ʈ ���� : Address[%s]  Port[%d]\n",
				inet_ntoa(lpClientData->clntAddr.sin_addr),
				ntohs(lpClientData->clntAddr.sin_port));

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			RemoveAtClientList(clientList);
			continue;
		}
		else {

			//�Է� ���� ������ ó��
			lpIoData->wsaBuf.buf[BytesTransferred] = '\0';
			printf("[%s:%d]���� ���� : %s\n",
				inet_ntoa(lpClientData->clntAddr.sin_addr),
				ntohs(lpClientData->clntAddr.sin_port),
				lpIoData->wsaBuf.buf);


			//�Է¹��� �����͸� Ŭ���̾�Ʈ�� ���� (����)
			lpIoData->wsaBuf.len = BytesTransferred;

			printf("���۹��� Ŭ���̾�Ʈ �� %d\n", clientList->size);

			for (int i = 0; i < clientList->size; i++) {
				LP_CLIENT_DATA lpTargetClient = &clientList->connectionList[i];

				printf("[%d : %s:%d]�� ����\n",
					i,
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
				&BytesTransferred,			//������ �Է��� �Ϸ�� ���, ���� �������� ����Ʈ ũ�� output
				&flags,						//WSARecv �Լ��� ȣ����
				&lpIoData->overlapped,	//Overlapped ����ü�� ������
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