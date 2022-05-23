# git-credential-cert
Protect git repository auth token with certificate from hardware token

Status: <b>working alpha ✔</b>

#### Requirements:
1. installed git
2. certificate with private key in MY or from hardware token
3. .NET Framework 4.7.1 or higher
4. Visual Studio 2013 to build binary from sources
------
#### Installation:
1. Copy <b>git-credential-cert.exe</b> to <b>C:\Program Files\Git\mingw64\libexec\git-core </b> folder<br>
2. Specify git-credential-cert as git credential helper:
```
git config --global credential.helper cert
```

------
#### Generate certificate
Generate certificate with private key for password encryption and signing data in store:
```
makecert.exe -pe -r -a sha1 -len 2048 -n "CN=<specify here your own cert name>" -eku "1.3.6.1.5.5.7.3.4" -sky exchange -sv cert.pvk cert.cer
```
```
pvk2pfx.exe -pvk cert.pvk -spc cert.cer -pfx cert.pfx
```
Import certificate to hardware token using SafeNet Authentication Client or any other third-party software.<br>
During import, choose pfx file and enter empty password.

------
#### Uninstall:

💡ToDo

------
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
2.5. 💡 Change token during cert select (should update certs list)<br>

###### 3. Read existing credentials (GET):
3.1. 💡 There are no required parameters<br>
3.2. 💡 Store has no credential for required url<br>
3.3. 💡 Cert for signature check and decryption does not exist<br>
3.4. 💡 Cert for signature check and decryption exist, but have no private key<br>
3.5. 💡 Trying to add already existing url<br>
3.6. 💡 Incorrect signature<br>

###### 4. Remove existing credentials (ERASE):
4.1. 💡 Trying to remove not existing credentials<br>
###### 5. Other
5.1. 💡 Working from Visual Studio Code
5.2. 💡 Working from Visual Studio

test123
