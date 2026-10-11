# Expected-value source: daily/weekly gift limit

## Source

Stardew Valley Wiki, "Friendship" page: <https://stardewvalleywiki.com/Friendship>
Accessed: 2026-10-11

## Quote

> one gift per day, up to two gifts per week

## Expected values derived from this quote

These expected values are taken from the wiki quote above, not from running our own program:

- `GiftsToday = 1` → no gift can be given (the daily limit is already used).
- `GiftsToday = 0` and `GiftsThisWeek = 2` → no gift can be given (the weekly limit is already used).
- `GiftsToday = 0` and `GiftsThisWeek = 1` → a gift can be given.

## Related test specification

A test would go red if the mod handed a gift to a villager who has already received a gift today (not a spouse, not on their birthday).

## Not covered

Per the same wiki page, a birthday gift can always be given (it doesn't count against the daily/weekly limit), and a spouse receives one gift per day. Neither case is examined by these tests.
