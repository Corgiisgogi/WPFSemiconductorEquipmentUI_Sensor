# WPF Auth 테스트용 Flask 서버

WPF 앱의 Auth API 연동을 테스트하기 위한 최소 Flask 서버입니다.
회원가입/로그인/승인 상태 조회 API와 간단한 웹 관리자 화면을 제공합니다.

## 설치

```powershell
cd flask_test_server
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

## 실행

```powershell
python app.py
```

다른 컴퓨터에서 접속하려면 서버 PC 방화벽에서 TCP 5000 포트를 열고, WPF Settings의 `Flask API URL`에 아래처럼 입력하세요.

```text
http://서버PC_IP:5000
```

서버 PC IP 확인:

```powershell
ipconfig
```

방화벽 열기(관리자 PowerShell):

```powershell
New-NetFirewallRule -DisplayName "Flask API 5000" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

## 테스트 주소

```text
GET http://localhost:5000/api/health
GET http://localhost:5000/admin
```

## 기본 계정

| User ID | Password | Role | Status |
| --- | --- | --- | --- |
| operator01 | 1234 | Operator | Approved |
| admin | admin1234 | Admin | Approved |
| pending01 | 1234 | Operator | Pending |

## WPF가 사용하는 API

### 회원가입

```http
POST /api/auth/register
Content-Type: application/json

{
  "userId": "newuser",
  "password": "1234",
  "displayName": "New User"
}
```

응답:

```json
{
  "success": true,
  "userId": "newuser",
  "approvalStatus": "Pending",
  "message": "Registration submitted. Waiting for admin approval."
}
```

### 로그인

```http
POST /api/auth/login
Content-Type: application/json

{
  "userId": "operator01",
  "password": "1234"
}
```

승인 계정 응답:

```json
{
  "success": true,
  "userId": "operator01",
  "displayName": "Operator 01",
  "role": "Operator",
  "approvalStatus": "Approved",
  "message": "Login success."
}
```

### 승인 상태 조회

```http
GET /api/users/operator01/status
```

응답:

```json
{
  "success": true,
  "userId": "operator01",
  "displayName": "Operator 01",
  "role": "Operator",
  "approvalStatus": "Approved"
}
```

## 간단 웹 관리자

브라우저에서 접속:

```text
http://localhost:5000/admin
```

여기서 회원 승인/거절, Operator/Admin 변경을 테스트할 수 있습니다.
