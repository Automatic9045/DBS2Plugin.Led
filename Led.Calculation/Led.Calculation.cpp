// Led.Calculation.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include <wingdi.h>

extern "C"
{
	// DrawLedDots(byte[] 描画先ビットマップ, byte[] 描画色, int 横LEDドット数, int 縦LEDドット数, double 横LEDドット間距離, double 縦LEDドット間距離, double LEDドット半径, int 描画先配列のストライド, bool 強制全再描画
	__declspec(dllexport) int __stdcall DrawLedDots(unsigned char bmp[], unsigned char dots[], int dotW, int dotH, double dotWDis, double dotHDis, double dotRad, int stride, bool forced)
	{
		int cx, cy, curR, curG, curB, d;
		bool notMatchR, notMatchG, notMatchB;

		int x, y;
		double xx, yy;
		int cxpx, cxmx, cypy, cymy, cxpy, cxmy, cypx, cymx;

		for (int i = 0; i < dotW; i++)
		{
			for (int p = 0; p < dotH; p++)
			{
				cx = (int)(dotWDis * (i + 1) + dotRad * ((i << 1) + 1));
				cy = (int)(dotHDis * (p + 1) + dotRad * ((p << 1) + 1));
				curB = dots[(i + dotW * p) * 3];
				curG = dots[(i + dotW * p) * 3 + 1];
				curR = dots[(i + dotW * p) * 3 + 2];
				notMatchR = curR != bmp[(cx << 2) + stride * cy + 2];
				notMatchG = curG != bmp[(cx << 2) + stride * cy + 1];
				notMatchB = curB != bmp[(cx << 2) + stride * cy];

				if (forced || notMatchR || notMatchG || notMatchB)
				{
					for (d = dotRad * 2; d > 0; d -= 2)
					{
						x = 0;
						y = 0;
						xx = d << 6;
						yy = 0;
						while (yy <= xx)
						{
							x = xx / 128;
							y = yy / 128;
							cxpx = cx + x;
							cxmx = cx - x;
							cypy = cy + y;
							cymy = cy - y;
							cxpy = cx + y;
							cxmy = cx - y;
							cypx = cy + x;
							cymx = cy - x;
							if (forced || notMatchB)
							{
								bmp[(cxpx << 2) + stride * cypy] = curB;
								bmp[(cxmx << 2) + stride * cymy] = curB;
								bmp[(cxmx << 2) + stride * cypy] = curB;
								bmp[(cxpx << 2) + stride * cymy] = curB;
								bmp[(cxpy << 2) + stride * cypx] = curB;
								bmp[(cxmy << 2) + stride * cymx] = curB;
								bmp[(cxmy << 2) + stride * cypx] = curB;
								bmp[(cxpy << 2) + stride * cymx] = curB;
							}
							if (forced || notMatchG)
							{
								bmp[(cxpx << 2) + stride * cypy + 1] = curG;
								bmp[(cxmx << 2) + stride * cymy + 1] = curG;
								bmp[(cxmx << 2) + stride * cypy + 1] = curG;
								bmp[(cxpx << 2) + stride * cymy + 1] = curG;
								bmp[(cxpy << 2) + stride * cypx + 1] = curG;
								bmp[(cxmy << 2) + stride * cymx + 1] = curG;
								bmp[(cxmy << 2) + stride * cypx + 1] = curG;
								bmp[(cxpy << 2) + stride * cymx + 1] = curG;
							}
							if (forced || notMatchR)
							{
								bmp[(cxpx << 2) + stride * cypy + 2] = curR;
								bmp[(cxmx << 2) + stride * cymy + 2] = curR;
								bmp[(cxmx << 2) + stride * cypy + 2] = curR;
								bmp[(cxpx << 2) + stride * cymy + 2] = curR;
								bmp[(cxpy << 2) + stride * cypx + 2] = curR;
								bmp[(cxmy << 2) + stride * cymx + 2] = curR;
								bmp[(cxmy << 2) + stride * cypx + 2] = curR;
								bmp[(cxpy << 2) + stride * cymx + 2] = curR;
							}
							yy += xx / 128;
							xx -= yy / 128;
						}
					}
				}
			}
		}

		return 0;
	}

	// mode == trueのとき、透明部分をコピーしない
	__declspec(dllexport) int __stdcall CopyImageToDots(unsigned char dots[], unsigned char source[], int x, int y, int w, int h, int dw, int dh, int dotW, int dotH, int xi, int yi, bool mode, bool init)
	{
		int curX, curY;

		int sx, sy;
		int xpw = x + dw;
		int yph = y + dh;

		int posD, posS;

		if (x < 0) curX = 0; else curX = x;
		for (; curX < xpw && curX < dotW; curX++)
		{
			if (y < 0) curY = 0; else curY = y;
			for (; curY < yph && curY < dotH; curY++)
			{
				posD = (curX + curY * dotW) * 3;

				if (init)
				{
					dots[posD] = 0x33;
					dots[posD + 1] = 0x33;
					dots[posD + 2] = 0x33;
				}

				if (xi == -1 || yi == -1)
				{
				}
				else
				{
					if (mode)
					{
						if (dots[posD] != 0x33) dots[posD] = 0x33;
						if (dots[posD + 1] != 0x33) dots[posD + 1] = 0x33;
						if (dots[posD + 2] != 0x33) dots[posD + 2] = 0x33;
					}

					sx = curX - x + dw * xi;
					sy = curY - y + dh * yi;
					posS = (sx + sy * w) << 2;
					if (source[posS + 3] == 0xff)
					{
						dots[posD] = source[posS];
						dots[posD + 1] = source[posS + 1];
						dots[posD + 2] = source[posS + 2];
					}
				}
			}
		}

		return 0;
	}

	__declspec(dllexport) int __stdcall ClearDots(unsigned char dots[], int dotW, int dotH)
	{
		for (int x = 0; x < dotW; x++)
		{
			for (int y = 0; y < dotH; y++)
			{
				dots[(x + y * dotW) * 3] = 0x33;
				dots[(x + y * dotW) * 3 + 1] = 0x33;
				dots[(x + y * dotW) * 3 + 2] = 0x33;
			}
		}

		return 0;
	}

	__declspec(dllexport) int __stdcall ReleaseLOGFONT(LOGFONT lf)
	{

		return 0;
	}

	__declspec(dllexport) int __stdcall DrawTextToDots(unsigned char dots[], unsigned char charCode, LOGFONT lf)
	{
		//unsigned char bytes = ;

		return 0;
	}
}