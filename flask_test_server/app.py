from __future__ import annotations

import os
import sqlite3
from datetime import datetime
from functools import wraps
from pathlib import Path

from flask import Flask, jsonify, redirect, render_template_string, request, url_for
from werkzeug.security import check_password_hash, generate_password_hash

APP_DIR = Path(__file__).resolve().parent
DB_PATH = APP_DIR / "auth_test.db"
VALID_STATUSES = {"Pending", "Approved", "Rejected", "Disabled"}
VALID_ROLES = {"Operator", "Admin"}

app = Flask(__name__)


def utc_now() -> str:
    return datetime.utcnow().isoformat(timespec="seconds") + "Z"


def get_db() -> sqlite3.Connection:
    connection = sqlite3.connect(DB_PATH)
    connection.row_factory = sqlite3.Row
    return connection


def init_db() -> None:
    with get_db() as db:
        db.execute(
            """
            CREATE TABLE IF NOT EXISTS users (
                user_id TEXT PRIMARY KEY,
                password_hash TEXT NOT NULL,
                display_name TEXT NOT NULL,
                role TEXT NOT NULL DEFAULT 'Operator',
                approval_status TEXT NOT NULL DEFAULT 'Pending',
                created_at TEXT NOT NULL,
                approved_at TEXT NULL
            );
            """
        )
        db.execute(
            """
            CREATE TABLE IF NOT EXISTS sensor_events (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                received_at TEXT NOT NULL,
                event_type TEXT NOT NULL,
                payload TEXT NOT NULL
            );
            """
        )
        db.execute(
            """
            CREATE TABLE IF NOT EXISTS log_events (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                received_at TEXT NOT NULL,
                payload TEXT NOT NULL
            );
            """
        )
        ensure_seed_user(db, "admin", "admin1234", "Test Admin", "Admin", "Approved")
        ensure_seed_user(db, "operator01", "1234", "Operator 01", "Operator", "Approved")
        ensure_seed_user(db, "pending01", "1234", "Pending User", "Operator", "Pending")
        db.commit()


def ensure_seed_user(db: sqlite3.Connection, user_id: str, password: str, display_name: str, role: str, status: str) -> None:
    exists = db.execute("SELECT 1 FROM users WHERE user_id = ?", (user_id,)).fetchone()
    if exists:
        return

    db.execute(
        """
        INSERT INTO users (user_id, password_hash, display_name, role, approval_status, created_at, approved_at)
        VALUES (?, ?, ?, ?, ?, ?, ?)
        """,
        (
            user_id,
            generate_password_hash(password),
            display_name,
            role,
            status,
            utc_now(),
            utc_now() if status == "Approved" else None,
        ),
    )


def user_to_dict(row: sqlite3.Row, success: bool = True, message: str | None = None) -> dict:
    result = {
        "success": success,
        "userId": row["user_id"],
        "displayName": row["display_name"],
        "role": row["role"],
        "approvalStatus": row["approval_status"],
    }
    if message:
        result["message"] = message
    return result


def require_json(func):
    @wraps(func)
    def wrapper(*args, **kwargs):
        if not request.is_json:
            return jsonify({"success": False, "message": "JSON body is required."}), 400
        return func(*args, **kwargs)

    return wrapper


@app.route("/api/health", methods=["GET"])
def health():
    return jsonify({"status": "ok", "message": "WPF auth test Flask API is reachable."})


@app.route("/api/auth/register", methods=["POST"])
@require_json
def register():
    data = request.get_json() or {}
    user_id = (data.get("userId") or "").strip()
    password = data.get("password") or ""
    display_name = (data.get("displayName") or user_id).strip()

    if not user_id or not password:
        return jsonify({"success": False, "approvalStatus": "Rejected", "message": "userId and password are required."}), 400

    with get_db() as db:
        exists = db.execute("SELECT 1 FROM users WHERE user_id = ?", (user_id,)).fetchone()
        if exists:
            return jsonify({"success": False, "userId": user_id, "approvalStatus": "Rejected", "message": "User already exists."}), 409

        db.execute(
            """
            INSERT INTO users (user_id, password_hash, display_name, role, approval_status, created_at, approved_at)
            VALUES (?, ?, ?, 'Operator', 'Pending', ?, NULL)
            """,
            (user_id, generate_password_hash(password), display_name, utc_now()),
        )
        db.commit()

    return jsonify({
        "success": True,
        "userId": user_id,
        "approvalStatus": "Pending",
        "message": "Registration submitted. Waiting for admin approval.",
    })


@app.route("/api/auth/login", methods=["POST"])
@require_json
def login():
    data = request.get_json() or {}
    user_id = (data.get("userId") or "").strip()
    password = data.get("password") or ""

    with get_db() as db:
        row = db.execute("SELECT * FROM users WHERE user_id = ?", (user_id,)).fetchone()

    if row is None or not check_password_hash(row["password_hash"], password):
        return jsonify({"success": False, "userId": user_id, "approvalStatus": "Rejected", "message": "Invalid userId or password."}), 401

    if row["approval_status"] != "Approved":
        return jsonify(user_to_dict(row, success=False, message=f"Account is {row['approval_status']}.")), 403

    return jsonify(user_to_dict(row, success=True, message="Login success."))


@app.route("/api/users/<user_id>/status", methods=["GET"])
def user_status(user_id: str):
    with get_db() as db:
        row = db.execute("SELECT * FROM users WHERE user_id = ?", (user_id,)).fetchone()

    if row is None:
        return jsonify({"success": False, "userId": user_id, "approvalStatus": "Rejected", "message": "User not found."}), 404

    return jsonify(user_to_dict(row, success=row["approval_status"] == "Approved"))


@app.route("/api/sensor", methods=["POST"])
@require_json
def receive_sensor_event():
    data = request.get_json() or {}
    event_type = data.get("type") or "unknown"
    with get_db() as db:
        db.execute(
            "INSERT INTO sensor_events (received_at, event_type, payload) VALUES (?, ?, ?)",
            (utc_now(), event_type, __import__("json").dumps(data, ensure_ascii=False)),
        )
        db.commit()
    return jsonify({"success": True, "message": "Telemetry received.", "type": event_type})


@app.route("/api/log", methods=["POST"])
@require_json
def receive_log_event():
    data = request.get_json() or {}
    with get_db() as db:
        db.execute(
            "INSERT INTO log_events (received_at, payload) VALUES (?, ?)",
            (utc_now(), __import__("json").dumps(data, ensure_ascii=False)),
        )
        db.commit()
    return jsonify({"success": True, "message": "Log received."})


@app.route("/api/admin/log-events", methods=["GET"])
def admin_log_events():
    limit = int(request.args.get("limit", "100"))
    with get_db() as db:
        rows = db.execute(
            "SELECT id, received_at, payload FROM log_events ORDER BY id DESC LIMIT ?",
            (limit,),
        ).fetchall()
    return jsonify({"success": True, "logs": [dict(row) for row in rows]})


@app.route("/api/admin/sensor-events", methods=["GET"])
def admin_sensor_events():
    limit = int(request.args.get("limit", "100"))
    with get_db() as db:
        rows = db.execute(
            "SELECT id, received_at, event_type, payload FROM sensor_events ORDER BY id DESC LIMIT ?",
            (limit,),
        ).fetchall()
    return jsonify({"success": True, "events": [dict(row) for row in rows]})


@app.route("/api/admin/users", methods=["GET"])
def admin_users():
    with get_db() as db:
        rows = db.execute("SELECT user_id, display_name, role, approval_status, created_at, approved_at FROM users ORDER BY created_at DESC").fetchall()
    return jsonify({"success": True, "users": [dict(row) for row in rows]})


@app.route("/api/admin/users/<user_id>/approve", methods=["POST"])
def approve_user(user_id: str):
    return update_user(user_id, status="Approved")


@app.route("/api/admin/users/<user_id>/reject", methods=["POST"])
def reject_user(user_id: str):
    return update_user(user_id, status="Rejected")


@app.route("/api/admin/users/<user_id>/role", methods=["POST"])
@require_json
def update_role(user_id: str):
    data = request.get_json() or {}
    role = (data.get("role") or "").strip()
    if role not in VALID_ROLES:
        return jsonify({"success": False, "message": "role must be Operator or Admin."}), 400
    return update_user(user_id, role=role)


def update_user(user_id: str, status: str | None = None, role: str | None = None):
    if status is not None and status not in VALID_STATUSES:
        return jsonify({"success": False, "message": "invalid approval status."}), 400
    if role is not None and role not in VALID_ROLES:
        return jsonify({"success": False, "message": "invalid role."}), 400

    with get_db() as db:
        row = db.execute("SELECT * FROM users WHERE user_id = ?", (user_id,)).fetchone()
        if row is None:
            return jsonify({"success": False, "userId": user_id, "message": "User not found."}), 404

        new_status = status or row["approval_status"]
        new_role = role or row["role"]
        approved_at = utc_now() if new_status == "Approved" else row["approved_at"]
        db.execute(
            "UPDATE users SET approval_status = ?, role = ?, approved_at = ? WHERE user_id = ?",
            (new_status, new_role, approved_at, user_id),
        )
        db.commit()
        row = db.execute("SELECT * FROM users WHERE user_id = ?", (user_id,)).fetchone()

    return jsonify(user_to_dict(row, success=True, message="User updated."))


ADMIN_PAGE = """
<!doctype html>
<html lang="ko">
<head>
  <meta charset="utf-8">
  <title>WPF Auth Test Admin</title>
  <style>
    body { font-family: Segoe UI, Arial, sans-serif; margin: 32px; color: #1f2937; }
    table { border-collapse: collapse; width: 100%; margin-top: 16px; }
    th, td { border: 1px solid #d1d5db; padding: 8px 10px; text-align: left; }
    th { background: #f3f4f6; }
    form { display: inline; }
    button { margin: 2px; padding: 5px 9px; cursor: pointer; }
    .Approved { color: #047857; font-weight: 700; }
    .Pending { color: #b45309; font-weight: 700; }
    .Rejected, .Disabled { color: #b91c1c; font-weight: 700; }
    code { background: #f3f4f6; padding: 2px 4px; }
  </style>
</head>
<body>
  <h1>WPF Auth Test Admin</h1>
  <p>WPF Settings의 Flask API URL: <code>http://서버IP:5000</code></p>
  <p>기본 계정: <code>operator01 / 1234</code>, <code>admin / admin1234</code>, <code>pending01 / 1234</code></p>
  <table>
    <thead><tr><th>User ID</th><th>Name</th><th>Role</th><th>Status</th><th>Created</th><th>Action</th></tr></thead>
    <tbody>
      {% for user in users %}
      <tr>
        <td>{{ user.user_id }}</td>
        <td>{{ user.display_name }}</td>
        <td>{{ user.role }}</td>
        <td class="{{ user.approval_status }}">{{ user.approval_status }}</td>
        <td>{{ user.created_at }}</td>
        <td>
          <form method="post" action="/admin/users/{{ user.user_id }}/approve"><button>Approve</button></form>
          <form method="post" action="/admin/users/{{ user.user_id }}/reject"><button>Reject</button></form>
          <form method="post" action="/admin/users/{{ user.user_id }}/role/admin"><button>Make Admin</button></form>
          <form method="post" action="/admin/users/{{ user.user_id }}/role/operator"><button>Make Operator</button></form>
        </td>
      </tr>
      {% endfor %}
    </tbody>
  </table>
</body>
</html>
"""


@app.route("/admin", methods=["GET"])
def admin_page():
    with get_db() as db:
        users = db.execute("SELECT user_id, display_name, role, approval_status, created_at FROM users ORDER BY created_at DESC").fetchall()
    return render_template_string(ADMIN_PAGE, users=users)


@app.route("/admin/users/<user_id>/approve", methods=["POST"])
def admin_page_approve(user_id: str):
    update_user(user_id, status="Approved")
    return redirect(url_for("admin_page"))


@app.route("/admin/users/<user_id>/reject", methods=["POST"])
def admin_page_reject(user_id: str):
    update_user(user_id, status="Rejected")
    return redirect(url_for("admin_page"))


@app.route("/admin/users/<user_id>/role/<role>", methods=["POST"])
def admin_page_role(user_id: str, role: str):
    update_user(user_id, role="Admin" if role.lower() == "admin" else "Operator")
    return redirect(url_for("admin_page"))


if __name__ == "__main__":
    init_db()
    port = int(os.environ.get("PORT", "5000"))
    app.run(host="0.0.0.0", port=port, debug=True)
else:
    init_db()
