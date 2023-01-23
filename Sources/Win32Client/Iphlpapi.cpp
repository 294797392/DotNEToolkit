#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include <Windows.h>
#include <WinUser.h>

#define KEYDOWN(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) ? 1 : 0)
#define KEYUP(vk_code) ((GetAsyncKeyState(vk_code) & 0x8000) ? 0 : 1)

int main() 
{
	while(1)
	{
		if(KEYDOWN('A'))
		{
			printf("°´ÏÂ\n");
		}
	}

	return 0;
}


