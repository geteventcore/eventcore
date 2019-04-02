#!/bin/bash
/usr/bin/ssh-keygen -A
/usr/sbin/sshd -D &
dotnet Threadsy.EventStore.Api.dll