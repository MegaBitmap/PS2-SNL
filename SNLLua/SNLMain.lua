

-- Screen.setMode(NTSC, 640, 448, CT24, INTERLACED, FIELD)
-- Screen.setMode(_480p, 640, 448, CT24, NONINTERLACED, FRAME)

dofile("SNLFunctions.lua");

HighlightColor = Color.new(0, 69, 0) -- dark green
BackgroundColor = Color.new(255, 255, 255) -- white
FontColor = Color.new(0, 0, 0) -- black
SineColor = Color.new(200, 210, 255) -- light blue
SineOutline = Color.new(220, 230, 255) -- lighter blue

Font.ftInit()
MainFont = Font.ftLoad("Gudea-Bold.ttf")
SmallFont = Font.ftLoad("Gudea-Bold.ttf")
SelectFont = Font.ftLoad("Gudea-Bold.ttf")
GiantFont = Font.ftLoad("Gudea-Bold.ttf")
Font.ftSetPixelSize(MainFont, 16, 16)
Font.ftSetPixelSize(SmallFont, 12, 12)
Font.ftSetPixelSize(SelectFont, 24, 24)
Font.ftSetPixelSize(GiantFont, 60, 40)

SelectedIndex = 1
ScrollIndex = 0
InputWait = 0
InputHeld = 0
InputExit = 0
AnalogHeld = false
NumCurrentList = 19
NeutrinoArgs = ""
ShowNeutrinoArgs = false
ScreenSaver = false
IdleCounter = 0
WaitSleep = 0
ScreenSaverX = 50
ScreenSaverY = 50
VelX = 3
VelY = 1
DrawOffset = 0

LoadList()

while true do
	Screen.clear(BackgroundColor)
	DrawSine()

	if ShowNeutrinoArgs then
		Font.ftPrint(SmallFont, 20, 15, 0, 0, 0, NeutrinoArgs, HighlightColor)
	else
		Font.ftPrint(SmallFont, 402, 15, 0, 0, 0, "Simple Neutrino Loader v1.0.0.0", FontColor)
	end
	if WaitSleep < 1 then
		ReadInput()
	else
		WaitSleep = WaitSleep - 1
	end
	DrawList()

	while ScreenSaver do
		if ReadAnyInput() then
			ScreenSaver = false
			IdleCounter = 0
			WaitSleep = 60
		end
		DrawScreenSaver()
	end
	-- Font.ftPrint(SmallFont, 20, 15, 0, 0, 0, Screen.getFPS(80).." FPS", FontColor)
	Screen.flip()
	Screen.waitVblankStart()
end

