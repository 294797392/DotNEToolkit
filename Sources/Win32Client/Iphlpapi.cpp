#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include <Windows.h>
#include <WinUser.h>
#include <wincon.h>

#define KEYDOWN(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) ? 1 : 0)
#define KEYUP(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) ? 0 : 1)

int main()
{
	STARTUPINFOEX

	//HANDLE screenBuffer = CreateConsoleScreenBuffer(GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CONSOLE_TEXTMODE_BUFFER, NULL);
	//if(screenBuffer == NULL)
	//{
	//	return 0;
	//}

	//SetConsoleActiveScreenBuffer(screenBuffer);

	//COORD coordBufSize;
	//coordBufSize.X = 2;
	//coordBufSize.Y = 80;
	//CHAR_INFO chiBuffer[160];
	//ReadConsoleOutput(screenBuffer, chiBuffer, coordBufSize, 0,)

	//DWORD n;
	//WriteConsole(screenBuffer, "123\r\n", 3, &n, NULL);

	//DWORD readed;
	//char buffer[1024];
	//ReadConsole(screenBuffer, buffer, 1024, &readed, NULL);



	//while(1)
	//{
	//	if(KEYDOWN('A'))
	//	{
	//		printf("°´ÏÂ\n");
	//	}
	//}

	return 0;
}
