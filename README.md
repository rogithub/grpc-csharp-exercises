* SSL / TSL Certificates setup
[[https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-6.0][msdn docs]]


SSL_CERT_PWD="gordopechocho"

dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p <CREDENTIAL_PLACEHOLDER>
dotnet dev-certs https --trust


