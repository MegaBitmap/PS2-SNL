

local startFile = "SNLMain.lua"
local startFolder = "SimpleNeutrinoLoader"
local startPath = startFolder.."/"..startFile

if doesFileExist(System.currentDirectory():match(".*/")..startPath) then
	System.currentDirectory(System.currentDirectory():match(".*/")..startFolder)
	dofile(startFile)
elseif doesFileExist("mc0:/"..startPath) then
	System.currentDirectory("mc0:/"..startFolder)
	dofile(startFile)
elseif doesFileExist("mc1:/"..startPath) then
	System.currentDirectory("mc1:/"..startFolder)
	dofile(startFile)
elseif doesFileExist("mass:/"..startPath) then
	System.currentDirectory("mass:/"..startFolder)
	dofile(startFile)
end

dofile("helloworld.lua")

