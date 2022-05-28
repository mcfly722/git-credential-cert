# git-credential-cert
Protect git repository auth token with certificate from hardware token

Status: <b>working alpha ✔</b><br><br>
![](https://github.com/mcfly722/git-credential-cert/blob/main/doc/howTo.gif)

------
#### Requirements
1. installed git
2. certificate with private key on hardware token
3. .NET Framework 4.7.1 or higher
4. Visual Studio 2013 to build binary from sources
------
#### Installation
1. Copy <b>git-credential-cert.exe</b> to <b>C:\Program Files\Git\mingw64\libexec\git-core </b> folder<br>
2. Specify <b>git-credential-cert</b> as git credential helper:
```
git config --global credential.helper cert
```
3. Don't forget to delete your credentials from Windows Credential Manager and all other places where it is not in safe<br>
------
#### Configuring for Visual Studio
For <b>Visual Studio</b> it is required to specify this helper in <b>%USERPROFILE%\\.gitconfig</b> file<br><br>
You can add it like this:<br>
```
[credential]
	helper=C:\\\\Program\\ Files\\\\Git\\\\mingw64\\\\libexec\\\\git-core\\\\git-credential-cert.exe
```

------
#### Generate certificate
Generate certificate with private key for token encryption and signing credentials data in store:
```
makecert.exe -pe -r -a sha1 -len 2048 -n "CN=<specify here your own cert name>" -eku "1.3.6.1.5.5.7.3.4" -sky exchange -sv cert.pvk cert.cer
```
```
pvk2pfx.exe -pvk cert.pvk -spc cert.cer -pfx cert.pfx
```
Import certificate to hardware token using SafeNet Authentication Client or any other third-party software.<br>
During import, choose pfx file and enter empty password for importing. Do not forget to delete private key from your disk.<br>

------
#### Adding new credentials
```
git push
```
Git helper checks your database (<b>%USERPROFILE%\\git-credential-cert</b> file), and if there are no existing credentials for current URL, it will ask for the username and password to store it.

------
#### Listing of existing credentials
```
git-credential-cert.exe list
```
------
#### Deleting existing credentials
```
git-credential-cert.exe erase <URL>
```
------
#### Uninstall

💡ToDo

------
#### List Of Tests

###### 1. Common
1.1. 💡 Incorrect method<br>
1.2. 💡 Input has no key=value format<br>
1.3. 💡 Input has unknown key<br>
1.4. 💡 Store file corrupted<br>
1.5. 💡 Writing store file exception<br>

###### 2. Add new credentials (STORE):
2.1. ✔ Create new store, file does not exist<br>
2.2. 💡 No certs with private key<br>
2.3. 💡 Fail to encrypt (token ejected)<br>
2.4. 💡 There are no required parameters<br>
2.5. ❌ Change token during cert select (should update certs list)<br>

###### 3. Read existing credentials (GET):
3.1. 💡 There are no required parameters<br>
3.2. 💡 Store has no credential for required url<br>
3.3. ✔ Cert for signature and decryption not found<br>
3.4. 💡 Cert for signature check and decryption exist, but have no private key<br>
3.5. 💡 Trying to add already existing url<br>
3.6. 💡 Incorrect signature<br>

###### 4. Remove existing credentials (ERASE):
4.1. 💡 Trying to remove not existing credentials<br>

###### 5. Other
5.1. ✔ Working from Visual Studio Code<br>
5.2. ✔ Working from Visual Studio
