

function Debounce(pad)
	if pad ~= PrevInput or not AnalogHeld then
		DebounceActive = false
		InputWait = 0
		InputHeld = 0
		return true
	end
	if DebounceActive and InputHeld == 0 then
		DebounceActive = false
		InputHeld = 30
		return false
	elseif InputHeld > 1 then
		InputHeld = InputHeld - 1
		return false
	elseif InputWait > 1 then
		InputWait = InputWait - 1
		return false
	else
		InputWait = 3
		return true
	end
end

function Scroll(i, waitDebounce)
	IdleCounter = 0
	ShowNeutrinoArgs = false
	if waitDebounce then
		DebounceActive = true
		if SelectedIndex + i < 1 then
			if ScrollIndex > 0 then
				ScrollIndex = ScrollIndex - 1
			elseif TotalGames ~= NumCurrentList then
				ScrollIndex = TotalGames - NumCurrentList
				SelectedIndex = NumCurrentList
			else
				SelectedIndex = NumCurrentList
			end
		elseif SelectedIndex + i > NumCurrentList then
			if SelectedIndex + ScrollIndex + i > TotalGames then
				ScrollIndex = 0
				SelectedIndex = 1
			else
				ScrollIndex = ScrollIndex + 1
			end
		else
			SelectedIndex = SelectedIndex + i
		end
	end
end

function StartGame(gameIndex)
	if gameIndex == TotalGames then
		LaunchELF()
	end
	local neutrinoArgs = {}
	local columIndex = 1
	for colum in GameList[gameIndex]:gmatch("[^|]+") do
		if columIndex > 2 then
			neutrinoArgs[columIndex - 2] = colum
		end
		columIndex = columIndex + 1
	end
	System.loadELF(System.currentDirectory().."/neutrino.elf", 0, table.unpack(neutrinoArgs))
end

function PrintGameArgs(gameIndex)
	local neutrinoArgs = {}
	local columIndex = 1
	for colum in GameList[gameIndex]:gmatch("[^|]+") do
		if columIndex > 2 then
			neutrinoArgs[columIndex - 2] = colum
		end
		columIndex = columIndex + 1
	end
	NeutrinoArgs = table.concat(neutrinoArgs, " ")
end

function ReadInput()
	local pad = Pads.get()
	local lx, ly = Pads.getLeftStick()
	local waitDebounce = Debounce(pad)

	if Pads.check(pad, PAD_CROSS) or Pads.check(pad, PAD_CIRCLE) or Pads.check(pad, PAD_START) then
		StartGame(ScrollIndex + SelectedIndex)

	elseif Pads.check(pad, PAD_SQUARE) or Pads.check(pad, PAD_TRIANGLE) then
		PrintGameArgs(ScrollIndex + SelectedIndex)
		ShowNeutrinoArgs = true

	elseif Pads.check(pad, PAD_SELECT) then
		InputExit = InputExit + 1
		if InputExit > 15 then
			System.exitToBrowser()
		end
	elseif ly < -90 or Pads.check(pad, PAD_UP) then
		Scroll(-1, waitDebounce)
		AnalogHeld = true
	elseif ly > 90 or Pads.check(pad, PAD_DOWN) then
		Scroll(1, waitDebounce)
		AnalogHeld = true
	else
		AnalogHeld = false
		InputExit = 0
		IdleCounter = IdleCounter + 1
		if IdleCounter > 18000 then -- wait 5 minutes
			ScreenSaver = true
		end
	end
	PrevInput = pad
end

function ReadAnyInput()
	local pad = Pads.get()
	local lx, ly = Pads.getLeftStick()
	local rx, ry = Pads.getRightStick()
	if Pads.check(pad, PAD_SELECT) or Pads.check(pad, PAD_START) or Pads.check(pad, PAD_UP) or
	Pads.check(pad, PAD_RIGHT) or Pads.check(pad, PAD_DOWN) or Pads.check(pad, PAD_LEFT) or
	Pads.check(pad, PAD_TRIANGLE) or Pads.check(pad, PAD_CIRCLE) or Pads.check(pad, PAD_CROSS) or
	Pads.check(pad, PAD_SQUARE) or Pads.check(pad, PAD_L1) or Pads.check(pad, PAD_R1) or
	Pads.check(pad, PAD_L2) or Pads.check(pad, PAD_R2) or Pads.check(pad, PAD_L3) or
	Pads.check(pad, PAD_R3) then
		return true
	end
	if lx * lx > 3600 or ly * ly > 3600 or rx * rx > 3600 or ry * ry > 3600 then
		return true
	end
	return false
end

function LoadList()
	TotalGames = 0
	GameList = {}
	ListTypes = {"HDD", "HDL", "ILINK", "MMCE", "MX4", "UDPBD", "USB"}
	for i = 1, #ListTypes do
		local file = io.open(ListTypes[i].."List.txt")
		if file then
			for tempLine in file:lines() do
				if not IsNullOrEmpty(tempLine) then
					TotalGames = TotalGames + 1
					GameList[TotalGames] = tempLine:gsub("\r", "")
				end
			end
			file:close()
			table.sort(GameList)
		end
	end
	TotalGames = TotalGames + 1
	GameList[TotalGames] = "Exit to LaunchELF"
	if TotalGames < 19 then
		NumCurrentList = TotalGames
	end
end

function GameInfo(i)
	local gameName = "ERROR NOT FOUND"
	local gameId = ""
	local lineIndex = 1
	for item in GameList[i]:gmatch("[^|]+") do
		if lineIndex == 1 then
			gameName = item
		elseif lineIndex == 2 then
			gameId = item
		end
		lineIndex = lineIndex + 1
	end
	return gameName, gameId
end

function DrawList()
	if TotalGames > 19 then
		Font.ftPrint(MainFont, 250, 408, 0, 0, 0, "↓ view more ↓", FontColor)
	end
	for item = 1, NumCurrentList do
		local name, id = GameInfo(item + ScrollIndex)
		if SelectedIndex == item then
			if IdleCounter % 90 > 45 then
				Font.ftPrint(SelectFont, 35, item * 20 + 13, 0, 0, 0, "→ "..name, HighlightColor)
				Font.ftPrint(MainFont, 480, item * 20 + 10, 0, 0, 0, id.." ←", HighlightColor)	
			else
				Font.ftPrint(SelectFont, 29, item * 20 + 13, 0, 0, 0, "→  "..name, HighlightColor)
				Font.ftPrint(MainFont, 480, item * 20 + 10, 0, 0, 0, id.."  ←", HighlightColor)	
			end
		else
			Font.ftPrint(MainFont, 60, item * 20 + 10, 0, 0, 0, name, FontColor)
			Font.ftPrint(MainFont, 480, item * 20 + 10, 0, 0, 0, id, FontColor)
		end
	end
end

function LaunchELF()
	if doesFileExist("mc0:/BOOT/ULE.ELF") then
		System.loadELF("mc0:/BOOT/ULE.ELF", 0)
	elseif doesFileExist("mc1:/BOOT/ULE.ELF") then
		System.loadELF("mc1:/BOOT/ULE.ELF", 0)
	elseif doesFileExist("mc0:/APPS/ULE.ELF") then
		System.loadELF("mc0:/APPS/ULE.ELF", 0)
	elseif doesFileExist("mc1:/APPS/ULE.ELF") then
		System.loadELF("mc1:/APPS/ULE.ELF", 0)
	elseif doesFileExist("mc0:/BOOT/BOOT.ELF") then
		System.loadELF("mc0:/BOOT/BOOT.ELF", 0)
	elseif doesFileExist("mc1:/BOOT/BOOT.ELF") then
		System.loadELF("mc1:/BOOT/BOOT.ELF", 0)
	else
		System.exitToBrowser()
	end
end

function DrawScreenSaver()
	Screen.clear(FontColor)
	ScreenSaverX = ScreenSaverX + VelX
	ScreenSaverY = ScreenSaverY + VelY
	if ScreenSaverX > 530 or ScreenSaverX < 5 then
		VelX = -VelX
	end
	if ScreenSaverY > 428 or ScreenSaverY < 20 then
		VelY = -VelY
	end
	Graphics.drawRect(ScreenSaverX - 5, ScreenSaverY - 20, 115, 40, HighlightColor)
	Font.ftPrint(GiantFont, ScreenSaverX, ScreenSaverY, 0, 0, 0, "SNL", BackgroundColor)
	Screen.flip()
	Screen.waitVblankStart()
end

function DrawSine()
	DrawOffset = DrawOffset + 1
	if DrawOffset > 1280 then
		DrawOffset = 0
	end
	for i = 0, 640, 2 do
		local xPos = (640 - i)
		local yPos = math.sin((i + DrawOffset) / 204) * (i + 1280) / 16 + 224
		Graphics.drawRect(xPos, yPos - 90, 2, 180, SineOutline)
		Graphics.drawRect(xPos, yPos - 80, 2, 160, SineColor)
	end
end

function IsNullOrEmpty(str)
    return str == nil or str == ''
end
