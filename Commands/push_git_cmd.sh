#!/bin/sh

cd ../../
cd /Volumes/DVA/UNITY/Lib/Assets
cd $(git rev-parse --show-cdup)
git add -A
