# Design brief: Auto-Gifter promo page

Made with Impeccable `shape` on 2026-11-13. Approved by the user. Content source: `docs/designs/promo-page.md`. Product record: `PRODUCT.md`.

## 1. Job and audience

- Stardew Valley players browsing for quality-of-life mods (Nexus-style), on desktop or phone, deciding in seconds.
- Mode: Persuade. They must understand what the mod does, trust it with their items, and try it.

## 2. Outcome and proof

- Primary action: **Download v0.1.0**, linking to https://github.com/esst-prog2/stardew-auto-gifter/releases/tag/v0.1.0 (pre-release). Secondary: Source on GitHub.
- Success: from the first viewport alone a player can say "you plan tonight, and tomorrow they get their gift when you walk past", and finds the download without scrolling.
- Proof: only the code-verified texts ("[x] Haley -> Coconut", "Emily: not met yet - talk to them first", "Gave Coconut to Haley.", "Auto-Gifter Chest") and the facts in `docs/designs/promo-page.md`. No invented screenshots, reviews or numbers.

## 3. Selected direction: Night Before, Morning After

Code-led (no image generation was available). Chosen from a re-rolled hand on the Impeccable decision page; seed reference `seed:e01f58c13c3de347`.

- **World:** the page is seen through a farmhouse window. Scrolling is one night: it starts in deep indigo (tonight, planning) and ends in morning sky (tomorrow, the walk). Night `#1c1f4a`, lamp amber `#ffb627`, morning `#8fd3ff`, white.
- **Panes carry state:** a lit amber pane is planned, a dark pane is skipped, a dashed outline is not met yet. The brightest pane is the download.
- **Raises carried over from declined directions:**
  - one vertical time axis, evening at the top and the walk at the bottom;
  - one continuous field from night to morning, with no boxed card sections;
  - each section poses tonight on the left and answers with tomorrow on the right;
  - state is brightness, not boxes or badges.
- **First viewport:** the brand and the headline "Walk past. They get their gift." on the left, with a one-sentence subline and the requirements line (Stardew Valley 1.6.14+ · SMAPI 4.5+ · single player). On the right is the window: three panes hold tomorrow's rows, one pane shows "Gave Coconut to Haley.", and a wide bright pane is Download v0.1.0.
- **Signature moment:** as you scroll from night into morning, the lit panes turn into the morning messages. The plan becomes the delivery.

## 4. Scope and boundaries

- One static page in `docs/` (HTML and CSS, with an optional few lines of JS), published with GitHub Pages. Production fidelity, a single surface.
- The seven sections from the design doc, in order:
  1. hero;
  2. how a day works;
  3. your rare items stay safe;
  4. how it compares;
  5. install and first day;
  6. known limits;
  7. how it's built.
- **Anti-goals:**
  - looking like a tech or SaaS product page;
  - imitating Stardew Valley's art, logo, fonts or pixel look;
  - a cozy cream-and-lamplight cliché (amber stays inside the lit panes);
  - any claim the design doc does not source.

## 5. States and ranges

- Plan rows come in three states: planned, skipped, not met yet.
- The hero clip and the step screenshots are labelled placeholders until December.
- The download pane has two states: live (the Release exists) and a disabled "coming soon" fallback.
- Text must cope with long villager and item names.

## 6. Interaction and layout

- The vertical flow follows the night-to-morning axis.
- At phone width the window collapses to a single column of panes under the headline, with no horizontal scroll.
- Motion: one orchestrated night-to-morning transition tied to scroll. It is off under `prefers-reduced-motion`, and everything stays readable without it.
- Clear focus states, and at least WCAG AA text contrast on every ground.

## 7. Constraints and open decisions

- No build step and no framework. Fonts are self-hosted or system fonts; no external font CDN.
- English only. Every quoted in-game text must match the code word for word.
- Open:
  - the December media (hero clip, step screenshots);
  - whether GitHub Pages can be enabled on the `esst-prog2` repository;
  - the five minor findings from the design-doc review;
  - the blocklist timing gap (the page copy already states it).
