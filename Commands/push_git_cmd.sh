#!/bin/sh

cd ../../
cd /Volumes/DVA/UNITY/DemoLib/Assets
git add -A
git commit -m "release_1.0"
git push origin HEAD:production_doNotCreateBranchFromHere -f