#include "pch.h"

#ifdef _WIN32

#include <Windows.h>
#include <gl/GL.h>

static PIXELFORMATDESCRIPTOR pfd = {
	sizeof(PIXELFORMATDESCRIPTOR),
	1,
	PFD_DRAW_TO_BITMAP | PFD_SUPPORT_OPENGL,
	PFD_TYPE_RGBA,
	32,
	0, 0, 0, 0, 0, 0, 0, 0,
	0, 0, 0, 0, 0,
	32,
	0,
	0,
	PFD_MAIN_PLANE,
	0,
	0, 0, 0,
};

DLLExport(void) CreateRenderingContext(int width, int height, HDC* hDC, HGLRC* hRC, HBITMAP* hbmp) {
	BITMAPINFO bmi = {
	{
		sizeof(BITMAPINFOHEADER),
		width,
		height,
		1,
		32,
		BI_RGB,
		0, 0, 0, 0, 0,
	},
	};

	*hDC = CreateCompatibleDC(NULL);

	/* �`��̈�ƂȂ�r�b�g�}�b�v���쐬 */
	void* pvBits;
	*hbmp = CreateDIBSection(*hDC, &bmi, DIB_RGB_COLORS, &pvBits, NULL, 0);
	
	/* HDC�Ƀr�b�g�}�b�v��ݒ� */
	SelectObject(*hDC, *hbmp);

	/* �s�N�Z���t�H�[�}�b�g��ݒ� */
	int pixFormat = ChoosePixelFormat(*hDC, &pfd);
	SetPixelFormat(*hDC, pixFormat, &pfd);

	*hRC = wglCreateContext(*hDC);
}

DLLExport(void) MakeCurrent(HDC hdc, HGLRC hrc) {
	wglMakeCurrent(hdc, hrc);
}

DLLExport(void) DeleteRenderingContext(HDC hDC, HGLRC hRC, HBITMAP hbmp) {
	wglMakeCurrent(NULL, NULL);
	wglDeleteContext(hRC);
	DeleteObject(hbmp);
	DeleteDC(hDC);
}

#endif // __Win32
