#pragma once

using namespace std;

#define LEDDOT_DEFAULT_COLOR 0x33

typedef DWORD COLOR64;

namespace led_drawing_calculator
{
	void draw(BYTE bmp[], BYTE dots[], int dotWidth, int dotHeight, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll);
	HFONT getHfont(wstring fontFamily, int fontSize, int fontWeight);
	int getStringAscent(HFONT hfont);
	int getStringHeight(HFONT hfont);
	int getStringWidth(wstring string, HFONT hfont, UINT antialiasFormat);
	void writeStringToDots(BYTE dots[], wstring string, HFONT hfont, UINT antialiasFormat, COLOR64 fontColor, COLOR64 backgroundColor, int x, int y, int dotWidth, int dotHeight);
	void writeImageToDots(BYTE dots[], BYTE source[], int x, int y, int width, int height, int drawWidth, int drawHeight, int dotWidth, int dotHeight, int xIndex, int yIndex, bool enableTransparent);
	void copyDots(BYTE destination[], BYTE source[], int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent);
	void fillDots(BYTE dots[], COLOR64 color, int x, int y, int drawWidth, int drawHeight, int width, int height);
	void clearDots(BYTE dots[], int width, int height);
}