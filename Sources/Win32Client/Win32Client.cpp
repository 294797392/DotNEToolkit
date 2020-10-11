#include <stdlib.h>

#include <combaseapi.h>
#include <d2d1.h>

int main()
{
	//LPOLESTR oleStr = NULL;
	//StringFromIID(IID_ID2D1Factory, &oleStr);

	SUCCEEDED

	IID iid;
	IIDFromString(OLESTR("{06152247-6F50-465A-9245-118BFD3B6007}"), &iid);

	void *p = NULL;
	HRESULT rc = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, iid, NULL, &p);
	return 0;
}
