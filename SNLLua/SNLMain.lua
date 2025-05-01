

-- Screen.setMode(NTSC, 640, 448, CT24, INTERLACED, FIELD)
-- Screen.setMode(_480p, 640, 448, CT24, NONINTERLACED, FRAME)

dofile("SNLFunctions.lua");

HighlightColor = Color.new(0, 69, 0) -- dark green
BackgroundColor = Color.new(255, 255, 255) -- white
FontColor = Color.new(0, 0, 0) -- black

Font.ftInit()
MainFont = Font.ftLoad("Gudea-Bold.ttf")
SmallFont = Font.ftLoad("Gudea-Bold.ttf")
Font.ftSetPixelSize(MainFont, 16, 16)
Font.ftSetPixelSize(SmallFont, 12, 12)

SelectedIndex = 1
ScrollIndex = 0
InputWait = 0
InputHeld = 0
InputExit = 0
AnalogHeld = false
NumCurrentList = 19
NeutrinoArgs = ""
ShowNeutrinoArgs = false

LoadList()

while true do
	Screen.clear(BackgroundColor)

	if ShowNeutrinoArgs then
		Font.ftPrint(SmallFont, 20, 15, 0, 0, 0, NeutrinoArgs, HighlightColor)
	else
		Font.ftPrint(SmallFont, 402, 15, 0, 0, 0, "Simple Neutrino Loader v1.0.0.0", FontColor)
	end

	ReadInput()

	DrawList()

	Screen.flip()
	Screen.waitVblankStart()
end

