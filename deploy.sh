#!/bin/bash
# stty -echo

DIR=$PWD
SOURCEDIR="./"
BINARYDIR="./build_output"
BINARYDIRENDED=$DIR"/build_output/"
DEPLOYDIR="./ReleaseBinaries"

if [ ! -d $DEPLOYDIR ]; then
{
	mkdir $BINARYDIR
}
else
{
	rm -rf $BINARYDIR/*
}
fi

xbuild $SOURCEDIR/OpenEngine.sln /property:OutDir=$BINARYDIRENDED;Configuration=Release

if [ ! -d $DEPLOYDIR ]; then
{
	mkdir $DEPLOYDIR
}
else
{
	rm -rf $DEPLOYDIR/*
}
fi

mkdir $DEPLOYDIR/Installer

cp $BINARYDIR/OpenEngine.Core.dll $DEPLOYDIR/OpenEngine.Core.dll
cp $BINARYDIR/OpenEngine.Console.exe $DEPLOYDIR/OpenEngine.Console.exe
cp $BINARYDIR/OpenEngine.Service.exe $DEPLOYDIR/OpenEngine.Service.exe
cp $BINARYDIR/style.css $DEPLOYDIR/style.css
cp $BINARYDIR/Configuration/* $DEPLOYDIR/
