#!/bin/bash
dotnet clean
dotnet restore
dotnet build
dotnet run --project Philosophers.Core


