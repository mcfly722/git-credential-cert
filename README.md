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
<br><br>
#### Tests List: ❌✔

###### 1. Common
1.1. 💡 Incorrect method<br>
1.2. 💡 Input has no key=value format<br>
1.3. 💡 Input has unknown key<br>
1.4. 💡 Store file corrupted<br>
1.5. 💡 Writing store file exception<br>

###### 2. Add new credentials (STORE):
2.1. 💡 Create new store, file does not exist<br>
2.2. 💡 No certs with private key<br>
2.3. 💡 Fail to encrypt (token ejected)<br>
2.4. 💡 There are no required parameters<br>

###### 3. Read existing credentials (GET):
3.1. 💡 Store has no credential for required url<br>
3.2. 💡 Trying to add already existing url<br>
3.3. 💡 Incorrect signature<br>

###### 4. Remove existing credentials (ERASE):
4.1. 💡 Unknown unsupported command<br>
4.2. 💡 Trying to remove not existing credentials<br>
