# The Fish Dimension

A small, silly mod for Stardew Valley that forces every randomly-generated Help
Wanted quest to be about fish.

## How it Works

Any generated quest that isn't a `FishingQuest` or an `ItemDeliveryQuest` will
be reassigned to one of those types (in the same proportion that they are
generated by default, about 1:4). All `ItemDeliveryQuest`s will choose randomly
from available fish instead of whatever rules they might normally follow.

The fish selection for `FishingQuest`s is not changed, but for
`ItemDeliveryQuest`s, the new code makes extensive use of context tags, so
modded fish should work out of the box if they are tagged correctly. Here's
what is checked on the entries in `Data/Objects`:

- must be in the fish category (-4, i.e. actual fish)
- must have the tag `season_<current season>` OR `season_all`
- must not have the tag `fish_mines` (mine fish are excluded normally)
- must not have the tag `fish_legendary`
- if the bus is not repaired, must not have the tag `fish_desert`
- 
