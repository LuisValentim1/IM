#!/bin/bash

msbuild speechModality/speechModality.sln /p:configuration=Debug
speechModality/bin/Debug/speechModality.exe

msbuild AppGui/AppGui.sln /p:configuration=Debug
AppGui/bin/Debug/AppGui.exe
