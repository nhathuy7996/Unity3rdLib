#!/bin/sh

cd ../../
cd /Volumes/DVA/UNITY/DemoLib/Assets
cd $(git rev-parse --show-cdup)
git add -A
git commit -m "prepare update lib!!!!!!"
git subtree pull --prefix Assets/DVAH/Unity3rdLib https://github.com/nhathuy7996/Unity3rdLib.git develop --squash