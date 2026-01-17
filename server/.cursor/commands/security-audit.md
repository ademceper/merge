---
title: Security Audit
description: Performs OWASP Top 10 security audit on current code
---

Perform comprehensive security audit:

## OWASP Top 10 Checklist

### A01:2021 - Broken Access Control
- [ ] All endpoints have [Authorize]
- [ ] Resource ownership validated
- [ ] CORS properly configured
- [ ] No IDOR vulnerabilities

```csharp
// Check ownership
var order = await _repository.GetByIdAsync(id, ct);
if (order.UserId != _currentUser.Id)
    throw new ForbiddenException();
```

### A02:2021 - Cryptographic Failures
- [ ] Passwords hashed (BCrypt/Argon2)
- [ ] Sensitive data encrypted at rest
- [ ] HTTPS enforced
- [ ] No hardcoded secrets

### A03:2021 - Injection
- [ ] Parameterized queries used
- [ ] No raw SQL with user input
- [ ] Input validation on all endpoints

```csharp
// ✅ Safe
_context.Products.Where(p => p.Name == name);

// ❌ SQL Injection
_context.Database.ExecuteSqlRaw($"SELECT * FROM Products WHERE Name = '{name}'");
```

### A04:2021 - Insecure Design
- [ ] Rate limiting implemented
- [ ] Input size limits
- [ ] Business logic validation

### A05:2021 - Security Misconfiguration
- [ ] Debug disabled in production
- [ ] Error details hidden
- [ ] Security headers configured
- [ ] Default credentials changed

### A06:2021 - Vulnerable Components
- [ ] Dependencies up to date
- [ ] No known vulnerabilities

```bash
dotnet list package --vulnerable
```

### A07:2021 - Auth Failures
- [ ] Strong password policy
- [ ] Account lockout
- [ ] MFA available
- [ ] Secure session management

### A08:2021 - Data Integrity
- [ ] Critical operations logged
- [ ] Digital signatures verified
- [ ] Update mechanisms secure

### A09:2021 - Logging Failures
- [ ] Security events logged
- [ ] No sensitive data in logs
- [ ] Log injection prevented

### A10:2021 - SSRF
- [ ] URL validation
- [ ] No user-controlled URLs to internal services

## Quick Scan Commands

```bash
# Find hardcoded secrets
grep -rn "password\s*=" --include="*.cs" .
grep -rn "apikey\s*=" --include="*.cs" .
grep -rn "secret\s*=" --include="*.cs" .

# Find raw SQL
grep -rn "ExecuteSqlRaw\|FromSqlRaw" --include="*.cs" .

# Find missing authorization
grep -rn "\[HttpGet\]\|\[HttpPost\]" --include="*.cs" . | grep -v "\[Authorize\]"
```

Report vulnerabilities with severity: Critical, High, Medium, Low
