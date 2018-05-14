#!/bin/bash

if [ -f "/tmp/miniscreen_deviceslist" ] 
then
    rm /tmp/miniscreen_deviceslist
fi
touch /tmp/miniscreen_deviceslist

for i in `xinput list | grep Mouse | cut -d"=" -f2 | cut -f1 -s`
do
    echo $i `xinput list-props $i | grep Enabled | cut -d"(" -f2 | cut -d")" -f1` >> /tmp/miniscreen_deviceslist
done
