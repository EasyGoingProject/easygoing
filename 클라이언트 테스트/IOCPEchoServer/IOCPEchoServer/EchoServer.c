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
	//Winsock 라이브러리
	WSADATA wsaData;
	
	//IOCP
	HANDLE hCompletionPort;
	
	//시스템 정보 : CPU 정보 호출용
	SYSTEM_INFO SystemInfo;
	
	//서버 소켓
	SOCKADDR_IN servAddr;
	SOCKET hServSock;

	//입출력 데이터 변수
	LP_IO_DATA lpIoData;
	LP_CLIENT_DATA lpClientData;
	int RecvBytes;
	int i, Flags;


	//==============  IOCP 서버 시작 ================

	//윈소켓 라이브러리 호출 (Winsock 2.2 DLL)
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
		ErrorHandling("Winsock 호출 실패");
	else
		printf("Winsock 호출 성공\n");


	//Completion Port 생성.
	hCompletionPort = CreateIoCompletionPort(INVALID_HANDLE_VALUE, NULL, 0, 0);
	GetSystemInfo(&SystemInfo);


	//Completion Port에서 입출력 완료를 대기하는 쓰레드를 CPU 개수만큼 생성
	for (i = 0; i < (int)SystemInfo.dwNumberOfProcessors; i++)
		_beginthreadex(NULL, 0, (LPTHREAD_START_ROUTINE)CompletionThread, (LPVOID)hCompletionPort, 0, NULL);


	//서버 소켓 설정
	hServSock = WSASocket(AF_INET, SOCK_STREAM, 0, NULL, 0, WSA_FLAG_OVERLAPPED);
	servAddr.sin_family = AF_INET;
	servAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	servAddr.sin_port = htons(atoi("2738"));


	//생성한 소켓을 서버소켓으로 등록
	if (bind(hServSock, (SOCKADDR*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
		ErrorHandling("서버 소켓 등록 실패");
	else
		printf("서버 소켓 등록 성공\n");


	//콜라이언트 수신대기열 생성
	if (listen(hServSock, 5) == SOCKET_ERROR)
		ErrorHandling("클라이언트 대기상태모드 진입 실패");
	else
		printf("클라이언트 대기상태 진입 성공\n");

	//클라이언트 목록 메모리할당
	clientList = (CLIENT_LIST*)malloc(sizeof(CLIENT_LIST));

	//클라이언트 목록 초기화
	ListInit(clientList);


	printf("==> IOCP 서버 준비 완료\n\n");


	while (TRUE)
	{
		//클라이언트 접속용 소켓 변수 등록
		SOCKET hClntSock;
		SOCKADDR_IN clntAddr;
		int addrLen = sizeof(clntAddr);


		//클라이언트 접근시 연결 수락
		hClntSock = accept(hServSock, (SOCKADDR*)&clntAddr, &addrLen);

		if (hClntSock == INVALID_SOCKET)
			ErrorHandling("클라이언트 연결 수락 실패");
		else
			printf("[Client %s 연결 성공]\n", inet_ntoa(clntAddr.sin_addr));


		//연결된 클라이언트의 소켓핸들정보와 주소정보를설정
		lpClientData = (LP_CLIENT_DATA)malloc(sizeof(CLIENT_DATA));
		lpClientData->hClntSock = hClntSock;
		memcpy(&(lpClientData->clntAddr), &clntAddr, addrLen);


		//클라이언트 목록에 추가
		AddToClientList(*lpClientData, clientList);

		printf("\n[접속중 플레이어 현황]\n");

		for (int i = 0; i < clientList->size; i++) {
			printf("플레이어 %d : Address[%s]  Port[%d]\n", 
				(i + 1), 
				inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr),
				ntohs(clientList->connectionList[i].clntAddr.sin_port));
		}

		printf("\n");

		//Overlapped 소켓과 CompletionPort의 연결
		CreateIoCompletionPort((HANDLE)hClntSock, hCompletionPort, (DWORD)lpClientData, 0);


		//연결된 클라이언트를 위한 버퍼설정
		//OVERLAPPED구조체 변수 초기화
		lpIoData = (LP_IO_DATA)malloc(sizeof(IO_DATA));
		memset(&(lpIoData->overlapped), 0, sizeof(OVERLAPPED));
		lpIoData->wsaBuf.len = BUFSIZE;
		lpIoData->wsaBuf.buf = lpIoData->buffer;
		Flags = 0;


		//중첩된데이터입력.
		WSARecv(
			lpClientData->hClntSock,	//연결 소켓을 가리키는 소켓 지정 번호
			&lpIoData->wsaBuf,		//구조체 배열의 포인터, 버퍼크기
			1,							//구조체의 개수
			(LPDWORD)&RecvBytes,		//데이터 입력이 완료된 경우, 읽은 데이터의 바이트 크기 output
			(LPDWORD)&Flags,			//WSARecv 함수의 호출방식
			&(lpIoData->overlapped),	//Overlapped 구조체의 포인터
			NULL						//데이터 입력이 완료 되었을 때 호출할 완료 루틴
		);
	}
	return 0;
}


//입출력을 담당하는 쓰레드의 행동 정의
unsigned int __stdcall CompletionThread(LPVOID pComPort)
{
	//IOCP
	HANDLE hCompletionPort = (HANDLE)pComPort;
	//전송받을 바이트
	DWORD BytesTransferred;
	//입출력될 데이터
	LP_IO_DATA lpIoData;
	LP_CLIENT_DATA lpClientData;
	DWORD flags;

	
	//쓰레드 행동 시작
	while (TRUE)
	{
		//입출력이 완료된 소켓의 정보 확보
		if (GetQueuedCompletionStatus(
			hCompletionPort,			// Completion Port
			&BytesTransferred,			// 전송된바이트수
			(LPDWORD)&lpClientData,
			(LPOVERLAPPED*)&lpIoData,	// OVERLAPPED 구조체포인터.
			INFINITE) == 0)
		{
			printf("Error - GetQueuedCompletionStatus\n");

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			RemoveAtClientList(clientList);
			return 1;
		}

		//EOF 전송시
		if (BytesTransferred == 0)
		{
			printf("클라이언트 종료 : Address[%s]  Port[%d]\n",
				inet_ntoa(lpClientData->clntAddr.sin_addr),
				ntohs(lpClientData->clntAddr.sin_port));

			closesocket(lpClientData->hClntSock);
			free(lpClientData);
			free(lpIoData);

			RemoveAtClientList(clientList);
			continue;
		}
		else {

			//입력 받은 데이터 처리
			lpIoData->wsaBuf.buf[BytesTransferred] = '\0';
			printf("[%s:%d]에서 전송 : %s\n",
				inet_ntoa(lpClientData->clntAddr.sin_addr),
				ntohs(lpClientData->clntAddr.sin_port),
				lpIoData->wsaBuf.buf);


			//입력받은 데이터를 클라이언트로 전송 (에코)
			lpIoData->wsaBuf.len = BytesTransferred;

			printf("전송받을 클라이언트 수 %d\n", clientList->size);

			for (int i = 0; i < clientList->size; i++) {
				LP_CLIENT_DATA lpTargetClient = &clientList->connectionList[i];

				printf("[%d : %s:%d]로 전송\n",
					i,
					inet_ntoa(clientList->connectionList[i].clntAddr.sin_addr),
					ntohs(clientList->connectionList[i].clntAddr.sin_port));

				if (WSASend(
					lpTargetClient->hClntSock,	//연결 소켓을 가리키는 소켓 지정 번호
					&(lpIoData->wsaBuf),		//구조체 배열의 포인터, 버퍼크기
					1,							//구조체의 개수
					lpIoData->wsaBuf.buf,		//함수의 호출로 전송된 데이터의 바이트 크기
					0,							//WSASend 함수의 호출방식
					NULL,						//Overlapped 구조체의 포인터
					NULL						//데이터 전송이 완료 되었을 때 호출할 완료 루틴
				) == SOCKET_ERROR)
				{
					if (WSAGetLastError() != WSA_IO_PENDING) {
						printf("WSASend Error : %d\n", WSAGetLastError());
						continue;
					}
				}
			}

			//메모리 초기화 및 데이터 재확보
			memset(&(lpIoData->overlapped), 0, sizeof(OVERLAPPED));
			lpIoData->wsaBuf.len = BUFSIZE;
			lpIoData->wsaBuf.buf = lpIoData->buffer;

			flags = 0;

			//리시브 처리
			if (WSARecv(
				lpClientData->hClntSock,	//연결 소켓을 가리키는 소켓 지정 번호
				&(lpIoData->wsaBuf),		//구조체 배열의 포인터, 버퍼크기
				1,							//구조체의 개수
				&BytesTransferred,			//데이터 입력이 완료된 경우, 읽은 데이터의 바이트 크기 output
				&flags,						//WSARecv 함수의 호출방식
				&lpIoData->overlapped,	//Overlapped 구조체의 포인터
				NULL						//데이터 입력이 완료 되었을 때 호출할 완료 루틴
			) == SOCKET_ERROR) {
				if (WSAGetLastError() != WSA_IO_PENDING) {
					printf("WSARecv Error : %d\n", WSAGetLastError());
				}
			}
		}
	}
	return 0;
}


//오류 처리 : 오류 메시지 출력 후 종료
void ErrorHandling(char *message)
{
	fputs(message, stderr);
	fputc('\n', stderr);
	exit(1);
}