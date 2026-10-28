# Lord of the Bow — Figma UI Builder

Figma MCP на Starter-плане исчерпан (лимит ~6 вызовов/мес). Этот плагин собирает весь UI в уже созданном файле одним запуском.

## Файл

https://www.figma.com/design/OpL4b9Y2znjSRqc4R2Btub

## Установка

1. Открой файл выше в **Figma Desktop**.
2. Установи шрифты игры (или Google Fonts): **Climate Crisis**, **Jura** (Regular / Medium / SemiBold / Bold).
3. Меню: `Plugins` → `Development` → `Import plugin from manifest…`
4. Выбери `Design/FigmaUIPlugin/manifest.json`.
5. Запусти плагин: `Plugins` → `Development` → `Lord of the Bow — Build UI`.

Плагин идемпотентен по смыслу: перед сборкой чистит страницы `00 Foundations` / `01 Components` / `02 Screens` / `03 States` если они уже есть, затем создаёт заново.

## Результат

- Foundations: цвета, spacing, radius, text styles
- Components: кнопки, панели, карточки, HUD
- Screens @ 1080×1920: PreStart, Shop, HUD, Pause, Upgrade, Win, Lose, Loading
- States: equipped / muted / cooldown / ad
