// Led.Calculation.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//

#include "stdafx.h"
#include "core.h"

using namespace std;

extern "C"
{
	// draw(byte[] 描画先ビットマップ, byte[] 各LEDドットへの描画色を記憶する配列, int 横LEDドット数, int 縦LEDドット数, double X方向のLEDドット間距離, double Y方向のLEDドット間距離, double LEDドット半径, int 描画先配列のストライド, bool 強制的に全て再描画するか)
	// dotsを基に、bmpへLED風のビットマップを描画します。
	__declspec(dllexport) void __stdcall draw(BYTE bmp[], BYTE dots[], int dotW, int dotH, double dotXDistance, double dotYDistance, double dotRadius, int stride, bool redrawAll)
	{
		led_drawing_calculator::draw(bmp, dots, dotW, dotH, dotXDistance, dotYDistance, dotRadius, stride, redrawAll);
	}

	// writeImageToDots(byte[] 各ドットへの描画色を記憶する配列, byte[] 描画する画像, int 描画を開始するLEDドットのX座標, int 描画を開始するLEDドットのY座標, int ビットマップの画像全体の横LEDドット数, int ビットマップの画像全体の縦LEDドット数, int ビットマップの描画する横LEDドット数, int ビットマップの描画する縦LEDドット数, int 横LEDドット数, int 縦LEDドット数, int フェイスインデックス, int コマインデックス, bool 透過させるか)
	// sourceの指定されたコマ・フェイスの画像を、dotsの指定された位置に書き込みます。
	__declspec(dllexport) void __stdcall writeImageToDots(BYTE dots[], BYTE source[], int x, int y, int w, int h, int drawW, int drawH, int dotW, int dotH, int xIndex, int yIndex, bool enableTransparent)
	{
		led_drawing_calculator::writeImageToDots(dots, source, x, y, w, h, drawW, drawH, dotW, dotH, xIndex, yIndex, enableTransparent);
	}

	__declspec(dllexport) void _stdcall copyDots(BYTE destination[], BYTE source[], int width, int height, int destX, int destY, int destWidth, int destHeight, int sourceX, int sourceY, int sourceWidth, int sourceHeight, bool enableTransparent)
	{
		led_drawing_calculator::copyDots(destination, source, width, height, destX, destY, destWidth, destHeight, sourceX, sourceY, sourceWidth, sourceHeight, enableTransparent);
	}

	// clearDots(byte[] 各LEDドットへの描画色を記憶する配列, int 横LEDドット数, int 縦LEDドット数)
	// dotsを無表示状態の色（#333333）で塗りつぶされた状態に上書きします。
	__declspec(dllexport) void __stdcall clearDots(BYTE dots[], int width, int height)
	{
		led_drawing_calculator::clearDots(dots, width, height);
	}
	
	// registerFont(string フォント名, int フォントサイズ, int フォントウェイト, uint アンチエイリアスフォーマット)
	// 戻り値: int フォントID
	// writeStringToDots関数で使用するフォントを登録します。戻り値のフォントIDはwriteStringToDots関数の引数で使用します。
	__declspec(dllexport) HFONT __stdcall getHfont(WCHAR fontFamily[], int fontSize, int fontWeight)
	{
		return led_drawing_calculator::getHfont(fontFamily, fontSize, fontWeight);
	}

	__declspec(dllexport) int __stdcall getStringAscent(HFONT hfont)
	{
		return led_drawing_calculator::getStringAscent(hfont);
	}

	__declspec(dllexport) int __stdcall getStringHeight(HFONT hfont)
	{
		return led_drawing_calculator::getStringHeight(hfont);
	}

	__declspec(dllexport) int __stdcall getStringWidth(WCHAR string[], HFONT hfont, UINT antialiasFormat)
	{
		return led_drawing_calculator::getStringWidth(string, hfont, antialiasFormat);
	}

	// writeStringToDots(byte[] 各ドットへの描画色を記憶する配列, string 描画する文字列, int 使用するフォントのフォントID, ulong 文字色, ulong 背景色, int 左上のX座標, int 左上のY座標, )
	// registerFont関数で登録したフォントを使用し、文字列をdotsの指定された位置に書き込みます。
	__declspec(dllexport) void __stdcall writeStringToDots(BYTE dots[], WCHAR string[], HFONT hFont, UINT antialiasFormat, COLOR64 fontColor, COLOR64 backgroundColor, int x, int y, int dotW, int dotH)
	{
		led_drawing_calculator::writeStringToDots(dots, string, hFont, antialiasFormat, fontColor, backgroundColor, x, y, dotW, dotH);
	}
}