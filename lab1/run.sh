#!/bin/bash
dotnet run --project ./Src/Src.csproj -- -m strategy_deadlock -c ./philosophers.conf -t 100 -i 100
