# Cursor IDE Configuration Guide

Bu klasör Cursor IDE için optimize edilmiş konfigürasyonları içerir.

## 📁 Dosya Yapısı

```
.cursor/
├── settings.json       # Cursor IDE ayarları
├── README.md           # Bu dosya
└── rules/              # MDC rule dosyaları
    ├── core.mdc        # Temel kurallar (always apply)
    ├── workflow.mdc    # Workflow best practices (always apply)
    ├── security.mdc    # Security rules (always apply)
    ├── nextjs.mdc      # Next.js kuralları (apps/web/**)
    ├── expo.mdc        # Expo kuralları (apps/mobile/**)
    ├── components.mdc  # Component patterns (packages/ui/**, packages/uim/**)
    ├── testing.mdc     # Test kuralları (**/__tests__/**)
    ├── api.mdc         # API & state (packages/api/**, packages/store/**)
    └── i18n.mdc        # i18n kuralları (packages/i18n/**)
```

## 🚀 Kullanım

### Plan Mode (ÖNEMLİ!)
Her non-trivial feature öncesi:
1. `Shift+Tab` - Plan Mode'a gir
2. Agent plan oluştursun
3. Plan'ı düzenle (gerekirse)
4. Onayladıktan sonra execute

### Context Referansları
```
@packages/ui/src/components/button.tsx  # Spesifik dosya
@apps/web/                              # Klasör
@docs                                   # İndexlenmiş dokümantasyon
@codebase                               # Tüm codebase ara
@web                                    # Web araması
@core.mdc                               # Spesifik rule
```

### Doküman İndexleme
Cursor Settings → Features → Docs'a ekle:
- https://react.dev
- https://nextjs.org/docs
- https://docs.expo.dev
- https://tanstack.com/query
- https://tailwindcss.com/docs

## 📋 Rule Tipleri

| Tip | Dosya | Açıklama |
|-----|-------|----------|
| **Always Apply** | `core.mdc`, `workflow.mdc`, `security.mdc` | Her conversation'da aktif |
| **Glob Match** | `nextjs.mdc`, `expo.mdc`, etc. | İlgili dosyalarda aktif |
| **Manual** | `@rule-name` ile | Manuel çağırılır |

## ⌨️ Keyboard Shortcuts

| Shortcut | İşlev |
|----------|-------|
| `Cmd+L` | Chat aç |
| `Cmd+K` | Inline edit |
| `Cmd+Shift+L` | Seçimi chat'e ekle |
| `Shift+Tab` | Plan Mode toggle |
| `Escape` | Agent'ı durdur |
| `Tab` | Öneriyi kabul et |

## 💡 Pro Tips

1. **Agent yanlış giderse**: `Escape` bas, yönlendir

2. **Uzun konuşmalarda**: "Remember the rules" veya `@core.mdc` yaz

3. **Verification loop**: Her büyük değişiklikten sonra:
   - `pnpm typecheck`
   - `pnpm lint`
   - `pnpm test`

4. **Ask vs Agent Mode**:
   - **Ask**: Anlama, sorgulama
   - **Agent**: Uygulama, kod yazma

5. **Rule optimization**: 500 satır altında tut, spesifik ol

## 🔧 Troubleshooting

### Rules yüklenmiyor
1. Cursor'u yeniden başlat
2. `.cursor/rules/` klasörünü kontrol et
3. MDC syntax hatalarını kontrol et

### Agent çok fazla dosya değiştiriyor
1. Daha spesifik prompt yaz
2. Plan mode kullan
3. Scope'u daralt: "Only modify X file"

### Context window doluyor
1. `/clear` ile temizle
2. Daha küçük task'lara böl
3. Gereksiz dosyaları referans verme
