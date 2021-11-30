Create Root CA key:
```
openssl genrsa -des3 -out KomTrackerRootCA.key 4096
```

Create Root CA cert:
```
$ openssl req -x509 -new -nodes -key KomTrackerRootCA.key -sha256 -days 10240 -out KomTrackerRootCA.crt
Enter pass phrase for KomTrackerRootCA.key:
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [AU]:PL
State or Province Name (full name) [Some-State]:Lesser Poland
Locality Name (eg, city) []:Cracow
Organization Name (eg, company) [Internet Widgits Pty Ltd]:KOM Tracker
Organizational Unit Name (eg, section) []:
Common Name (e.g. server FQDN or YOUR name) []:KOM Tracker ROOT CA
Email Address []:karql.pl@gmail.com
```