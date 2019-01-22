# AM-TCPCliente
> Cliente TCP conexion con soporte TLS.

> TCP Client with TLS support

# Cliente TCP bastante simple de usar
# TCP Cliente very usefull and simple to use

# DLL

` Los metodos estan expuestos para poder usarlos universalmente, al estilo C++ `

` Exposed methods to use universally like C++ `

# write, read, writeHex, readHex
## Si la data que enviaras contiene algun caracter "0" (caracter terminador) puedes usar writeHex y/o readHex, pasa mucho en envio de tramas Base24/ISO8583. Envia la trama a enviar en Hexadecimal separado por ":".
## Data to send/read contains "0" character (end character) ? Base24/ISO8583 very common. Send data using hex format split with ":".

### Ejemplo/Example

> \0DISO0060000000800822000000000000004000000000000001227170253000001001\3

> 00:44:49:53:4F:30:30:36:30:30:30:30:30:30:30:38:30:30:38:32:32:30:30:30:30:30:30:30:30:30:30:30:30:30:30:34:30:30:30:30:30:30:30:30:30:30:30:30:30:30:31:32:32:37:31:37:30:32:35:33:30:30:30:30:30:31:30:30:31:03

