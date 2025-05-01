

BackgroundColor = Color.new(255, 255, 255) -- white
FontColor = Color.new(0, 0, 0) -- black

Font.fmLoad()

while true do
	Screen.clear(BackgroundColor)
	Font.fmPrint(200, 200, 1, "Hello World!", FontColor)
	Font.fmPrint(20, 250, 0.6, "Please edit index.lua with the path to your code.", FontColor)
	Font.fmPrint(20, 270, 0.6, System.currentDirectory().."/index.lua", FontColor)

	Screen.flip()
	Screen.waitVblankStart()
end

