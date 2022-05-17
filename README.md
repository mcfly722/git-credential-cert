# git-credential-cert

Status: in progress


Protect git repository auth token with certificate from hardware token


Create link to git-credential-cert.exe
```
fsutil hardlink create "C:\Program Files\Git\mingw64\libexec\git-core\git-credential-cert.exe" "C:\Users\Anonymous\go\src\github.com\mcfly722\git-credential-cert\bin\debug\git-credential-cert.exe"
```

Specify git-credential-cert as git credential helper
```
git config --global credential.helper cert
```
