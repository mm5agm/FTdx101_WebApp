# How to Fix a Bug Reported by a User

## 1 — Save your current develop work
In Visual Studio **Git Changes** window, commit any uncommitted changes on `develop` before switching.

## 2 — Switch to main
```
git checkout main
```

## 3 — Fix the bug
Make the code change, then commit:
```
git add -A
git commit -m "Fix: description of the bug"
```

## 4 — Bump the version and push
Update `VERSION` in `installer.nsi` and `$Version` in `build-installer.ps1`, then:
```
git add installer.nsi build-installer.ps1
git commit -m "Bump version to x.x.x"
git push
```

## 5 — Create a release on GitHub
1. Go to https://github.com/mm5agm/FTdx101_WebApp/releases/new
2. Create tag `vx.x.x` and publish
3. The workflow builds and attaches `FTdx101_WebApp_Setup.exe` automatically

## 6 — Bring the fix back into develop
```
git checkout develop
git merge main
git push
```

You are now back on `develop` with the bug fix included.
