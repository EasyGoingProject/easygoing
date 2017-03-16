#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include "EchoServer.h"
#include <stdio.h>
#include <stdlib.h>
#include <winsock2.h>
#include <process.h>
#include "ClientList.h"


unsigned int __stdcall CompletionThread(LPVOID pComPort);
void ErrorHandling(char *message);


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
		_beginthreadex(NULL, 0, CompletionThread, (LPVOID)hCompletionPort, 0, NULL);


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
			printf("�÷��̾� %d : %s\n", 
				(i + 1), 
				inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr));
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
	LP_CLIENT_DATA lpClientData;
	LP_IO_DATA lpIoData;
	DWORD flags;


	//������ �ൿ ����
	while (TRUE)
	{
		//������� �Ϸ�� ������ ���� Ȯ��
		GetQueuedCompletionStatus(
			hCompletionPort,			// Completion Port
			&BytesTransferred,			// ���۵ȹ���Ʈ��
			(LPDWORD)&lpClientData,
			(LPOVERLAPPED*)&lpIoData,	// OVERLAPPED ����ü������.
			INFINITE
		);


		//EOF ���۽�
		if (BytesTransferred == 0) 
		{
			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);
			continue;
		}


		//�Է� ���� ������ ó��
		lpIoData->wsaBuf.buf[BytesTransferred] = '\0';
		printf("[%s]���� ���� : %s\n", 
				inet_ntoa(lpClientData->clntAddr.sin_addr), 
				lpIoData->wsaBuf.buf);


		//�Է¹��� �����͸� Ŭ���̾�Ʈ�� ���� (����)
		lpIoData->wsaBuf.len = BytesTransferred;
		
		//WSASend(
		//	lpClientData->hClntSock,	//���� ������ ����Ű�� ���� ���� ��ȣ
		//	&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
		//	1,							//����ü�� ����
		//	NULL,						//�Լ��� ȣ��� ���۵� �������� ����Ʈ ũ��
		//	0,							//WSASend �Լ��� ȣ����
		//	NULL,						//Overlapped ����ü�� ������
		//	NULL						//������ ������ �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
		//);

		for (int i = 0; i < clientList->size; i++) {
			printf("Ŭ���̾�Ʈ�� ���� : %s\n", 
				inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr));

			lpClientData = &clientList->connectionList[i];

			WSASend(
				lpClientData->hClntSock,
											//���� ������ ����Ű�� ���� ���� ��ȣ
				&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
				1,							//����ü�� ����
				NULL,						//�Լ��� ȣ��� ���۵� �������� ����Ʈ ũ��
				0,							//WSASend �Լ��� ȣ����
				NULL,						//Overlapped ����ü�� ������
				NULL						//������ ������ �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
			);
		}

		printf("\n");


		//�޸� �ʱ�ȭ �� ������ ��Ȯ��
		memset(&(lpIoData->overlapped), 0, sizeof(OVERLAPPED));
		lpIoData->wsaBuf.len = BUFSIZE;
		lpIoData->wsaBuf.buf = lpIoData->buffer;

		flags = 0;


		//���ú� ó��
		WSARecv(
			lpClientData->hClntSock,	//���� ������ ����Ű�� ���� ���� ��ȣ
			&(lpIoData->wsaBuf),		//����ü �迭�� ������, ����ũ��
			1,							//����ü�� ����
			NULL,						//������ �Է��� �Ϸ�� ���, ���� �������� ����Ʈ ũ�� output
			&flags,						//WSARecv �Լ��� ȣ����
			&(lpIoData->overlapped),	//Overlapped ����ü�� ������
			NULL						//������ �Է��� �Ϸ� �Ǿ��� �� ȣ���� �Ϸ� ��ƾ
		);
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