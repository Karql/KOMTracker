This is my ansible configuration for CentOS host.

If you like to use it first you need to add `hosts` file with:

```
all:
  hosts:
    my_super_host1:
      ansible_host: x.x.x.x
    
  children:
    docker:
      hosts:
        my_super_host1:
```

Next on my_super_host1:

```
adduser ansible

mkdir /home/ansible/.ssh

echo "ssh-rsa your public key for super secrete private id_rsa" >> /home/ansible/.ssh/authorized_keys

chown -R ansible:ansible /home/ansible/.ssh

chmod 700 /home/ansible/.ssh

chmod 600 /home/ansible/.ssh/authorized_keys

echo "ansible ALL=(ALL) NOPASSWD: ALL" > /etc/sudoers.d/ansible
```

Next on ansible controller try to connect to accept certs:

```
superhero@localhost:~$ ssh ansible@my_super_host1
The authenticity of host 'my_super_host1 (x.x.x.x)' can't be established.
ECDSA key fingerprint is SHA256:lalalarotflolimo.
Are you sure you want to continue connecting (yes/no/[fingerprint])? yes
```

Do not forget to install ansible: https://docs.ansible.com/ansible/latest/installation_guide/intro_installation.html#

Install requirments: `ansible-galaxy install -r requirements.yml`

Finally run playbook: `ansible-playbook main.yml -i hosts --private-key ~/.ssh/id_rsa -u ansible`

For update packages: `ansible-playbook update.yml -i hosts --private-key ~/.ssh/id_rsa -u ansible`