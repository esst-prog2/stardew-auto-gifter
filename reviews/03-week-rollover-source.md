# Expected-value source: week rollover for the gift limit

This file sits next to `hw5/expected-value-source.md` and extends it. It is written before any test for R1 (HW6 review 03).

## Source

Stardew Valley Wiki, "Friendship" page: <https://stardewvalleywiki.com/Friendship>
The quote was provided by the user on 2026-10-17.

## Quote

> the week begins on Sunday

Together with the HW5 quote from the same page ("one gift per day, up to two gifts per week"), this means:
- the weekly gift count starts again from 0 on Sunday;
- the daily count starts again from 0 every new day.

## How the day of the week is found from the day counter

The mod numbers days with SMAPI's days-since-start count (`SDate.DaysSinceStart`): spring 1 of year 1 is day 1. Every season has 28 days and starts on a Monday. This comes from the game's own calendar, not from the quote above. So:

- Weekday: `(day - 1) % 7`, where 0 = Monday, 1 = Tuesday, …, 5 = Saturday, 6 = Sunday. Day 6 (spring 6) is a Saturday, and day 7 (spring 7) is a Sunday.
- Week number, with weeks beginning on Sunday as the quote says: `day / 7`, using integer division. Days 1-6 (Monday to Saturday of the first, partial week) are week 0, days 7-13 (Sunday to Saturday) are week 1, and so on.
- A target day starts a new week compared with today when `targetDay / 7 != today / 7`. For a plan for tomorrow, that happens exactly when tomorrow is a Sunday.

## Expected values derived from these quotes

These are not taken from running our own program:

- Planning on Saturday evening (today = day 6) for Sunday (target = day 7), with `GiftsThisWeek = 2` and `GiftsToday = 0` → the villager **can** be planned, because Sunday starts a new week.
- Planning on a weekday evening (today = day 2, Tuesday) for the next day (target = day 3, Wednesday, same week), with `GiftsThisWeek = 2` and `GiftsToday = 0` → the villager **cannot** be planned, because the weekly limit is already used.
- Giving today (target = today = day 3), with `GiftsToday = 1` and `GiftsThisWeek = 0` → **no** gift can be given, because the daily limit is already used.

## Not covered

- Birthday gifts, as in HW5.
- Spouses, as in HW5.
- Year boundaries are covered by the formula, since a year is 4 × 28 = 112 days, a multiple of 7, but no test checks them.
