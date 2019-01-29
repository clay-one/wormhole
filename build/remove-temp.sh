#!/bin/bash  
DIR_NAME=$1 
echo "Removing $DIR_NAME"
if [ -d "$DIR_NAME" ]; then
	rm -rf "$DIR_NAME"
else
	echo "It is not a directory"
fi
