@echo off
if not exist bin\Debug\net6.0\Nicegang.dll (
    dotnet build
)
dotnet run --no-build
