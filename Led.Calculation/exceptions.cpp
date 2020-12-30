#include "stdafx.h"

using namespace std;

namespace led_drawing_calculator
{
	// 指定されたフォントファミリーが見つからなかったことを示す例外です。
	class FontFamilyNotFoundException : public invalid_argument {};

	// 無効なアンチエイリアスフォーマットが指定されたことを示す例外です。
	class InvalidAntialiasFormatException : public invalid_argument {};
}