Screen Watermark
    Screen Watermark is an application that overlays an image over the desktop
and can act as a flexible watermark on videos or pictures taken directly
off the screen. The main use is for adding in a watermark when your video
or screen capture software does not support watermarks correctly or at all.
Just drag and drop your PNG file on ScreenWatermark.exe to start.

Configuration file is config.xml
    You can change the default starting position.

Key Bindings:
W: one pixel up
S: one pixel down
A: one pixel left
D: one pixel right

TGFH: ten pixels
IKJL: twenty pixels

N: 10% more translucent
M: 10% less translucent

X: Exit


###Developer README
##Latest recompile - QT5.13.0/mingw73_32@20190810
##Requirements:
#QT + MinGW
##Instructions for MinGW:
#mkdir build
#cd build
#qmake ..
#mingw32-make

#Redistribution - 
##C:\Qt\5.13.0\mingw73_32\bin
###Qt5Core.dll
###Qt5Gui.dll
###Qt5Widgets.dll
###libgcc_s_dw2-1.dll
###libstdc++-6.dll
###libwinpthread-1.dll
##C:\Qt\5.13.0\mingw73_32\plugins
###qwindows.dll->platforms\qwindows.dll