# Claude Code Configuration Guide

Bu klasör Claude Code için optimize edilmiş konfigürasyonları içerir.

## 📁 Dosya Yapısı

```
.claude/
├── CLAUDE.md           # Ana proje kuralları (her session'da yüklenir)
├── settings.json       # İzinler + Hooks konfigürasyonu
├── README.md           # Bu dosya
└── commands/           # Custom slash commands
    ├── review.md       # /project:review
    ├── component.md    # /project:component [name]
    ├── test.md         # /project:test
    ├── fix-issue.md    # /project:fix-issue [number]
    ├── catchup.md      # /project:catchup
    ├── mobile-screen.md # /project:mobile-screen [name]
    └── api-endpoint.md # /project:api-endpoint [name]
```

## 🚀 Kullanım

### Custom Commands
```bash
/project:review              # Kod review yap
/project:component Button    # Yeni component oluştur
/project:test                # Testleri çalıştır ve düzelt
/project:fix-issue 123       # GitHub issue'ı çöz
/project:catchup             # /clear sonrası context yükle
/project:mobile-screen Cart  # Yeni mobile screen
/project:api-endpoint products # API entegrasyonu
```

### Hooks (Otomatik)
Konfigüre edilmiş hooks:

| Hook | Tetikleyici | İşlev |
|------|-------------|-------|
| **PreToolUse (Bash)** | Her bash komutu | Tehlikeli komutları engeller |
| **PreToolUse (Edit/Write)** | Dosya değişikliği | Sensitive dosyaları korur |
| **PostToolUse (Edit/Write)** | TS/TSX dosya düzenleme | Otomatik Prettier format |
| **Stop** | Task tamamlandığında | Completion notification |

### Engellenen Tehlikeli Komutlar
- `rm -rf /`, `rm -rf ~`
- `git push --force`
- `git reset --hard`
- `DROP TABLE`, `DELETE FROM`
- `sudo` komutları

### Korunan Dosyalar
- `.env`, `.env.local`
- `credentials.json`
- `secrets/` klasörü

## 🔧 MCP Servers

`.mcp.json` dosyasında konfigüre:
- **filesystem**: Dosya sistemi erişimi
- **memory**: Persistent memory (session arası)
- **github**: GitHub API (token gerekli)

## 💡 Pro Tips

1. **Context temizleme**: Uzun session'larda `/clear` kullan, sonra `/project:catchup`

2. **Instruction ekleme**: Çalışırken `#` tuşuna bas, Claude otomatik CLAUDE.md'ye ekler

3. **Subagents**: Karmaşık görevlerde "use a subagent to verify this" de

4. **Git worktrees**: Paralel çalışma için `git worktree add` kullan

5. **Headless mode**: CI/CD için `claude -p "task"` kullan

## 📊 Best Practices

1. **CLAUDE.md'yi kısa tut** - 60 satır ideal, max 150
2. **Linter kurallarını koyma** - ESLint/Prettier kullan
3. **Detayları ayrı dosyalarda tut** - Progressive disclosure
4. **Her commit'te güncelle** - Team sync için
