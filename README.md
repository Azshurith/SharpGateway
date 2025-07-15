# 💳 SharpGateway — Authorize.Net Payment Gateway API (ASP.NET Core)

**SharpGateway** is a secure, modular, and production-ready REST API built with ASP.NET Core for integrating [Authorize.Net](https://developer.authorize.net/) payments. It supports full transaction workflows — including authorization, capture, refund, void, and safe structured logging.

---

## ⚙️ Features

- 🔐 Credit card **Authorization Only** & **Auth + Capture**
- 🔁 **Capture** of previously authorized transactions
- 💸 **Refunds** & **Voids**
- 📑 Swagger (OpenAPI) support for easy testing
- 🛡️ Secure request logging (card number, CVV, expiration masked)
- 🔄 Environment-based config using `.env`
- 📦 Clean separation of services, models, and controllers

---

## 📁 API Endpoints

| Method | Endpoint          | Description                             |
|--------|-------------------|-----------------------------------------|
| POST   | `/api/Authorize`  | Authorize card without capturing funds |
| POST   | `/api/Charge`     | Authorize + Capture in one step        |
| POST   | `/api/Capture`    | Capture a previously authorized trans. |
| POST   | `/api/Refund`     | Refund a previously settled transaction |
| POST   | `/api/Void`       | Void an unsettled (auth-only) transaction |

---

## 📦 Tech Stack

- **ASP.NET Core 8.0**
- **Authorize.Net SDK v2.0**
- **Swashbuckle (Swagger UI)**
- **DotNetEnv** for `.env` config loading
- **Microsoft.Extensions.Logging** with safe masking

---

## 🔐 Environment Configuration

Your API credentials should be placed in a `.env` file (never commit this!):

```
AUTHORIZENET__APILOGINID=your_login_id
AUTHORIZENET__TRANSACTIONKEY=your_transaction_key
```

> ✅ Automatically mapped to the `AuthorizeNet` section in your config.

---

## 📄 appsettings.json (safe sample)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AuthorizeNet": {
    "ApiLoginId": "",
    "TransactionKey": ""
  },
  "AllowedHosts": "*"
}
```

---

## 🧪 Run the Project

```bash
dotnet restore
dotnet build
dotnet run
```

Navigate to: [https://localhost:5001/swagger](https://localhost:5001/swagger)

---

## 🔒 Request Logging Safety

Sensitive fields like:

- `CardNumber`
- `CVV`
- `Expiration`

...are **never logged in plaintext**. They are masked using secure utilities and safe anonymous logging objects.

---

## 🚀 Future Enhancements

- [ ] Support for recurring billing / customer profiles
- [ ] Webhook listener for Authorize.Net transaction updates
- [ ] Docker containerization
- [ ] Multi-environment deployment setup (dev/staging/prod)

---

## 📝 License

MIT © 2025 — Devitrax

---

## 🤝 Contributions

PRs are welcome! Open an issue or fork this repo to contribute improvements.
