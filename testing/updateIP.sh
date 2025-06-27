#!/bin/bash

if [ $# -eq 0 ]; then
    read -p "Enter new websocket URL: " new_websocket
else
    new_websocket="$1"
fi

sed -i "s|\"websocket\": \".*\"|\"websocket\": \"$new_websocket\"|" setup.json