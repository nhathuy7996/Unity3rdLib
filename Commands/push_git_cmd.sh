#!/bin/sh

git add -A
git commit -m "release appname +..."
git push origin HEAD:production_hnn -f