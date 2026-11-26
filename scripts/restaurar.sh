#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

if [[ ${1:-} == --help ]]; then
  cat <<'USO'
Uso: restaurar.sh RESPALDO.sql BASE_NUEVA
Requiere los archivos .meta y .sha256 generados por respaldo.sh.
Solo importa respaldos de confianza en una base NUEVA. Rechaza una base existente
y el nombre original. Nunca borra una base, tampoco si la importación falla.
Usa un servidor de comprobación separado cuando sea posible.
Credenciales: archivo de opciones del cliente o MYSQL_DEFAULTS_FILE (ruta privada).
Opcionales: MYSQL_HOST, MYSQL_TCP_PORT, MYSQL_USER, MYSQL_BIN.
Tras restaurar, comprobar conteos y relaciones antes de usar la copia.
USO
  exit 0
fi
[[ $# == 2 ]] || { echo 'Uso: restaurar.sh RESPALDO.sql BASE_NUEVA (ver --help)' >&2; exit 2; }
archivo=$1
destino=$2
[[ $destino =~ ^[a-zA-Z0-9_]{1,64}$ ]] || { echo 'Nombre de base inválido.' >&2; exit 2; }
case ${destino,,} in mysql|sys|information_schema|performance_schema) echo 'Destino de sistema prohibido.' >&2; exit 2;; esac
[[ -s $archivo && -f $archivo.meta && -f $archivo.sha256 ]] || { echo 'Falta el respaldo, su origen o su suma de comprobación.' >&2; exit 2; }
origen=$(cat -- "$archivo.meta")
[[ $origen =~ ^[a-zA-Z0-9_]{1,64}$ && ${destino,,} != "${origen,,}" ]] || { echo 'El destino debe ser distinto de la base original.' >&2; exit 2; }
esperado=$(cat -- "$archivo.sha256")
[[ $esperado =~ ^[a-f0-9]{64}$ && $(sha256sum < "$archivo" | cut -d ' ' -f 1) == "$esperado" ]] || { echo 'El respaldo no supera la comprobación SHA-256.' >&2; exit 2; }

opciones=()
if [[ -n ${MYSQL_DEFAULTS_FILE:-} ]]; then
  [[ -f $MYSQL_DEFAULTS_FILE ]] || { echo 'No existe el archivo privado de opciones.' >&2; exit 2; }
  opciones+=("--defaults-extra-file=$MYSQL_DEFAULTS_FILE")
fi
[[ -z ${MYSQL_HOST:-} ]] || opciones+=("--host=$MYSQL_HOST")
[[ -z ${MYSQL_TCP_PORT:-} ]] || opciones+=("--port=$MYSQL_TCP_PORT")
[[ -z ${MYSQL_USER:-} ]] || opciones+=("--user=$MYSQL_USER")
mysql=${MYSQL_BIN:-mysql}
command -v "$mysql" >/dev/null || { echo 'No se encuentra mysql.' >&2; exit 2; }
existe=$("$mysql" "${opciones[@]}" --batch --skip-column-names --execute="SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '$destino'")
[[ $existe == 0 ]] || { echo 'La base de destino ya existe. No se ha modificado.' >&2; exit 2; }
# CREATE sin IF NOT EXISTS protege también frente a una creación simultánea.
"$mysql" "${opciones[@]}" --execute="CREATE DATABASE \`$destino\` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci"
if ! "$mysql" "${opciones[@]}" --database="$destino" < "$archivo"; then
  echo "La importación falló. Se conserva la base nueva $destino para revisar; no se ha borrado ninguna base." >&2
  exit 1
fi
printf 'Restauración terminada en base nueva: %s\n' "$destino"
