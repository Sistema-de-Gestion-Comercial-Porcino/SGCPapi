#!/usr/bin/env bash
# Prueba real aislada: jamás recibe nombres de bases del usuario.
set -Eeuo pipefail
raiz=$(cd -- "$(dirname -- "$0")/../.." && pwd -P)
mysql=${MYSQL_BIN:-mysql}
opciones=()
[[ -z ${MYSQL_DEFAULTS_FILE:-} ]] || opciones+=("--defaults-extra-file=$MYSQL_DEFAULTS_FILE")
[[ -z ${MYSQL_HOST:-} ]] || opciones+=("--host=$MYSQL_HOST")
[[ -z ${MYSQL_TCP_PORT:-} ]] || opciones+=("--port=$MYSQL_TCP_PORT")
[[ -z ${MYSQL_USER:-} ]] || opciones+=("--user=$MYSQL_USER")
marca=$(date -u +%Y%m%d%H%M%S)_${RANDOM}_${RANDOM}
origen=sgcp_pruebas_respaldo_$marca
destino=sgcp_pruebas_restaurado_$marca
trabajo=$(mktemp -d)
sql() { "$mysql" "${opciones[@]}" --batch --skip-column-names --execute="$1"; }
limpiar() {
  # Solo los dos nombres propios e impredecibles de ESTA ejecución.
  sql "DROP DATABASE IF EXISTS \`$origen\`; DROP DATABASE IF EXISTS \`$destino\`;" >/dev/null
  rm -f -- "$trabajo"/*.sql "$trabajo"/*.meta "$trabajo"/*.sha256
  rmdir -- "$trabajo"
}
trap limpiar EXIT
sql "CREATE DATABASE \`$origen\` CHARACTER SET utf8mb4;
CREATE TABLE \`$origen\`.Clientes (Id INT PRIMARY KEY, Nombre VARCHAR(100));
CREATE TABLE \`$origen\`.Pedidos (Id INT PRIMARY KEY, ClienteId INT, Codigo VARCHAR(40), Total DECIMAL(10,2), FOREIGN KEY (ClienteId) REFERENCES Clientes(Id));
INSERT INTO \`$origen\`.Clientes VALUES (1, 'Cliente sintético de respaldo');
INSERT INTO \`$origen\`.Pedidos VALUES (1,1,'PED-PRUEBA-1',12.50),(2,1,'PED-PRUEBA-2',35.75);
CREATE PROCEDURE \`$origen\`.ComprobarPedidos() SELECT COUNT(*) FROM Pedidos;"
bash "$raiz/scripts/respaldo.sh" "$origen" "$trabajo"
archivo=$(find "$trabajo" -maxdepth 1 -name '*.sql' -print -quit)
antes=$(sql "SELECT COUNT(*) FROM \`$origen\`.Pedidos")
bash "$raiz/scripts/restaurar.sh" "$archivo" "$destino"
despues=$(sql "SELECT COUNT(*) FROM \`$destino\`.Pedidos")
[[ $antes == 2 && $antes == "$despues" ]] || { echo 'Conteo de pedidos incorrecto.' >&2; exit 1; }
[[ $(sql "SELECT COUNT(*) FROM \`$destino\`.Pedidos p JOIN \`$destino\`.Clientes c ON p.ClienteId=c.Id WHERE p.Total IN (12.50,35.75)") == 2 ]]
[[ $(sql "CALL \`$destino\`.ComprobarPedidos()") == 2 ]]
echo "OK respaldo real: pedidos antes=$antes, después=$despues; relaciones, decimales y rutina conservados."
if bash "$raiz/scripts/restaurar.sh" "$archivo" "$origen"; then echo 'ERROR: permitió restaurar al origen.' >&2; exit 1; fi
if bash "$raiz/scripts/restaurar.sh" "$archivo" "$destino"; then echo 'ERROR: permitió sobrescribir destino.' >&2; exit 1; fi
printf '\n-- alterado\n' >> "$archivo"
if bash "$raiz/scripts/restaurar.sh" "$archivo" "sgcp_pruebas_invalido_$marca"; then echo 'ERROR: aceptó un respaldo alterado.' >&2; exit 1; fi
[[ $(sql "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='sgcp_pruebas_invalido_$marca'") == 0 ]]
[[ $(sql "SELECT COUNT(*) FROM \`$origen\`.Pedidos") == 2 && $(sql "SELECT COUNT(*) FROM \`$destino\`.Pedidos") == 2 ]]
echo 'OK protecciones: origen intacto, destino existente intacto y copia alterada rechazada antes de crear una base.'
