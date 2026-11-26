#!/usr/bin/env bash
set -Eeuo pipefail
umask 077

if [[ ${1:-} == --help ]]; then
  cat <<'USO'
Uso: respaldo.sh BASE DIRECTORIO
Requiere MySQL 8 y mysqldump. Usa --single-transaction y --routines.
Credenciales: archivo de opciones del cliente o MYSQL_DEFAULTS_FILE (ruta privada).
Opcionales: MYSQL_HOST, MYSQL_TCP_PORT, MYSQL_USER, MYSQLDUMP_BIN.
No recibe ni guarda contraseñas. Genera .sql, .meta y .sha256; conservar juntos.
La copia cubre la base SQL. Los archivos privados de evidencias se respaldan aparte.
Ejecutar sin cambios de esquema concurrentes; verificar con una restauración.
USO
  exit 0
fi
[[ $# == 2 ]] || { echo 'Uso: respaldo.sh BASE DIRECTORIO (ver --help)' >&2; exit 2; }
base=$1
[[ $base =~ ^[a-zA-Z0-9_]{1,64}$ ]] || { echo 'Nombre de base inválido.' >&2; exit 2; }
case ${base,,} in mysql|sys|information_schema|performance_schema) echo 'No se respaldan bases del sistema.' >&2; exit 2;; esac

opciones=()
if [[ -n ${MYSQL_DEFAULTS_FILE:-} ]]; then
  [[ -f $MYSQL_DEFAULTS_FILE ]] || { echo 'No existe el archivo privado de opciones.' >&2; exit 2; }
  opciones+=("--defaults-extra-file=$MYSQL_DEFAULTS_FILE")
fi
[[ -z ${MYSQL_HOST:-} ]] || opciones+=("--host=$MYSQL_HOST")
[[ -z ${MYSQL_TCP_PORT:-} ]] || opciones+=("--port=$MYSQL_TCP_PORT")
[[ -z ${MYSQL_USER:-} ]] || opciones+=("--user=$MYSQL_USER")
dump=${MYSQLDUMP_BIN:-mysqldump}
command -v "$dump" >/dev/null || { echo 'No se encuentra mysqldump.' >&2; exit 2; }
command -v sha256sum >/dev/null || { echo 'No se encuentra sha256sum.' >&2; exit 2; }
mkdir -p -- "$2"
directorio=$(cd -- "$2" && pwd -P)
temporal=$(mktemp "$directorio/sgcp_${base}_$(date -u +%Y%m%dT%H%M%SZ)_XXXXXX.part")
trap 'rm -f -- "$temporal"' EXIT
salida=${temporal%.part}.sql
# Sin --databases: no se emite CREATE DATABASE/USE de la base original.
"$dump" "${opciones[@]}" --single-transaction --routines --triggers --hex-blob \
  --no-tablespaces --set-gtid-purged=OFF --column-statistics=0 --skip-add-drop-table "$base" > "$temporal"
[[ -s $temporal ]] || { echo 'El respaldo está vacío.' >&2; exit 1; }
mv -- "$temporal" "$salida"
printf '%s\n' "$base" > "$salida.meta"
sha256sum < "$salida" | cut -d ' ' -f 1 > "$salida.sha256"
printf 'Respaldo creado: %s\n' "$salida"
