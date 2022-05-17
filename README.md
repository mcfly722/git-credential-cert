# git-credential-cert

Status: <b>in progress</b>


Protect git repository auth token with certificate from hardware token


#### Installation:

1. Create link to <b>git-credential-cert.exe</b>
```
fsutil hardlink create "C:\Program Files\Git\mingw64\libexec\git-core\git-credential-cert.exe" "C:\Users\Anonymous\go\src\github.com\mcfly722\git-credential-cert\bin\debug\git-credential-cert.exe"
```
2. Specify git-credential-cert as git credential helper
```
git config --global credential.helper cert
```

#### Uninstall
1. unset git-credential-cert
```
git config --global --unset credential.helper
```
2. delete link from \git-core folder
```
del "C:\Program Files\Git\mingw64\libexec\git-core\git-credential-cert.exe"
```
