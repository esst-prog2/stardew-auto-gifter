---
name: Auto-Gifter
description: One night seen through a farmhouse window, brightening into the morning the gifts are handed over.
colors:
  night: "#1c1f4a"
  night-2: "#232760"
  dusk: "#2e3478"
  first-light: "#4f62b2"
  pre-dawn: "#5b6fbc"
  morning: "#8fd3ff"
  morning-2: "#c9ecff"
  day: "#ffffff"
  lamp: "#ffb627"
  lamp-ink: "#1c1f4a"
  pane-dark: "#2b2f6b"
  pane-dark-ink: "#b9bdf0"
  frame: "#13153a"
  sash: "#3d4291"
  sash-edge: "#4c52a6"
  sill: "#565cb4"
  outline: "#7d83cc"
  ink-night: "#ffffff"
  ink-night-soft: "#d6d9ff"
  ink-day: "#1c1f4a"
  ink-day-soft: "#313670"
typography:
  display:
    fontFamily: "Bricolage Grotesque, Atkinson Hyperlegible, sans-serif"
    fontSize: "clamp(3rem, 7.6vw, 6rem)"
    fontWeight: 800
    lineHeight: 0.92
    letterSpacing: "-0.04em"
  headline:
    fontFamily: "Bricolage Grotesque, Atkinson Hyperlegible, sans-serif"
    fontSize: "clamp(2.2rem, 4.4vw, 4rem)"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "-0.03em"
  title:
    fontFamily: "Bricolage Grotesque, Atkinson Hyperlegible, sans-serif"
    fontSize: "clamp(1.3rem, 2vw, 1.7rem)"
    fontWeight: 700
    letterSpacing: "-0.015em"
  action:
    fontFamily: "Bricolage Grotesque, Atkinson Hyperlegible, sans-serif"
    fontSize: "clamp(1.5rem, 2.6vw, 2.4rem)"
    fontWeight: 800
    letterSpacing: "-0.02em"
  body:
    fontFamily: "Atkinson Hyperlegible, system-ui, sans-serif"
    fontSize: "clamp(17px, 1.15vw, 19px)"
    fontWeight: 400
    lineHeight: 1.55
  pane:
    fontFamily: "Atkinson Hyperlegible, system-ui, sans-serif"
    fontSize: "clamp(1rem, 1.3vw, 1.2rem)"
    fontWeight: 700
  label:
    fontFamily: "Atkinson Hyperlegible, system-ui, sans-serif"
    fontSize: "0.85rem"
    fontWeight: 400
    letterSpacing: "0.02em"
rounded:
  pane: "3px"
  focus: "4px"
  frame: "6px"
spacing:
  gutter: "clamp(20px, 5vw, 72px)"
  step: "clamp(64px, 10vw, 128px)"
  pane-pad: "clamp(14px, 1.6vw, 22px)"
  frame-pad: "clamp(12px, 1.4vw, 18px)"
  mullion: "clamp(8px, 0.9vw, 12px)"
components:
  window:
    backgroundColor: "{colors.sash}"
    rounded: "{rounded.frame}"
    padding: "{spacing.frame-pad}"
  pane-lit:
    backgroundColor: "{colors.lamp}"
    textColor: "{colors.lamp-ink}"
    typography: "{typography.pane}"
    rounded: "{rounded.pane}"
    padding: "{spacing.pane-pad}"
  pane-dark:
    backgroundColor: "{colors.pane-dark}"
    textColor: "{colors.pane-dark-ink}"
    typography: "{typography.pane}"
    rounded: "{rounded.pane}"
    padding: "{spacing.pane-pad}"
  pane-out:
    textColor: "{colors.ink-night-soft}"
    typography: "{typography.pane}"
    rounded: "{rounded.pane}"
    padding: "{spacing.pane-pad}"
  pane-action:
    backgroundColor: "{colors.day}"
    textColor: "{colors.ink-day}"
    typography: "{typography.action}"
    rounded: "{rounded.pane}"
    padding: "clamp(18px, 2vw, 28px) clamp(18px, 2.2vw, 30px)"
  pane-action-hover:
    backgroundColor: "{colors.lamp}"
    textColor: "{colors.ink-day}"
  nav-link:
    textColor: "{colors.ink-night-soft}"
    typography: "{typography.body}"
  nav-link-hover:
    textColor: "{colors.ink-night}"
---

# Design System: Auto-Gifter

## Overview

**Creative North Star: "The Farmhouse Window at Night"**

The whole page is one night seen through a window. It opens in deep indigo dark, where tomorrow is planned, and the sky brightens section by section until the last band is full daylight. Every piece of structured content sits inside a window: a painted sash frame with mullion gaps between panes and a sill underneath. A pane's state is its light. Lit amber means planned, dark indigo means skipped or off, and a dashed outline means not available yet. The same three states recur as small square marks beside list items.

Density is low and type is large. A heavy warm grotesk carries the headlines and the primary action. A hyperlegible body face carries everything else at a generous 17 to 19px. There is no game art, no pixel look, and no imagery. The light itself is the illustration.

**Key Characteristics:**
- A single sky gradient running from night at the top to white day at the bottom, with no seams between sections.
- Window panes as the only containers. There are no cards and no boxes outside a window frame, apart from the dashed placeholder for missing media.
- Pane state shown by brightness: lit, dark, outlined.
- Amber lamp light as the one warm accent, also used for text selection and the focus ring on dark ground.
- One scroll-driven moment: a dark pane lights up as it comes into view.

## Colors

A cool indigo-to-sky ramp with a single warm amber lamp.

### Primary
- **Lamp Amber** (lamp): the lit pane, the planned state, the hover of the primary action, the skip link, text selection, and the focus ring on dark ground. It is light in a dark room, so it appears only where something is planned or actionable.
- **Lamp Ink** (lamp-ink): text on amber. It is the night indigo, so lit panes read as night ink on lamplight.

### Secondary
- **Morning Sky** (morning): the destination of the sky gradient, the third band's ground, and the requirements line in the hero.
- **Pale Morning** (morning-2): the fourth band's ground, the step before full day.

### Neutral
- **Night Indigo** (night): page ground, the top of the sky, the theme color, and day-ground text ink.
- **Late Night** (night-2) and **Dusk** (dusk): intermediate stops in the first band's gradient.
- **First Light** (first-light) and **Pre-Dawn** (pre-dawn): mid-sky stops where the first and second bands meet and hand over.
- **Window Frame** (frame): the deepest indigo, used for the scrollbar track.
- **Sash Blue** (sash), **Sash Edge** (sash-edge), **Sill** (sill): the painted window frame, its inner bevel line, and the ledge beneath.
- **Dark Pane** (pane-dark): unlit pane and the dark state mark.
- **Unlit Lavender** (pane-dark-ink): text on a dark pane, the dim counterpart of lamp ink.
- **Dusk Outline** (outline): dashed borders for the not-yet state and placeholders, the scrollbar thumb, the inset ring on dark marks, and the separator dot in the requirements line.
- **Day White** (day): the primary action pane and the last band's ground.
- **Night Ink** (ink-night) and **Soft Night Ink** (ink-night-soft): primary and secondary text on dark ground.
- **Day Ink** (ink-day) and **Soft Day Ink** (ink-day-soft): primary and secondary text on morning and day ground.

### Named Rules
**The One Sky Rule.** Section backgrounds are consecutive stops of one gradient. Each band starts on the exact color the previous one ended on (night, night-2, dusk, first-light, then first-light to pre-dawn, then pre-dawn to morning, morning-2, day). A new section never introduces an unrelated ground.

**The Ink Follows the Sky Rule.** Night ink on the dark bands, day ink from the morning band onward. Where a gradient brightens under soft ink, the text steps up to full night ink (white) to keep contrast.

**The One Lamp Rule.** Amber is the only warm color. It marks planned or actionable things and nothing decorative.

## Typography

**Display Font:** Bricolage Grotesque (self-hosted variable, weights 500 to 800, with Atkinson Hyperlegible and sans-serif as fallback)
**Body Font:** Atkinson Hyperlegible (self-hosted, 400, 700 and 400 italic, with system-ui and sans-serif as fallback)
**Code Font:** the platform monospace stack (ui-monospace, Cascadia Mono, Consolas), at 0.92em on a faint tint

**Character:** A sturdy, slightly quirky grotesk set tight and heavy for headlines, paired with a body face designed for legibility. The headlines feel warm and confident, and the body text reads easily at any size.

### Hierarchy
- **Display** (800, clamp(3rem, 7.6vw, 6rem), line-height 0.92, -0.04em): the hero headline only, balanced and capped at about 16ch.
- **Headline** (800, clamp(2.2rem, 4.4vw, 4rem), line-height 1, -0.03em): section headings, balanced and capped at about 18ch.
- **Title** (700, clamp(1.3rem, 2vw, 1.7rem), -0.015em): step headings inside a section.
- **Action** (800, clamp(1.5rem, 2.6vw, 2.4rem), -0.02em): the label of the primary action pane, in the display face.
- **Body** (400, clamp(17px, 1.15vw, 19px), line-height 1.55): running text, with paragraphs capped at 66ch and lists at 66 to 72ch. The hero lede is a larger body step (clamp(1.05rem, 1.35vw, 1.25rem), 46ch).
- **Pane** (700, clamp(1rem, 1.3vw, 1.2rem)): the content line inside a pane.
- **Label** (400, 0.85 to 0.95rem, 0.02em): the state line above a pane's content, the small meta line inside the action pane, and the requirements line. Sentence case, never uppercase.

### Named Rules
**The Two Faces Rule.** The display face is used for the brand, headings, the action label and list numerals. Everything else is set in the body face.

## Layout

A single full-bleed column of bands. The horizontal gutter is fluid (gutter token) and the vertical rhythm between bands is one step (step token). Later bands shorten the step to 0.75, 0.6 or 0.5 of it as the page settles into day. Text measure is capped rather than gridded.

The hero fills the first viewport (min-height of 100svh minus the top bar) and holds the headline above a five-column window strip: four equal panes and a wider action pane at 1.25fr. The "how a day works" section is two equal columns, with steps on the left and a single-column window of three panes on the right whose rows line up with the steps.

Breakpoints: at 860px and below, the section nav hides, the hero window becomes two columns with the action pane spanning both, and two-column sections stack. At 420px and below, the hero window is a single column.

## Elevation & Depth

Depth is physical and belongs to the window only. The frame casts a long, soft, low shadow onto the night, and the sill casts a smaller shadow of its own. Lit panes glow with amber light. Nothing else on the page is lifted. Text, lists and bands stay flat.

### Shadow Vocabulary
- **Frame cast** (`box-shadow: inset 0 0 0 3px #4c52a6, 0 30px 60px -20px rgba(5, 6, 25, 0.7)`): the window frame. The inset line is the sash bevel.
- **Sill cast** (`box-shadow: 0 10px 18px -8px rgba(5, 6, 25, 0.6)`): the ledge under every window.
- **Lamp glow** (`box-shadow: 0 14px 34px -10px rgba(255, 182, 39, 0.45)`): lit panes only, as light spilling out of the glass.

### Named Rules
**The Only the Window Casts Rule.** Shadows exist only on the window, its sill and its lit panes. Content outside a window has no shadow.

## Shapes

Small, practical corners: 3px on panes and state marks, 4px on focus rings, code and the placeholder, 6px on the window frame. The sill overhangs the frame by 14px on each side. Not-yet states use a 2px dashed outline border. State marks are 22px squares, the same three states as the panes in miniature.

## Components

### Window
A painted sash frame (sash fill, 3px inset bevel, frame cast shadow, 6px corners) holding a CSS grid of panes with mullion-sized gaps, and a sill drawn as a pseudo-element beneath it. The grid adapts: a five-column strip in the hero, one column of three rows beside the steps, and a single inline cell around a lone call to action.

### Panes
- **Lit** (lamp fill, lamp ink, lamp glow): planned, or a gift handed over.
- **Dark** (pane-dark fill, unlit lavender text): skipped or switched off.
- **Outlined** (transparent, 2px dashed outline, soft night ink): not available yet.
- Every pane stacks a label-sized state line above its bold content line, aligned to the bottom of the pane.

### Primary Action Pane
The download link drawn as the brightest pane in the window: day-white fill, day ink, display-face label at action size, and a small body-face meta line ("Pre-release · zip") in soft day ink. On hover it turns lamp amber and rises 2px (`transform: translateY(-2px)`) over 180ms with `cubic-bezier(.2,.8,.2,1)`, which also eases the background.

### Navigation
The top bar shows the brand wordmark (display face, 800, 1.25rem) on the left and the section links on the right, without underlines and in soft night ink. On hover the link turns full night ink and gets an underline. The links hide at 860px and below.

### Links and Focus
Inline links inherit color and get a 2px underline offset by 0.22em. The keyboard focus ring is a 3px solid outline offset by 4px with 4px corners, amber on dark ground and night indigo on the morning and day bands, where amber would disappear. Text selection is amber with night ink. A skip link in amber appears on focus.

### State List
A list where each item is led by a 22px square state mark (lit, dark with an inset outline ring, or dashed), followed by a bold lead phrase and soft-ink explanation.

### Placeholder
Missing media is shown as an honest dashed frame (2px dashed outline, 4px corners) with a bold title and one line of description, capped at 66ch. It is the only bordered box outside a window.

### Dawn (signature motion)
As the day section scrolls into view, the answer pane in its window animates from the dark pane state to the lit state (`animation-timeline: view()`, range `entry 20%` to `cover 45%`, linear). It runs only when `animation-timeline` is supported and the user has no reduced-motion preference. Otherwise the pane is simply shown lit with its lamp glow. Smooth anchor scrolling is likewise turned off under reduced motion.

## Do's and Don'ts

### Do:
- **Do** put structured content in panes inside a window frame, with state shown by brightness: lit for planned, dark for skipped, dashed for not yet.
- **Do** continue the sky: start each new band on the color the previous one ended on.
- **Do** switch to day ink and night-colored focus rings once the ground is morning or lighter.
- **Do** keep amber for planned or actionable things, selection and focus on dark ground.
- **Do** keep motion tied to the night-to-morning story and give it a static, fully legible reduced-motion state.
- **Do** self-host both faces and set the display face at 700 to 800 with negative tracking.

### Don't:
- **Don't** use cards, tiles or boxed sections outside the window grid.
- **Don't** use game art, logos, pixel fonts or a pixel look.
- **Don't** add a second warm accent or a decorative gradient that is not a stop of the sky.
- **Don't** put shadows on anything but the window, its sill and its lit panes.
- **Don't** set state labels in uppercase or use them as decorative kickers above headings. They belong inside panes.
