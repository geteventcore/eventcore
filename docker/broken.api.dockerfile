FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet publish src/Threadsy.EventStore.Api/ -c Release -o bin/publish

FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=build-env /app/src/Threadsy.EventStore.Api/bin/publish .

# SSH Server support, only available via Azure SCM.
RUN apk add --update openssh \
	&& rm -rf /tmp/* /var/cache/apk/* \
	&& echo "root:Docker!" | chpasswd
RUN rm -rf /etc/ssh/ssh_host_rsa_key /etc/ssh/ssh_host_dsa_key
COPY /docker/azure-sshdconfig /etc/ssh/sshd_config

COPY /docker/api.entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 80 2222

ENTRYPOINT ["sh", "/usr/local/bin/docker-entrypoint.sh"]