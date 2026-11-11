#!/usr/bin/env python3
"""Recorrido de aceptacion del Sprint 1 contra una API ya levantada.

Comprueba por HTTP el criterio de validacion escrito en el Excel de
planificacion y detallado en la seccion 5 de
SGCPdocumentacion/00_Definicion/Como_ejecutar_el_sistema.md:

  - el pedido se niega si la cotizacion no esta aceptada
  - el pedido confirmado guarda costo Bs 112,00 y margen Bs 28,00
  - una vigencia de precio posterior no recalcula ese margen

No reemplaza a tests/SGCP.Verificacion, que aplica las migraciones en una base
aislada y cubre los cuatro sprints. Esto es la comprobacion corta, para usar
antes de una demostracion sobre la base que se va a mostrar.

Uso:
    python3 scripts/Verificar-Sprint1.py

Configuracion por variables de entorno:
    SGCP_CLAVE_ADMIN   obligatoria. La misma de DatosIniciales:ClaveAdministrador
    SGCP_API           opcional. Por omision http://127.0.0.1:5081/api/v1
    SGCP_USUARIO       opcional. Por omision admin

Requiere Python 3.8 o posterior. No usa paquetes externos.

Deja en la base los registros que crea, con codigos marcados con la hora para
no chocar con los indices unicos. Para dejar la base limpia, ver la seccion 8
de SGCPdocumentacion/00_Definicion/Levantar_con_Docker.md.
"""
import json
import os
import sys
import urllib.error
import urllib.request
from datetime import datetime, timedelta, timezone

BASE = os.environ.get("SGCP_API", "http://127.0.0.1:5081/api/v1").rstrip("/")
USUARIO = os.environ.get("SGCP_USUARIO", "admin")
CLAVE = os.environ.get("SGCP_CLAVE_ADMIN")

if not CLAVE:
    print("Falta SGCP_CLAVE_ADMIN: es la clave de DatosIniciales:ClaveAdministrador.")
    print()
    print("  Linux o macOS:   SGCP_CLAVE_ADMIN='tu-clave' python3 scripts/Verificar-Sprint1.py")
    print("  PowerShell:      $env:SGCP_CLAVE_ADMIN='tu-clave'; python scripts\\Verificar-Sprint1.py")
    sys.exit(2)

correctas = 0
fallas = []
token = None


def llamar(metodo, ruta, cuerpo=None):
    datos = json.dumps(cuerpo).encode() if cuerpo is not None else None
    pedido = urllib.request.Request(BASE + ruta, data=datos, method=metodo)
    pedido.add_header("Content-Type", "application/json")
    if token:
        pedido.add_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(pedido, timeout=30) as r:
            texto = r.read().decode()
            return r.status, (json.loads(texto) if texto.strip() else None)
    except urllib.error.HTTPError as e:
        texto = e.read().decode()
        try:
            return e.code, json.loads(texto)
        except json.JSONDecodeError:
            return e.code, texto
    except urllib.error.URLError as e:
        print(f"No se pudo conectar a {BASE}: {e.reason}")
        print("Levanta la API primero. Ver la seccion 5 de Levantar_con_Docker.md.")
        sys.exit(2)


def paso(numero, descripcion, condicion, detalle=""):
    global correctas
    marca = "OK   " if condicion else "FALLA"
    if condicion:
        correctas += 1
    else:
        fallas.append(f"{numero}. {descripcion} [{detalle}]")
    print(f"  {marca} {numero}. {descripcion}" + (f"  [{detalle}]" if detalle else ""))


ahora = datetime.now(timezone.utc)
sello = ahora.strftime("%H%M%S")
fecha = lambda dias: (ahora + timedelta(days=dias)).strftime("%Y-%m-%dT%H:%M:%SZ")

print()
print("RECORRIDO DE ACEPTACION DEL SPRINT 1")
print(f"API: {BASE}")
print("=" * 72)

estado, r = llamar("POST", "/auth/login", {"login": USUARIO, "clave": CLAVE})
token = (r or {}).get("token") if isinstance(r, dict) else None
paso(1, f"Entrar a la API como {USUARIO}", estado == 200 and bool(token), f"HTTP {estado}")
if not token:
    print()
    print("Sin token no se puede seguir. Revisa SGCP_CLAVE_ADMIN y SGCP_USUARIO.")
    sys.exit(1)

estado, clasificacion = llamar("POST", "/clasificaciones", {
    "codigo": f"COST{sello}", "nombre": "Costilla",
    "familia": "CARNE_FRESCA", "descripcion": "Carne fresca"})
clasificacion_id = (clasificacion or {}).get("id")
paso(2, "Crear clasificacion Costilla, familia Carne fresca",
     estado in (200, 201) and bool(clasificacion_id), f"HTTP {estado}")

estado, producto = llamar("POST", "/productos", {
    "codigo": f"CC{sello}", "nombre": "Costilla de cerdo",
    "presentacion": "Pieza", "unidadVenta": "kg",
    "descripcion": "Costilla de cerdo fresca",
    "clasificacionProductoId": clasificacion_id})
producto_id = (producto or {}).get("id")
paso(3, "Crear producto Costilla de cerdo, unidad kg",
     estado in (200, 201) and bool(producto_id), f"HTTP {estado}")

estado, _ = llamar("POST", f"/productos/{producto_id}/precios", {
    "costoReferencia": 28.00, "precioVenta": 35.00, "vigenteDesde": fecha(-1)})
paso(4, "Registrar costo Bs 28,00 y precio Bs 35,00", estado in (200, 201), f"HTTP {estado}")

estado, cliente = llamar("POST", "/clientes", {
    "nombre": f"Cliente de prueba {sello}", "tipo": "MINORISTA",
    "telefono": "70000000", "direccion": "Direccion de prueba",
    "identificacionFiscal": f"100{sello}", "condicionHabitual": "CONTADO"})
cliente_id = (cliente or {}).get("id")
paso(5, "Crear un cliente", estado in (200, 201) and bool(cliente_id), f"HTTP {estado}")

estado, cotizacion = llamar("POST", "/cotizaciones", {
    "clienteId": cliente_id, "canal": "LOCAL", "vigenteHasta": fecha(7),
    "detalles": [{"productoId": producto_id, "cantidad": 4,
                  "unidad": "kg", "precioUnitario": 35.00}]})
cotizacion_id = (cotizacion or {}).get("id")
paso(6, "Crear cotizacion de 4 kg a Bs 35",
     estado in (200, 201) and bool(cotizacion_id), f"HTTP {estado}")

estado, _ = llamar("POST", f"/cotizaciones/{cotizacion_id}/presentar")
paso(7, "Presentar la cotizacion", estado in (200, 204), f"HTTP {estado}")

estado, negado = llamar("POST", "/pedidos", {
    "cotizacionId": cotizacion_id, "fechaComprometida": fecha(5), "condicionPago": "CONTADO"})
codigo = (negado or {}).get("codigo") if isinstance(negado, dict) else None
paso(8, "CRITERIO: el pedido se niega si la cotizacion no esta aceptada",
     estado >= 400, f"HTTP {estado} {codigo or ''}".strip())

estado, _ = llamar("POST", f"/cotizaciones/{cotizacion_id}/aceptar")
paso(9, "Aceptar la cotizacion", estado in (200, 204), f"HTTP {estado}")

estado, pedido = llamar("POST", "/pedidos", {
    "cotizacionId": cotizacion_id, "fechaComprometida": fecha(5), "condicionPago": "CONTADO"})
pedido_id = (pedido or {}).get("id")
paso(10, "Confirmar el pedido sobre la cotizacion aceptada",
     estado in (200, 201) and bool(pedido_id), f"HTTP {estado}")

estado, detalle = llamar("GET", f"/pedidos/{pedido_id}")
margen = (detalle or {}).get("margenEstimadoConfirmado")
costo = (detalle or {}).get("costoEstimadoConfirmado")
paso(11, "CRITERIO: queda costo Bs 112,00 y margen Bs 28,00",
     estado == 200
     and margen is not None and abs(float(margen) - 28.0) < 0.01
     and costo is not None and abs(float(costo) - 112.0) < 0.01,
     f"costo={costo} margen={margen}")

estado, _ = llamar("POST", f"/productos/{producto_id}/precios", {
    "costoReferencia": 28.00, "precioVenta": 40.00, "vigenteDesde": fecha(1)})
paso(12, "Registrar una vigencia nueva de Bs 40", estado in (200, 201), f"HTTP {estado}")

estado, relectura = llamar("GET", f"/pedidos/{pedido_id}")
margen_final = (relectura or {}).get("margenEstimadoConfirmado")
paso(13, "CRITERIO: el margen sigue en Bs 28,00, no se recalcula",
     estado == 200 and margen_final is not None and abs(float(margen_final) - 28.0) < 0.01,
     f"margen={margen_final}")

guardado, token = token, None
estado, _ = llamar("GET", "/pedidos")
token = guardado
paso(14, "Sin token la API responde 401", estado == 401, f"HTTP {estado}")

print("=" * 72)
print(f"  {correctas} correctas, {len(fallas)} fallas")
for linea in fallas:
    print("   -", linea)
print()
sys.exit(0 if not fallas else 1)
