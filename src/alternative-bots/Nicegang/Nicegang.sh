#!/usr/bin/env bash
if [ ! -f bin/Debug/net6.0/Nicegang.dll ]; then
    dotnet build
fi
dotnet run --no-build
